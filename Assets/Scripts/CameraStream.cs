using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Threading;

public class CameraStream : MonoBehaviour
{
    [Header("서버 설정")]
    public string serverIP = "192.168.45.14";
    public int serverPort = 5000;

    [Header("스크린 오브젝트")]
    public Renderer rgbScreen;
    public Renderer depthScreen;

    private Texture2D _rgbTex;
    private Texture2D _depthTex;

    // 스레드 → 메인 스레드 전달용
    private byte[] _rgbPending;
    private byte[] _depthPending;
    private readonly object _rgbLock = new object();
    private readonly object _depthLock = new object();

    private Thread _rgbThread;
    private Thread _depthThread;
    private bool _running = false;

    void Start()
    {
        _rgbTex = new Texture2D(2, 2);
        _depthTex = new Texture2D(2, 2);

        if (rgbScreen) rgbScreen.material.mainTexture = _rgbTex;
        if (depthScreen) depthScreen.material.mainTexture = _depthTex;

        _running = true;
        _rgbThread = new Thread(() => MjpegLoop($"http://{serverIP}:{serverPort}/video_feed", _rgbLock, ref _rgbPending));
        _depthThread = new Thread(() => MjpegLoop($"http://{serverIP}:{serverPort}/depth_feed", _depthLock, ref _depthPending));
        _rgbThread.IsBackground = true;
        _depthThread.IsBackground = true;
        _rgbThread.Start();
        _depthThread.Start();
    }

    void Update()
    {
        // RGB 업데이트
        byte[] rgbData = null;
        lock (_rgbLock) { if (_rgbPending != null) { rgbData = _rgbPending; _rgbPending = null; } }
        if (rgbData != null) _rgbTex.LoadImage(rgbData);

        // Depth 업데이트
        byte[] depthData = null;
        lock (_depthLock) { if (_depthPending != null) { depthData = _depthPending; _depthPending = null; } }
        if (depthData != null) _depthTex.LoadImage(depthData);
    }

    void MjpegLoop(string url, object lockObj, ref byte[] pending)
    {
        while (_running)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 5000;
                using var resp = req.GetResponse();
                using var stream = resp.GetResponseStream();

                string boundary = null;
                string contentType = resp.ContentType;
                int bIdx = contentType.IndexOf("boundary=");
                if (bIdx >= 0)
                    boundary = "--" + contentType.Substring(bIdx + 9).Trim();

                var buf = new byte[1024 * 64];
                var ms = new MemoryStream();

                while (_running)
                {
                    int read = stream.Read(buf, 0, buf.Length);
                    if (read <= 0) break;
                    ms.Write(buf, 0, read);

                    byte[] data = ms.ToArray();
                    // JPEG SOI(FFD8) ~ EOI(FFD9) 찾기
                    int start = FindBytes(data, new byte[] { 0xFF, 0xD8 });
                    int end = FindBytes(data, new byte[] { 0xFF, 0xD9 });

                    if (start >= 0 && end >= 0 && end > start)
                    {
                        int len = end - start + 2;
                        var jpeg = new byte[len];
                        Array.Copy(data, start, jpeg, 0, len);

                        lock (lockObj) { pending = jpeg; }

                        // 처리된 부분 제거
                        byte[] remain = new byte[data.Length - (end + 2)];
                        Array.Copy(data, end + 2, remain, 0, remain.Length);
                        ms = new MemoryStream();
                        ms.Write(remain, 0, remain.Length);
                    }

                    if (ms.Length > 1024 * 512) ms = new MemoryStream(); // 버퍼 초과 방지
                }
            }
            catch (Exception e)
            {
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
        _running = false;
    }
}