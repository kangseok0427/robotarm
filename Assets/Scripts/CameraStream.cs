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
    public string endpoint = "/video_feed";   // /video_feed 또는 /depth_feed

    [Header("출력 대상 (Quad의 Renderer)")]
    public Renderer targetScreen;

    [HideInInspector] public Texture2D tex;
    [HideInInspector] public byte[] pending;
    [HideInInspector] public object lockObj = new object();
    [HideInInspector] public Thread thread;
    [HideInInspector] public bool running;

    public string Url => $"http://{serverIP}:{serverPort}{endpoint}";
}

public class CameraStream : MonoBehaviour
{
    [Header("=== 외부캠 (O.C) ===")]
    public CameraFeed outdoorCam;

    [Header("=== 왼쪽 로봇 카메라 ===")]
    public CameraFeed leftRGB;
    public CameraFeed leftDepth;

    [Header("=== 오른쪽 로봇 카메라 ===")]
    public CameraFeed rightRGB;
    public CameraFeed rightDepth;

    private CameraFeed[] _allFeeds;

    void Start()
    {
        _allFeeds = new[] { outdoorCam, leftRGB, leftDepth, rightRGB, rightDepth };

        foreach (var feed in _allFeeds)
            StartFeed(feed);
    }

    void StartFeed(CameraFeed feed)
    {
        if (feed == null || !feed.enabled) return;
        if (feed.targetScreen == null)
        {
            Debug.LogWarning($"[CameraStream] {feed.endpoint} - targetScreen 미설정, 스킵");
            return;
        }

        feed.tex = new Texture2D(2, 2);
        feed.targetScreen.material.mainTexture = feed.tex;

        feed.running = true;
        var f = feed;
        feed.thread = new Thread(() => MjpegLoop(f));
        feed.thread.IsBackground = true;
        feed.thread.Start();
    }

    void Update()
    {
        if (_allFeeds == null) return;

        foreach (var feed in _allFeeds)
        {
            if (feed == null || !feed.enabled || feed.tex == null) continue;

            byte[] data = null;
            lock (feed.lockObj)
            {
                if (feed.pending != null) { data = feed.pending; feed.pending = null; }
            }
            if (data != null)
            {
                try { feed.tex.LoadImage(data); }
                catch (Exception e) { Debug.LogWarning($"[CameraStream] 텍스처 로드 실패: {e.Message}"); }
            }
        }
    }

    void MjpegLoop(CameraFeed feed)
    {
        while (feed.running)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(feed.Url);
                req.Timeout = 5000;
                using var resp = req.GetResponse();
                using var stream = resp.GetResponseStream();

                var buf = new byte[1024 * 64];
                var ms = new MemoryStream();

                while (feed.running)
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
                        lock (feed.lockObj) { feed.pending = jpeg; }

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
                if (feed.running)
                    Debug.LogWarning($"[CameraStream] {feed.Url} 오류: {e.Message}");
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
        if (_allFeeds == null) return;
        foreach (var feed in _allFeeds)
            if (feed != null) feed.running = false;
    }
}