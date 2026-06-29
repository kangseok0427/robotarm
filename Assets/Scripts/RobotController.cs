using UnityEngine;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour
{
    [Header("왼팔 입력값")]
    [Range(0f, 1f)] public float leftTrigger;
    public Vector3 leftHandPosition;
    public Quaternion leftHandRotation;

    [Header("오른팔 입력값")]
    [Range(0f, 1f)] public float rightTrigger;
    public Vector3 rightHandPosition;
    public Quaternion rightHandRotation;

    [Header("Input Actions")]
    public InputActionReference leftTriggerAction;
    public InputActionReference leftPositionAction;
    public InputActionReference leftRotationAction;
    public InputActionReference rightTriggerAction;
    public InputActionReference rightPositionAction;
    public InputActionReference rightRotationAction;

    [Header("캘리브레이션 버튼")]
    public InputActionReference leftCalibrateAction;
    public InputActionReference rightCalibrateAction;

    [Header("캘리브레이션 결과")]
    public Vector3 leftCalibOffset = Vector3.zero;
    public Vector3 rightCalibOffset = Vector3.zero;
    public bool leftCalibrated = false;
    public bool rightCalibrated = false;

    [Header("가동범위 시각화")]
    public Transform leftWorkspaceSphere;
    public Transform rightWorkspaceSphere;

    void OnEnable()
    {
        leftTriggerAction?.action.Enable();
        leftPositionAction?.action.Enable();
        leftRotationAction?.action.Enable();
        rightTriggerAction?.action.Enable();
        rightPositionAction?.action.Enable();
        rightRotationAction?.action.Enable();
        leftCalibrateAction?.action.Enable();
        rightCalibrateAction?.action.Enable();

        if (leftCalibrateAction != null) leftCalibrateAction.action.performed += _ => CalibrateLeft();
        if (rightCalibrateAction != null) rightCalibrateAction.action.performed += _ => CalibrateRight();
    }

    void OnDisable()
    {
        leftTriggerAction?.action.Disable();
        leftPositionAction?.action.Disable();
        leftRotationAction?.action.Disable();
        rightTriggerAction?.action.Disable();
        rightPositionAction?.action.Disable();
        rightRotationAction?.action.Disable();
        leftCalibrateAction?.action.Disable();
        rightCalibrateAction?.action.Disable();

        if (leftCalibrateAction != null) leftCalibrateAction.action.performed -= _ => CalibrateLeft();
        if (rightCalibrateAction != null) rightCalibrateAction.action.performed -= _ => CalibrateRight();
    }

    void CalibrateLeft()
    {
        leftCalibOffset = leftPositionAction?.action.ReadValue<Vector3>() ?? Vector3.zero;
        leftCalibrated = true;
        if (leftWorkspaceSphere != null) leftWorkspaceSphere.position = leftCalibOffset;
        Debug.Log($"[캘리] 왼손: {leftCalibOffset}");
    }

    void CalibrateRight()
    {
        rightCalibOffset = rightPositionAction?.action.ReadValue<Vector3>() ?? Vector3.zero;
        rightCalibrated = true;
        if (rightWorkspaceSphere != null) rightWorkspaceSphere.position = rightCalibOffset;
        Debug.Log($"[캘리] 오른손: {rightCalibOffset}");
    }

    void Update()
    {
        leftTrigger = leftTriggerAction?.action.ReadValue<float>() ?? 0f;
        leftHandPosition = (leftPositionAction?.action.ReadValue<Vector3>() ?? Vector3.zero) - leftCalibOffset;
        leftHandRotation = leftRotationAction?.action.ReadValue<Quaternion>() ?? Quaternion.identity;

        rightTrigger = rightTriggerAction?.action.ReadValue<float>() ?? 0f;
        rightHandPosition = (rightPositionAction?.action.ReadValue<Vector3>() ?? Vector3.zero) - rightCalibOffset;
        rightHandRotation = rightRotationAction?.action.ReadValue<Quaternion>() ?? Quaternion.identity;
    }
}