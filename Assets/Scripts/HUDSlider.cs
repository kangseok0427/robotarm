using UnityEngine;
using UnityEngine.InputSystem;

public class HUDSlider : MonoBehaviour
{
    [Header("조정 대상 (HUDGroup)")]
    public Transform hudGroup;

    [Header("컨트롤러 (오른손 기준)")]
    public InputActionReference adjustButtonAction;   // A 버튼
    public InputActionReference handPositionAction;   // 오른손 Position

    [Header("조정 범위 (m, 기본 위치 기준 +-)")]
    public float adjustRange = 0.5f;

    [Header("손 움직임 민감도")]
    public float sensitivity = 1.0f;

    private float _baseY;
    private float _currentOffset = 0f;
    private bool _holding = false;
    private float _grabStartHandY;
    private float _grabStartOffset;

    void OnEnable()
    {
        adjustButtonAction?.action.Enable();
        handPositionAction?.action.Enable();
    }

    void OnDisable()
    {
        adjustButtonAction?.action.Disable();
        handPositionAction?.action.Disable();
    }

    void Start()
    {
        if (hudGroup != null)
            _baseY = hudGroup.position.y;
    }

    void Update()
    {
        if (hudGroup == null || adjustButtonAction == null || handPositionAction == null) return;

        float pressed = adjustButtonAction.action.ReadValue<float>();
        Vector3 handPos = handPositionAction.action.ReadValue<Vector3>();

        bool isPressed = pressed > 0.5f;

        if (isPressed && !_holding)
        {
            // 버튼 막 눌림 - 기준점 기록
            _holding = true;
            _grabStartHandY = handPos.y;
            _grabStartOffset = _currentOffset;
        }
        else if (isPressed && _holding)
        {
            // 누르고 있는 동안 - 손 움직임만큼 오프셋 반영
            float delta = (handPos.y - _grabStartHandY) * sensitivity;
            _currentOffset = Mathf.Clamp(_grabStartOffset + delta, -adjustRange, adjustRange);
            ApplyOffset();
        }
        else if (!isPressed && _holding)
        {
            // 버튼 뗌
            _holding = false;
        }
    }

    void ApplyOffset()
    {
        Vector3 pos = hudGroup.position;
        pos.y = _baseY + _currentOffset;
        hudGroup.position = pos;
    }
}