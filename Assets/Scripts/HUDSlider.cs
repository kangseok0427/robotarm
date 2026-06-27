using UnityEngine;
using UnityEngine.UI;

public class HUDSlider : MonoBehaviour
{
    [Header("조정할 대상 (HUDGroup)")]
    public Transform hudGroup;

    [Header("슬라이더 UI")]
    public Slider heightSlider;   // Y축 높이
    public Slider lateralSlider;  // X축 좌우
    public Slider depthSlider;    // Z축 앞뒤

    [Header("조정 범위 (m)")]
    public float heightMin = -1f;
    public float heightMax = 1f;
    public float lateralMin = -1f;
    public float lateralMax = 1f;
    public float depthMin = 0.3f;
    public float depthMax = 2f;

    [Header("기준 위치 (Inspector에서도 직접 조정 가능)")]
    public Vector3 baseOffset = new Vector3(0f, 0f, 0.8f);

    void Start()
    {
        if (heightSlider)
        {
            heightSlider.minValue = heightMin;
            heightSlider.maxValue = heightMax;
            heightSlider.value = baseOffset.y;
            heightSlider.onValueChanged.AddListener(OnHeightChanged);
        }

        if (lateralSlider)
        {
            lateralSlider.minValue = lateralMin;
            lateralSlider.maxValue = lateralMax;
            lateralSlider.value = baseOffset.x;
            lateralSlider.onValueChanged.AddListener(OnLateralChanged);
        }

        if (depthSlider)
        {
            depthSlider.minValue = depthMin;
            depthSlider.maxValue = depthMax;
            depthSlider.value = baseOffset.z;
            depthSlider.onValueChanged.AddListener(OnDepthChanged);
        }

        ApplyOffset();
    }

    void OnValidate()
    {
        // Inspector에서 baseOffset 직접 바꿔도 반영
        ApplyOffset();
    }

    void OnHeightChanged(float val)
    {
        baseOffset.y = val;
        ApplyOffset();
    }

    void OnLateralChanged(float val)
    {
        baseOffset.x = val;
        ApplyOffset();
    }

    void OnDepthChanged(float val)
    {
        baseOffset.z = val;
        ApplyOffset();
    }

    void ApplyOffset()
    {
        if (hudGroup == null) return;
        hudGroup.localPosition = baseOffset;
    }
}