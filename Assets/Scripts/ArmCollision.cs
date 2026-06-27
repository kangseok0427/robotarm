using UnityEngine;

public class ArmCollision : MonoBehaviour
{
    [Header("왼팔 HTTPSender")]
    public HTTPSender leftSender;

    [Header("오른팔 HTTPSender (나중에 연결)")]
    public HTTPSender rightSender;

    [Header("충돌 설정 (mm)")]
    public float minArmDistance = 80f;
    public float softZoneRange = 40f;

    [Header("경고 시각화")]
    public Renderer leftSphere;
    public Renderer rightSphere;
    public Color normalColor = new Color(0.2f, 0.8f, 1f, 0.2f);
    public Color warningColor = new Color(1f, 0.4f, 0f, 0.4f);

    [HideInInspector] public Vector3 leftPosOverride = Vector3.zero;
    [HideInInspector] public Vector3 rightPosOverride = Vector3.zero;
    [HideInInspector] public bool leftOverrideActive = false;
    [HideInInspector] public bool rightOverrideActive = false;

    void Update()
    {
        leftOverrideActive = false;
        rightOverrideActive = false;

        // 오른팔 없으면 스킵
        if (leftSender == null || rightSender == null)
        {
            SetSphereColor(leftSphere, normalColor);
            SetSphereColor(rightSphere, normalColor);
            return;
        }

        Vector3 leftPos = leftSender.GetTargetPos();
        Vector3 rightPos = rightSender.GetTargetPos();

        float dist = Vector3.Distance(leftPos, rightPos);

        if (dist < minArmDistance + softZoneRange)
        {
            SetSphereColor(leftSphere, warningColor);
            SetSphereColor(rightSphere, warningColor);

            if (dist < minArmDistance)
            {
                Vector3 pushDir = (leftPos - rightPos).normalized;
                float push = (minArmDistance - dist) * 0.5f;

                leftPosOverride = leftPos + pushDir * push;
                rightPosOverride = rightPos - pushDir * push;
                leftOverrideActive = true;
                rightOverrideActive = true;
            }
        }
        else
        {
            SetSphereColor(leftSphere, normalColor);
            SetSphereColor(rightSphere, normalColor);
        }
    }

    void SetSphereColor(Renderer r, Color c)
    {
        if (r == null) return;
        r.material.color = c;
    }
}