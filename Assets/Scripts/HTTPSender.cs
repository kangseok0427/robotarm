using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class HTTPSender : MonoBehaviour
{
    [Header("서버 설정")]
    public string serverIP = "192.168.45.14";
    public int serverPort = 5000;

    [Header("전송 설정")]
    public float sendInterval = 0.05f;
    public float moveMs = 500f;
    public float timeout = 2f;

    [Header("스무딩")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.1f;

    [Header("로봇 가동범위")]
    public float robotReach = 280f;
    public Vector3 initPosition = new Vector3(200f, 0f, 150f);

    [Header("워크스페이스 구체")]
    public Transform workspaceSphere;

    [Header("컨트롤러")]
    public RobotController robotController;

    private Vector3 _smoothPos;
    private float _smoothGrip;
    private float _smoothPitch;
    private bool _initialized = false;
    private bool _busy = false;
    private float _timer = 0f;

    private string MoveXYZUrl => $"http://{serverIP}:{serverPort}/api/move/xyz";
    private string GripperUrl => $"http://{serverIP}:{serverPort}/api/gripper";

    void Start()
    {
        _smoothPos = initPosition;
        _smoothGrip = 60f;
        _smoothPitch = 0f;
        StartCoroutine(InitPose());
    }

    IEnumerator InitPose()
    {
        yield return new WaitForSeconds(1f);
        yield return Post(MoveXYZUrl, $"{{\"x\":{initPosition.x:F1},\"y\":{initPosition.y:F1},\"z\":{initPosition.z:F1},\"ms\":2000}}");
        yield return Post(GripperUrl, "{\"deg\":30,\"ms\":2000}");
        yield return new WaitForSeconds(2f);
        _initialized = true;
        Debug.Log("[HTTPSender] 초기 자세 완료");
    }

    void Update()
    {
        if (robotController == null || workspaceSphere == null) return;

        // 컨트롤러 월드 위치
        Vector3 handWorld = robotController.leftHandPosition + robotController.leftCalibOffset;
        Vector3 sphereCenter = workspaceSphere.position;
        float sphereRadius = workspaceSphere.lossyScale.x * 0.5f;

        // 스피어 기준 상대 위치
        Vector3 offset = handWorld - sphereCenter;
        if (offset.magnitude > sphereRadius)
            offset = offset.normalized * sphereRadius;

        // 정규화 (-1 ~ 1)
        Vector3 normalized = offset / sphereRadius;

        // Unity → 로봇 좌표계 매핑
        Vector3 robotPos = new Vector3(
            normalized.z * robotReach,
            normalized.x * robotReach * -1f,
            81f + normalized.y * robotReach
        );

        // 컨트롤러 pitch → 로봇 phi
        float pitch = robotController.leftHandRotation.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        // Lerp 스무딩
        _smoothPos = Vector3.Lerp(_smoothPos, robotPos, smoothSpeed);
        _smoothGrip = Mathf.Lerp(_smoothGrip, Mathf.Lerp(60f, 165f, robotController.leftTrigger), smoothSpeed);
        _smoothPitch = Mathf.Lerp(_smoothPitch, pitch, smoothSpeed * 0.3f);

        if (!_initialized || _busy) return;

        _timer += Time.deltaTime;
        if (_timer < sendInterval) return;
        _timer = 0f;

        StartCoroutine(SendAll());
    }

    IEnumerator SendAll()
    {
        _busy = true;

        string phi = _smoothPitch.ToString("F1");
        string posJson = $"{{\"x\":{_smoothPos.x:F1},\"y\":{_smoothPos.y:F1},\"z\":{_smoothPos.z:F1},\"ms\":{(int)moveMs},\"phi_deg\":{phi}}}";
        string gripJson = $"{{\"deg\":{(int)_smoothGrip},\"ms\":{(int)moveMs}}}";

        // 컨트롤러 Z축 회전 → 손목 롤 (0~270도)
        // 컨트롤러 Z축 회전 → 손목 롤
        float roll = robotController.leftHandRotation.eulerAngles.z;
        Debug.Log($"[ROLL] {roll:F1}");
        if (roll > 180f) roll -= 360f;   // -180~180 범위로 변환

        // 감도 조절 (0.5 = 절반만 반영, 값 낮출수록 둔감)
        float sensitivity = 1.5f;
        float wristTarget = 90f + (roll * sensitivity * -1f);  // 135도 중심, 축 반전

        int wristDeg = (int)Mathf.Clamp(wristTarget, 0f, 270f);
        string wristJson = $"{{\"deg\":{wristDeg},\"ms\":{(int)moveMs}}}";

        yield return Post(MoveXYZUrl, posJson);
        yield return Post(GripperUrl, gripJson);
        yield return Post($"http://{serverIP}:{serverPort}/api/wrist", wristJson);

        _busy = false;
    }

    IEnumerator Post(string url, string json)
    {
        byte[] body = Encoding.UTF8.GetBytes(json);
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = (int)timeout;
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogWarning($"HTTP 실패: {req.error}");
    }
}