using UnityEngine;

public class WebCamDisplay : MonoBehaviour
{
    [Header("웹캠 설정")]
    public string deviceName = "";  // 비워두면 첫 번째 웹캠 자동 선택
    public int width = 1280;
    public int height = 720;
    public int fps = 30;

    private WebCamTexture webCamTexture;
    private Renderer quadRenderer;

    void Start()
    {
        quadRenderer = GetComponent<Renderer>();

        // 연결된 웹캠 목록 출력 (DroidCam 이름 확인용)
        foreach (var device in WebCamTexture.devices)
            Debug.Log($"웹캠 발견: {device.name}");

        // 웹캠 시작
        webCamTexture = string.IsNullOrEmpty(deviceName)
            ? new WebCamTexture(width, height, fps)
            : new WebCamTexture(deviceName, width, height, fps);

        // Quad 표면에 웹캠 텍스처 적용
        quadRenderer.material.mainTexture = webCamTexture;
        webCamTexture.Play();
    }

    void OnDestroy()
    {
        // 씬 종료 시 웹캠 해제
        if (webCamTexture != null && webCamTexture.isPlaying)
            webCamTexture.Stop();
    }
}