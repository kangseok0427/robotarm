using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("컨트롤러")]
    public RobotController robotController;

    [Header("HUD 텍스트 - 왼손 (좌하단)")]
    public TextMeshProUGUI leftHUD;

    [Header("HUD 텍스트 - 오른손 (우하단)")]
    public TextMeshProUGUI rightHUD;

    void Update()
    {
        if (robotController == null) return;

        UpdateLeftHUD();
        UpdateRightHUD();
    }

    void UpdateLeftHUD()
    {
        if (leftHUD == null) return;

        Vector3 handPos = robotController.leftHandPosition;
        Vector3 calibPos = robotController.leftCalibOffset;
        bool calibed = robotController.leftCalibrated;

        // 로봇 좌표계로 변환 (HTTPSender와 동일한 scale)
        float scale = 300f;
        Vector3 robotPos = new Vector3(
             handPos.z * scale,
            -handPos.x * scale,
             handPos.y * scale
        );

        leftHUD.text =
            $"[L] {(calibed ? "CAL OK" : "NO CAL")}\n" +
            $"Hand  X:{handPos.x:F2} Y:{handPos.y:F2} Z:{handPos.z:F2}\n" +
            $"Robot X:{robotPos.x:F0} Y:{robotPos.y:F0} Z:{robotPos.z:F0}mm\n" +
            $"Calib X:{calibPos.x:F2} Y:{calibPos.y:F2} Z:{calibPos.z:F2}\n" +
            $"Grip  {robotController.leftTrigger * 100:F0}%";
    }

    void UpdateRightHUD()
    {
        if (rightHUD == null) return;

        Vector3 handPos = robotController.rightHandPosition;
        Vector3 calibPos = robotController.rightCalibOffset;
        bool calibed = robotController.rightCalibrated;

        float scale = 300f;
        Vector3 robotPos = new Vector3(
             handPos.z * scale,
            -handPos.x * scale,
             handPos.y * scale
        );

        rightHUD.text =
            $"[R] {(calibed ? "CAL OK" : "NO CAL")}\n" +
            $"Hand  X:{handPos.x:F2} Y:{handPos.y:F2} Z:{handPos.z:F2}\n" +
            $"Robot X:{robotPos.x:F0} Y:{robotPos.y:F0} Z:{robotPos.z:F0}mm\n" +
            $"Calib X:{calibPos.x:F2} Y:{calibPos.y:F2} Z:{calibPos.z:F2}\n" +
            $"Grip  {robotController.rightTrigger * 100:F0}%";
    }
}