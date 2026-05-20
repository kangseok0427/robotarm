using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class SerialCommunication : MonoBehaviour
{
    [Header("시리얼 포트 설정")]
    public string portName = "COM3";    // 포트명 (COM3, COM4 등)
    public int baudRate = 115200;        // 보드레이트
    public int readTimeout = 100;        // 읽기 타임아웃 (ms)

    [Header("전송 설정")]
    public float sendInterval = 0.05f;  // 전송 주기 (초), 기본 20fps

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning = false;
    private float sendTimer = 0f;

    // 로봇 상태 (RobotController에서 값 받아옴)
    [HideInInspector] public float[] leftMotors = new float[6]; // 왼팔 모터 1~6
    [HideInInspector] public float[] rightMotors = new float[6]; // 오른팔 모터 1~6

    void Start()
    {
        OpenPort();
    }

    void Update()
    {
        if (!IsConnected()) return;

        // 일정 주기마다 전송
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            SendMotorData();
        }
    }

    void OpenPort()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = readTimeout;
            serialPort.Open();

            // 수신 스레드 시작 (로봇에서 피드백 받을 때 사용)
            isRunning = true;
            readThread = new Thread(ReadLoop);
            readThread.Start();

            Debug.Log($"시리얼 연결 성공: {portName} / {baudRate}bps");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"시리얼 연결 실패: {e.Message}");
        }
    }

    void SendMotorData()
    {
        // 패킷 형식: L:m1,m2,m3,m4,m5,m6;R:m1,m2,m3,m4,m5,m6\n
        // TODO: 교수님한테 실제 패킷 형식 확인 후 수정
        string packet = $"L:{leftMotors[0]:F1},{leftMotors[1]:F1},{leftMotors[2]:F1},{leftMotors[3]:F1},{leftMotors[4]:F1},{leftMotors[5]:F1};" +
                        $"R:{rightMotors[0]:F1},{rightMotors[1]:F1},{rightMotors[2]:F1},{rightMotors[3]:F1},{rightMotors[4]:F1},{rightMotors[5]:F1}\n";

        try
        {
            serialPort.Write(packet);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"전송 실패: {e.Message}");
        }
    }

    void ReadLoop()
    {
        // 로봇에서 오는 피드백 수신 (필요시 활용)
        while (isRunning)
        {
            try
            {
                string response = serialPort.ReadLine();
                Debug.Log($"로봇 응답: {response}");
                // TODO: 응답 파싱 로직 추가
            }
            catch { }
        }
    }

    bool IsConnected()
    {
        return serialPort != null && serialPort.IsOpen;
    }

    void OnDestroy()
    {
        isRunning = false;
        readThread?.Join(500);

        if (IsConnected())
            serialPort.Close();
    }
}