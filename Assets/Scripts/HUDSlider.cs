using UnityEngine;
using UnityEngine.UI;

public class HUDSlider : MonoBehaviour
{
    [Header("조정 대상 (HUDGroup)")]
    public Transform hudGroup;

    [Header("슬라이더")]
    public Slider heightSlider;
    public Slider lateralSlider;
    public Slider depthSlider;

    [Header("범위 (m)")]
    public float heightMin = -1f; public float heightMax = 1f;
    public float lateralMin = -1f; public float lateralMax = 1f;
    public float depthMin = 0.3f; public float depthMax = 2f;

    [Header("기준 위치 (Inspector 직접 조정 가능)")]
    public Vector3 baseOffset = new Vector3(0f, 0f, 0.8f);

    void Start()
    {
        InitSlider(heightSlider, heightMin, heightMax, baseOffset.y, OnHeightChanged);
        InitSlider(lateralSlider, lateralMin, lateralMax, baseOffset.x, OnLateralChanged);
        InitSlider(depthSlider, depthMin, depthMax, baseOffset.z, OnDepthChanged);
        ApplyOffset();
    }

    void InitSlider(Slider s, float min, float max, float val, UnityEngine.Events.UnityAction<float> cb)
    {
        if (s == null) return;
        s.minValue = min;
        s.maxValue = max;
        s.value = val;
        s.onValueChanged.AddListener(cb);
    }

    void OnValidate() => ApplyOffset();
    void OnHeightChanged(float v) { baseOffset.y = v; ApplyOffset(); }
    void OnLateralChanged(float v) { baseOffset.x = v; ApplyOffset(); }
    void OnDepthChanged(float v) { baseOffset.z = v; ApplyOffset(); }

    void ApplyOffset()
    {
        if (hudGroup != null) hudGroup.localPosition = baseOffset;
    }
}