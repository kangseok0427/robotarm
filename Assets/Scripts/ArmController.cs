using UnityEngine;

public class ArmController : MonoBehaviour
{
    [Header("관절 Transform")]
    public Transform joint1;  // Y축 회전 (베이스 회전)
    public Transform joint2;  // Z축 회전 (앞뒤)
    public Transform joint3;  // Z축 회전 (앞뒤)
    public Transform joint4;  // Z축 회전 (앞뒤)
    public Transform joint5;  // X축 회전 (손목 비틀기)

    [Header("집게 Transform")]
    public Transform gripperL;
    public Transform gripperR;

    [Header("관절 각도 (Inspector 테스트용)")]
    [Range(0f, 180f)] public float angle1 = 90f;
    [Range(0f, 180f)] public float angle2 = 90f;
    [Range(0f, 180f)] public float angle3 = 90f;
    [Range(0f, 180f)] public float angle4 = 90f;
    [Range(0f, 270f)] public float angle5 = 135f;
    [Range(0f, 1f)] public float gripValue = 0f;  // 0=열림, 1=닫힘

    [Header("집게 설정")]
    public float gripOpenPos = 0.2f;
    public float gripClosePos = 0.05f;

    void Update()
    {
        if (joint1) joint1.localRotation = Quaternion.Euler(0, angle1, 0);
        if (joint2) joint2.localRotation = Quaternion.Euler(0, 0, angle2);
        if (joint3) joint3.localRotation = Quaternion.Euler(0, 0, angle3);
        if (joint4) joint4.localRotation = Quaternion.Euler(0, 0, angle4);
        if (joint5) joint5.localRotation = Quaternion.Euler(angle5, 0, 0);

        UpdateGripper();
    }

    void UpdateGripper()
    {
        if (gripperL == null || gripperR == null) return;

        float pos = Mathf.Lerp(gripOpenPos, gripClosePos, gripValue);

        var posL = gripperL.localPosition;
        posL.x = -pos;
        gripperL.localPosition = posL;

        var posR = gripperR.localPosition;
        posR.x = pos;
        gripperR.localPosition = posR;
    }

    // RobotController에서 호출
    public void SetAngles(float a1, float a2, float a3, float a4, float a5, float grip)
    {
        angle1 = a1;
        angle2 = a2;
        angle3 = a3;
        angle4 = a4;
        angle5 = a5;
        gripValue = grip;
    }
}