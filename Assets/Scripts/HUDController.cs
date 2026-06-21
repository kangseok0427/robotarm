using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("=== 왼손 패널 사용 여부 ===")]
    public bool useLeftPanel = true;

    [Header("=== 오른손 패널 사용 여부 ===")]
    public bool useRightPanel = false;

    [Header("컨트롤러")]
    public RobotController robotController;

    [Header("왼손 HUD 텍스트")]
    public TextMeshProUGUI leftHUD;

    [Header("오른손 HUD 텍스트")]
    public TextMeshProUGUI rightHUD;

    [Header("좌표 변환 스케일 (HTTPSender와 동일하게)")]
    public float coordScale = 280f;

    void Update()
    {
        if (robotController == null) return;

        if (useLeftPanel)  UpdateLeftHUD();
        if (useRightPanel) UpdateRightHUD();
    }

    void UpdateLeftHUD()
    {
        if (leftHUD == null) return;

        Vector3 handPos  = robotController.leftHandPosition;
        Vector3 calibPos = robotController.leftCalibOffset;
        bool    calibed  = robotController.leftCalibrated;

        Vector3 robotPos = new Vector3(
             handPos.z * coordScale,
            -handPos.x * coordScale,
             handPos.y * coordScale
        );

        leftHUD.text =
            $"<b>LEFT ARM</b> {(calibed ? "<color=#00FF88>● CAL</color>" : "<color=#FF4444>○ NO CAL</color>")}\n" +
            $"Hand  {handPos.x:F2} {handPos.y:F2} {handPos.z:F2}\n" +
            $"Robot {robotPos.x:F0} {robotPos.y:F0} {robotPos.z:F0} mm\n" +
            $"Grip  {robotController.leftTrigger * 100:F0}%";
    }

    void UpdateRightHUD()
    {
        if (rightHUD == null) return;

        Vector3 handPos  = robotController.rightHandPosition;
        bool    calibed  = robotController.rightCalibrated;

        Vector3 robotPos = new Vector3(
             handPos.z * coordScale,
            -handPos.x * coordScale,
             handPos.y * coordScale
        );

        rightHUD.text =
            $"<b>RIGHT ARM</b> {(calibed ? "<color=#00FF88>● CAL</color>" : "<color=#FF4444>○ NO CAL</color>")}\n" +
            $"Hand  {handPos.x:F2} {handPos.y:F2} {handPos.z:F2}\n" +
            $"Robot {robotPos.x:F0} {robotPos.y:F0} {robotPos.z:F0} mm\n" +
            $"Grip  {robotController.rightTrigger * 100:F0}%";
    }
}
