using UnityEngine;

public class ArmIK : MonoBehaviour
{
    [Header("관절 (순서대로 Joint1~5)")]
    public Transform[] joints;

    [Header("타겟")]
    public Transform target;

    [Header("IK 설정")]
    public int iterations = 10;
    public float tolerance = 0.01f;

    [Header("링크 길이 (직접 입력)")]
    public float[] linkLengths;

    [Header("집게")]
    public Transform gripperL;
    public Transform gripperR;
    [Range(0f, 1f)] public float gripValue = 0f;
    public float gripOpenPos = 0.2f;
    public float gripClosePos = 0.05f;

    private Vector3[] positions;

    void Start()
    {
        positions = new Vector3[joints.Length];
    }

    void LateUpdate()
    {
        if (target == null || joints.Length < 2) return;

        Vector3 root = joints[0].position;

        // 현재 위치 저장
        for (int i = 0; i < joints.Length; i++)
            positions[i] = joints[i].position;

        // FABRIK
        for (int iter = 0; iter < iterations; iter++)
        {
            // Forward
            positions[positions.Length - 1] = target.position;
            for (int i = positions.Length - 2; i >= 0; i--)
            {
                Vector3 dir = (positions[i] - positions[i + 1]).normalized;
                positions[i] = positions[i + 1] + dir * linkLengths[i];
            }

            // Backward
            positions[0] = root;
            for (int i = 1; i < positions.Length; i++)
            {
                Vector3 dir = (positions[i] - positions[i - 1]).normalized;
                positions[i] = positions[i - 1] + dir * linkLengths[i - 1];
            }

            if (Vector3.Distance(positions[positions.Length - 1], target.position) < tolerance)
                break;
        }

        // 위치 + 회전 적용 (Cylinder는 Y축이 위라서 up 방향으로 LookRotation)
        for (int i = 0; i < joints.Length - 1; i++)
        {
            joints[i].position = positions[i];
            Vector3 dir = positions[i + 1] - positions[i];
            if (dir.sqrMagnitude > 0.0001f)
                joints[i].rotation = Quaternion.FromToRotation(Vector3.up, dir);
        }

        // 마지막 Joint는 이전 방향 그대로 따라가게
        joints[joints.Length - 1].position = positions[joints.Length - 1];
        joints[joints.Length - 1].rotation = joints[joints.Length - 2].rotation;

        // 집게 제어
        if (gripperL != null && gripperR != null)
        {
            float pos = Mathf.Lerp(gripOpenPos, gripClosePos, gripValue);
            var posL = gripperL.localPosition;
            posL.x = -pos;
            gripperL.localPosition = posL;

            var posR = gripperR.localPosition;
            posR.x = pos;
            gripperR.localPosition = posR;
        }
    }

    void OnDrawGizmos()
    {
        if (positions == null || positions.Length == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < positions.Length - 1; i++)
            Gizmos.DrawLine(positions[i], positions[i + 1]);

        Gizmos.color = Color.red;
        foreach (var p in positions)
            Gizmos.DrawSphere(p, 0.05f);
    }
}