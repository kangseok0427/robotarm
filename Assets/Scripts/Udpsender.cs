using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.VisualScripting;

public class UDPSender : MonoBehaviour
{
    [Header("UDP 설정")]
    public string ip = "127.0.0.1";  // PyBullet이 같은 PC면 127.0.0.1
    public int port = 5005;

    [Header("연결할 컨트롤러")]
    public RobotController robotController;

    private UdpClient udpClient;

    [Header("공 위치 조정")]
    public float scale = 0.3f;

    void Start()
    {
        udpClient = new UdpClient();
    }

    void Update()
    {
        if (robotController == null) return;
        SendData();
    }

    void SendData()
    {
        // RobotController에서 트리거/포지션 값 가져와서 전송
        // 포맷: leftTrigger,rightTrigger,lx,ly,lz,rx,ry,rz
        Vector3 pos = robotController.leftHandPosition;
        string data = $"{robotController.leftTrigger:F3}," +
                      $"{robotController.rightTrigger:F3}," +
                      $"{pos.x * scale:F3}," +
                      $"{pos.z * scale:F3}," +   // Unity Z → PyBullet Y
                      $"{pos.y * scale:F3}," +   // Unity Y → PyBullet Z
                      $"{robotController.rightHandPosition.x * scale:F3}," +
                      $"{robotController.rightHandPosition.z * scale:F3}," +
                      $"{robotController.rightHandPosition.y * scale:F3}";

        byte[] bytes = Encoding.UTF8.GetBytes(data);

        try
        {
            udpClient.Send(bytes, bytes.Length, ip, port);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"UDP 전송 실패: {e.Message}");
        }
    }

    void OnDestroy()
    {
        udpClient?.Close();
    }
}