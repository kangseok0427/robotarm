using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

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

    [Header("집게 설정")]
    public float gripOpenAngle = 0f;
    public float gripCloseAngle = 90f;

    [Header("Input Actions")]
    public InputActionReference leftTriggerAction;
    public InputActionReference leftPositionAction;
    public InputActionReference leftRotationAction;

    public InputActionReference rightTriggerAction;
    public InputActionReference rightPositionAction;
    public InputActionReference rightRotationAction;

    void OnEnable()
    {
        leftTriggerAction?.action.Enable();
        leftPositionAction?.action.Enable();
        leftRotationAction?.action.Enable();
        rightTriggerAction?.action.Enable();
        rightPositionAction?.action.Enable();
        rightRotationAction?.action.Enable();
    }

    void OnDisable()
    {
        leftTriggerAction?.action.Disable();
        leftPositionAction?.action.Disable();
        leftRotationAction?.action.Disable();
        rightTriggerAction?.action.Disable();
        rightPositionAction?.action.Disable();
        rightRotationAction?.action.Disable();
    }

    void Update()
    {
        ReadLeftArm();
        ReadRightArm();
    }

    void ReadLeftArm()
    {
        leftTrigger = leftTriggerAction?.action.ReadValue<float>() ?? 0f;
        leftHandPosition = leftPositionAction?.action.ReadValue<Vector3>() ?? Vector3.zero;
        leftHandRotation = leftRotationAction?.action.ReadValue<Quaternion>() ?? Quaternion.identity;

        float leftGripAngle = Mathf.Lerp(gripOpenAngle, gripCloseAngle, leftTrigger);

        // TODO: 왼팔 모터 1~5번에 위치/회전 전송
        // TODO: 왼팔 모터 6번(집게)에 leftGripAngle 전송
    }

    void ReadRightArm()
    {
        rightTrigger = rightTriggerAction?.action.ReadValue<float>() ?? 0f;
        rightHandPosition = rightPositionAction?.action.ReadValue<Vector3>() ?? Vector3.zero;
        rightHandRotation = rightRotationAction?.action.ReadValue<Quaternion>() ?? Quaternion.identity;

        float rightGripAngle = Mathf.Lerp(gripOpenAngle, gripCloseAngle, rightTrigger);

        // TODO: 오른팔 모터 1~5번에 위치/회전 전송
        // TODO: 오른팔 모터 6번(집게)에 rightGripAngle 전송
    }
}