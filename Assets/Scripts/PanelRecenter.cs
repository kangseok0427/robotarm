using UnityEngine;
using UnityEngine.InputSystem;

public class PanelRecenter : MonoBehaviour
{
    [Header("재배치할 패널들 (커브드 HUD 전체)")]
    public Transform[] panels;

    [Header("플레이어 (헤드셋 카메라)")]
    public Transform headCamera;

    [Header("재배치 거리 (m)")]
    public float distance = 1.0f;

    [Header("그립 버튼 (양손 동시)")]
    public InputActionReference leftGripAction;
    public InputActionReference rightGripAction;

    [Header("꾹 누르는 시간 (초)")]
    public float holdTime = 1.0f;

    private float _holdTimer = 0f;
    private bool  _triggered = false;

    void OnEnable()
    {
        leftGripAction?.action.Enable();
        rightGripAction?.action.Enable();
    }

    void OnDisable()
    {
        leftGripAction?.action.Disable();
        rightGripAction?.action.Disable();
    }

    void Update()
    {
        if (leftGripAction == null || rightGripAction == null || headCamera == null) return;

        float leftGrip  = leftGripAction.action.ReadValue<float>();
        float rightGrip = rightGripAction.action.ReadValue<float>();

        bool bothHeld = leftGrip > 0.8f && rightGrip > 0.8f;

        if (bothHeld)
        {
            _holdTimer += Time.deltaTime;
            if (_holdTimer >= holdTime && !_triggered)
            {
                RecenterPanels();
                _triggered = true;
            }
        }
        else
        {
            _holdTimer = 0f;
            _triggered = false;
        }
    }

    void RecenterPanels()
    {
        if (panels == null || panels.Length == 0)
        {
            Debug.LogWarning("[PanelRecenter] panels 배열이 비어있음");
            return;
        }

        // 머리 정면 방향 기준 (Y축 회전만 사용, 위아래 기울임 무시)
        Vector3 forward = headCamera.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 center = headCamera.position + forward * distance;
        Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);

        foreach (var panel in panels)
        {
            if (panel == null) continue;
            // 패널 각자의 로컬 오프셋은 유지한 채 그룹 전체를 이동
            // (패널들이 부모 오브젝트 자식이면 부모만 옮기면 됨 - 권장)
        }

        // panels[0]의 부모가 전체 그룹이면 그 부모를 옮기는 게 가장 간단함
        if (panels[0].parent != null)
        {
            panels[0].parent.position = center;
            panels[0].parent.rotation = rot;
        }

        Debug.Log("[PanelRecenter] 패널 재배치 완료");
    }
}
