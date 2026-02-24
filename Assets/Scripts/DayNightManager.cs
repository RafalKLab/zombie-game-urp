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

    [Header("Moon")]
    [SerializeField] private Light moonLight;

    [Header("Debug")]
    [SerializeField] private bool freezeCycle = false;
    [SerializeField] private bool useDebugSunAngle = false;
    [SerializeField, Range(0f, 360f)] private float debugSunAngle = 0f;

    private float timeOfDay; // 0..1

    private void Awake()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        if (dayDurationInSeconds <= 0f)
            dayDurationInSeconds = 120f;

        if (moonLight != null && nightPreset != null)
        {
            moonLight.color = nightPreset.sunColor;
            moonLight.intensity = 0f;
        }
    }

    private void Update()
    {
        if (sunLight == null) return;

        if (useDebugSunAngle)
        {
            timeOfDay = Mathf.Repeat(debugSunAngle / 360f, 1f);
        }
        else if (!freezeCycle)
        {
            timeOfDay = Mathf.Repeat(
                timeOfDay + (Time.deltaTime / dayDurationInSeconds),
                1f
            );
        }

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
        float sunAngle = GetSunAngle();

        if (sunAngle < 45f) // Dawn
        {
            float t = Mathf.InverseLerp(0f, 45f, sunAngle);
            ApplyLerp(dawnPreset, dayPreset, t);
        }
        else if (sunAngle < 180f) // Day
        {
            float t = Mathf.InverseLerp(45f, 180f, sunAngle);
            ApplyLerp(dayPreset, duskPreset, t);
        }
        else if (sunAngle < 225f) // Dusk
        {
            float t = Mathf.InverseLerp(180f, 225f, sunAngle);
            ApplyLerp(duskPreset, nightPreset, t);
        }
        else // Night
        {
            float t = Mathf.InverseLerp(225f, 360f, sunAngle);
            ApplyLerp(nightPreset, dawnPreset, t);
        }
    }

    private void ApplyLerp(LightingPreset a, LightingPreset b, float t)
    {
        if (a == null || b == null) return;

        sunLight.color = Color.Lerp(a.sunColor, b.sunColor, t);

        float baseIntensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);

        // 
        float sunHeightFactor = Mathf.Clamp01(
            Vector3.Dot(sunLight.transform.forward, Vector3.down)
        );

        sunLight.intensity = baseIntensity * sunHeightFactor;

        RenderSettings.ambientLight = Color.Lerp(a.ambientColor, b.ambientColor, t);

        RenderSettings.ambientIntensity = Mathf.Lerp(a.ambientIntensity, b.ambientIntensity, t);

        // MOON (fixed)
        if (moonLight != null && nightPreset != null)
        {
            float nightFactor = 1f - sunHeightFactor;
            moonLight.intensity = nightPreset.sunIntensity * nightFactor;
        }
    }

    [System.Serializable]
    private class LightingPreset
    {
        public float sunIntensity = 1f;
        public Color sunColor = Color.white;

        public Color ambientColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public float ambientIntensity = 1f;
    }

    private float GetSunAngle()
    {
        return timeOfDay * 360f;
    }
}