using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("사용 여부")]
    public bool useLeftPanel = true;
    public bool useRightPanel = true;

    [Header("컨트롤러")]
    public RobotController robotController;

    [Header("HUD 텍스트 (TextMeshPro 3D 오브젝트)")]
    public TextMeshPro leftHUD;
    public TextMeshPro rightHUD;

    [Header("좌표 스케일 (HTTPSender와 동일)")]
    public float coordScale = 280f;

    void Update()
    {
        if (robotController == null) return;
        if (useLeftPanel) UpdateLeft();
        if (useRightPanel) UpdateRight();
    }

    void UpdateLeft()
    {
        if (leftHUD == null) return;
        Vector3 h = robotController.leftHandPosition;
        Vector3 r = new Vector3(h.z * coordScale, -h.x * coordScale, h.y * coordScale);
        bool cal = robotController.leftCalibrated;

        leftHUD.text =
            $"LEFT  {(cal ? "CAL OK" : "NO CAL")}\n" +
            $"H {h.x:F2} {h.y:F2} {h.z:F2}\n" +
            $"R {r.x:F0} {r.y:F0} {r.z:F0} mm\n" +
            $"Grip {robotController.leftTrigger * 100:F0}%";
    }

    void UpdateRight()
    {
        if (rightHUD == null) return;
        Vector3 h = robotController.rightHandPosition;
        Vector3 r = new Vector3(h.z * coordScale, -h.x * coordScale, h.y * coordScale);
        bool cal = robotController.rightCalibrated;

        rightHUD.text =
            $"RIGHT {(cal ? "CAL OK" : "NO CAL")}\n" +
            $"H {h.x:F2} {h.y:F2} {h.z:F2}\n" +
            $"R {r.x:F0} {r.y:F0} {r.z:F0} mm\n" +
            $"Grip {robotController.rightTrigger * 100:F0}%";
    }
}