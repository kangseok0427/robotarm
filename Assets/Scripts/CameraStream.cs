using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Threading;

[System.Serializable]
public class CameraFeed
{
    [Header("사용 여부")]
    public bool enabled = true;

    [Header("연결 정보")]
    public string serverIP = "192.168.1.100";
    public int serverPort = 5000;
    public string endpoint = "/video_feed";

    [Header("출력 대상")]
    public Renderer targetScreen;

    [HideInInspector] public Texture2D tex;
    [HideInInspector] public byte[] pending;
    [HideInInspector] public object lockObj = new object();
    [HideInInspector] public Thread thread;
    [HideInInspector] public bool running;

    public string Url => $"http://{serverIP}:{serverPort}{endpoint}";
}

[System.Serializable]
public class CapeCameraSet
{
    [Header("사용 여부")]
    public bool enabled = true;

    [Header("연결 정보")]
    public string serverIP = "192.168.1.100";
    public int serverPort = 5000;
    public string rgbEndpoint = "/video_feed";
    public string depthEndpoint = "/depth_feed";

    [Header("Cape Quad (DepthBlend 셰이더 적용 Renderer)")]
    public Renderer capeScreen;

    [HideInInspector] public Texture2D rgbTex;
    [HideInInspector] public Texture2D depthTex;
    [HideInInspector] public byte[] rgbPending;
    [HideInInspector] public byte[] depthPending;
    [HideInInspector] public object rgbLock = new object();
    [HideInInspector] public object depthLock = new object();
    [HideInInspector] public Thread rgbThread;
    [HideInInspector] public Thread depthThread;
    [HideInInspector] public bool running;

    public string RgbUrl => $"http://{serverIP}:{serverPort}{rgbEndpoint}";
    public string DepthUrl => $"http://{serverIP}:{serverPort}{depthEndpoint}";
}

public class CameraStream : MonoBehaviour
{
    [Header("=== 외부캠 (O.C) ===")]
    public CameraFeed outdoorCam;

    [Header("=== 왼쪽 로봇 카메라 (Cape) ===")]
    public CapeCameraSet leftCape;

    [Header("=== 오른쪽 로봇 카메라 (Cape) ===")]
    public CapeCameraSet rightCape;

    void Start()
    {
        StartSingleFeed(outdoorCam);
        StartCapeFeed(leftCape);
        StartCapeFeed(rightCape);
    }

    void StartSingleFeed(CameraFeed feed)
    {
        if (feed == null || !feed.enabled) return;
        if (feed.targetScreen == null)
        {
            Debug.LogWarning($"[CameraStream] {feed.endpoint} targetScreen 미설정");
            return;
        }
        feed.tex = new Texture2D(2, 2);
        feed.targetScreen.material.mainTexture = feed.tex;
        feed.running = true;
        var f = feed;
        feed.thread = new Thread(() =>
        {
            MjpegLoop(f.Url, f.lockObj, () => f.pending, v => f.pending = v, () => f.running);
        });
        feed.thread.IsBackground = true;
        feed.thread.Start();
    }

    void StartCapeFeed(CapeCameraSet cape)
    {
        if (cape == null || !cape.enabled) return;
        if (cape.capeScreen == null)
        {
            Debug.LogWarning("[CameraStream] Cape capeScreen 미설정");
            return;
        }

        cape.rgbTex = new Texture2D(2, 2);
        cape.depthTex = new Texture2D(2, 2);
        cape.capeScreen.material.SetTexture("_RGBTex", cape.rgbTex);
        cape.capeScreen.material.SetTexture("_DepthTex", cape.depthTex);

        cape.running = true;
        var c = cape;

        c.rgbThread = new Thread(() =>
        {
            MjpegLoop(c.RgbUrl, c.rgbLock, () => c.rgbPending, v => c.rgbPending = v, () => c.running);
        });
        c.rgbThread.IsBackground = true;
        c.rgbThread.Start();

        c.depthThread = new Thread(() =>
        {
            MjpegLoop(c.DepthUrl, c.depthLock, () => c.depthPending, v => c.depthPending = v, () => c.running);
        });
        c.depthThread.IsBackground = true;
        c.depthThread.Start();
    }

    void Update()
    {
        UpdateSingleFeed(outdoorCam);
        UpdateCapeFeed(leftCape);
        UpdateCapeFeed(rightCape);
    }

    void UpdateSingleFeed(CameraFeed feed)
    {
        if (feed == null || !feed.enabled || feed.tex == null) return;
        byte[] data = null;
        lock (feed.lockObj) { if (feed.pending != null) { data = feed.pending; feed.pending = null; } }
        if (data != null) try { feed.tex.LoadImage(data); } catch (Exception e) { Debug.LogWarning($"[CS] RGB 로드 실패: {e.Message}"); }
    }

    void UpdateCapeFeed(CapeCameraSet cape)
    {
        if (cape == null || !cape.enabled) return;

        byte[] rgb = null;
        lock (cape.rgbLock) { if (cape.rgbPending != null) { rgb = cape.rgbPending; cape.rgbPending = null; } }
        if (rgb != null) try { cape.rgbTex.LoadImage(rgb); } catch (Exception e) { Debug.LogWarning($"[CS] Cape RGB 로드 실패: {e.Message}"); }

        byte[] depth = null;
        lock (cape.depthLock) { if (cape.depthPending != null) { depth = cape.depthPending; cape.depthPending = null; } }
        if (depth != null) try { cape.depthTex.LoadImage(depth); } catch (Exception e) { Debug.LogWarning($"[CS] Cape Depth 로드 실패: {e.Message}"); }
    }

    void MjpegLoop(string url, object lockObj, Func<byte[]> getP, Action<byte[]> setP, Func<bool> isRunning)
    {
        while (isRunning())
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 5000;
                using var resp = req.GetResponse();
                using var stream = resp.GetResponseStream();

                var buf = new byte[1024 * 64];
                var ms = new MemoryStream();

                while (isRunning())
                {
                    int read = stream.Read(buf, 0, buf.Length);
                    if (read <= 0) break;
                    ms.Write(buf, 0, read);

                    byte[] data = ms.ToArray();
                    int start = FindBytes(data, new byte[] { 0xFF, 0xD8 });
                    int end = FindBytes(data, new byte[] { 0xFF, 0xD9 });

                    if (start >= 0 && end >= 0 && end > start)
                    {
                        int len = end - start + 2;
                        var jpeg = new byte[len];
                        Array.Copy(data, start, jpeg, 0, len);
                        lock (lockObj) { setP(jpeg); }

                        byte[] remain = new byte[data.Length - (end + 2)];
                        Array.Copy(data, end + 2, remain, 0, remain.Length);
                        ms = new MemoryStream();
                        ms.Write(remain, 0, remain.Length);
                    }

                    if (ms.Length > 1024 * 512) ms = new MemoryStream();
                }
            }
            catch (Exception e)
            {
                if (isRunning())
                    Debug.LogWarning($"[CameraStream] {url} 오류: {e.Message}");
                Thread.Sleep(1000);
            }
        }
    }

    int FindBytes(byte[] data, byte[] pattern)
    {
        for (int i = 0; i <= data.Length - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
                if (data[i + j] != pattern[j]) { match = false; break; }
            if (match) return i;
        }
        return -1;
    }

    void OnDestroy()
    {
        if (outdoorCam != null) outdoorCam.running = false;
        if (leftCape != null) leftCape.running = false;
        if (rightCape != null) rightCape.running = false;
    }
}