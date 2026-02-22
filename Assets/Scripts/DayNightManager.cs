using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    [Header("Presets")]
    [SerializeField] private LightingPreset dawnPreset;
    [SerializeField] private LightingPreset dayPreset;
    [SerializeField] private LightingPreset duskPreset;
    [SerializeField] private LightingPreset nightPreset;

    [Header("Sun")]
    [SerializeField] private Light sunLight;
    [SerializeField] private float dayDurationInSeconds = 120f;
    [SerializeField] private float sunYawY = 45f;

    private float timeOfDay; // 0..1

    private void Awake()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        if (dayDurationInSeconds <= 0f)
            dayDurationInSeconds = 120f;
    }

    private void Update()
    {
        if (sunLight == null) return;

        timeOfDay = Mathf.Repeat(timeOfDay + (Time.deltaTime / dayDurationInSeconds), 1f);

        UpdateSunRotation();
        ApplyPreset();
    }

    private void UpdateSunRotation()
    {
        float sunAngleX = timeOfDay * 360f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngleX, sunYawY, 0f);
    }

    private void ApplyPreset()
    {
        if (timeOfDay < 0.20f)
        {
            float t = Mathf.InverseLerp(0.00f, 0.20f, timeOfDay);
            ApplyLerp(dawnPreset, dayPreset, t);
        }
        else if (timeOfDay < 0.70f)
        {
            // Day -> Dusk
            float t = Mathf.InverseLerp(0.20f, 0.70f, timeOfDay);
            ApplyLerp(dayPreset, duskPreset, t);
        }
        else if (timeOfDay < 0.85f)
        {
            // Dusk -> Night
            float t = Mathf.InverseLerp(0.70f, 0.85f, timeOfDay);
            ApplyLerp(duskPreset, nightPreset, t);
        }
        else
        {
            float t = Mathf.InverseLerp(0.85f, 1.00f, timeOfDay);
            ApplyLerp(nightPreset, dawnPreset, t);
        }
    }

    private void ApplyLerp(LightingPreset a, LightingPreset b, float t)
    {
        if (a == null || b == null) return;

        sunLight.color = Color.Lerp(a.sunColor, b.sunColor, t);
        sunLight.intensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);

        RenderSettings.ambientLight = Color.Lerp(a.ambientColor, b.ambientColor, t);

        RenderSettings.ambientIntensity = Mathf.Lerp(a.ambientIntensity, b.ambientIntensity, t);
    }

    [System.Serializable]
    private class LightingPreset
    {
        public float sunIntensity = 1f;
        public Color sunColor = Color.white;

        public Color ambientColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public float ambientIntensity = 1f;
    }
}