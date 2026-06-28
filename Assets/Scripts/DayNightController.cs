using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightController : MonoBehaviour
{
    [System.Serializable]
    public class SkyPhase
    {
        public string phaseName;
        public Material skyMaterial;
        public float duration = 60f;

        [Header("Sun Settings")]
        public Color sunColor = Color.white;
        public float sunIntensity = 1f;
        public float targetSunAngleX = 90f;

        [Header("Fog Settings")]
        public Color fogColor = new Color(0.5f, 0.5f, 0.5f);
        public float fogDensity = 0.01f;

        [Header("Bloom")]
        public float bloomIntensity = 5f;
        public float bloomThreshold = 1.3f;
        public float bloomScatter = 0.83f;
        [ColorUsage(false, true)] public Color bloomTint = Color.white;

        [Header("Color Adjustments")]
        public float postExposure = 0.3f;
        public float contrast = -15f;
        public float saturation = 0f;
        public float hueShift = 0f;
        [ColorUsage(false)] public Color colorFilter = Color.white;

        [Header("Vignette")]
        public Color vignetteColor = Color.black;
        [Range(0f, 1f)] public float vignetteIntensity = 0.12f;
        [Range(0.01f, 1f)] public float vignetteSmoothness = 0.81f;

        [Header("Motion Blur")]
        [Range(0f, 1f)] public float motionBlurIntensity = 0.11f;
        [Range(0f, 1f)] public float motionBlurClamp = 0.101f;

        [Header("Depth of Field")]
        public bool enableDOF = false;
        public float dofFocusDistance = 10f;
        public float dofFocalLength = 50f;
        [Range(1f, 32f)] public float dofAperture = 5.6f;

        [Header("Temperature")]
        public float temperature = 25f;

        [Header("Environment Lighting")]
        [Tooltip("Environment Lighting Intensity Multiplier (Lighting > Environment)")]
        public float environmentIntensity = 1f;

        [Header("Light Color Temperature (Kelvin)")]
        [Tooltip("Sun/Moon light Kelvin temperature - warm (orange) low, cool (blue) high")]
        public float lightKelvin = 5500f;
    }

    [Header("Sky Phases")]
    public SkyPhase[] phases = new SkyPhase[]
    {
        new SkyPhase {
            phaseName = "Day Bright", duration = 120f,
            sunColor = new Color(1f, 0.98f, 0.9f), sunIntensity = 1.5f, targetSunAngleX = 60f,
            fogColor = new Color(0.8f, 0.9f, 1f), fogDensity = 0.002f,
            bloomIntensity = 5f, bloomThreshold = 1.3f, bloomScatter = 0.83f, bloomTint = Color.white,
            postExposure = 0.3f, contrast = -15f, saturation = 10f, hueShift = 0f, colorFilter = Color.white,
            vignetteColor = Color.black, vignetteIntensity = 0.12f, vignetteSmoothness = 0.81f,
            motionBlurIntensity = 0.11f, motionBlurClamp = 0.101f, enableDOF = false,
            temperature = 32f,
            environmentIntensity = 1.3f,
            lightKelvin = 6500f
        },
        new SkyPhase {
            phaseName = "About To Rain", duration = 60f,
            sunColor = new Color(0.8f, 0.8f, 0.85f), sunIntensity = 0.8f, targetSunAngleX = 70f,
            fogColor = new Color(0.6f, 0.65f, 0.7f), fogDensity = 0.015f,
            bloomIntensity = 2f, bloomThreshold = 1.5f, bloomScatter = 0.7f, bloomTint = new Color(0.85f, 0.85f, 1f),
            postExposure = 0f, contrast = -10f, saturation = -20f, hueShift = 0f, colorFilter = new Color(0.85f, 0.88f, 0.95f),
            vignetteColor = Color.black, vignetteIntensity = 0.25f, vignetteSmoothness = 0.8f,
            motionBlurIntensity = 0.08f, motionBlurClamp = 0.1f, enableDOF = false,
            temperature = 24f,
            environmentIntensity = 0.8f,
            lightKelvin = 6800f
        },
        new SkyPhase {
            phaseName = "Rain", duration = 90f,
            sunColor = new Color(0.6f, 0.65f, 0.75f), sunIntensity = 0.4f, targetSunAngleX = 80f,
            fogColor = new Color(0.4f, 0.45f, 0.55f), fogDensity = 0.04f,
            bloomIntensity = 1f, bloomThreshold = 2f, bloomScatter = 0.5f, bloomTint = new Color(0.7f, 0.8f, 1f),
            postExposure = -0.3f, contrast = -20f, saturation = -40f, hueShift = 5f, colorFilter = new Color(0.75f, 0.8f, 0.95f),
            vignetteColor = Color.black, vignetteIntensity = 0.4f, vignetteSmoothness = 0.9f,
            motionBlurIntensity = 0.15f, motionBlurClamp = 0.12f,
            enableDOF = true, dofFocusDistance = 15f, dofFocalLength = 50f, dofAperture = 8f,
            temperature = 18f,
            environmentIntensity = 0.5f,
            lightKelvin = 7500f
        },
        new SkyPhase {
            phaseName = "After The Rain", duration = 60f,
            sunColor = new Color(0.9f, 1f, 0.95f), sunIntensity = 1.1f, targetSunAngleX = 75f,
            fogColor = new Color(0.7f, 0.85f, 0.8f), fogDensity = 0.008f,
            bloomIntensity = 6f, bloomThreshold = 1.1f, bloomScatter = 0.9f, bloomTint = new Color(0.9f, 1f, 0.95f),
            postExposure = 0.4f, contrast = -18f, saturation = 15f, hueShift = -3f, colorFilter = new Color(0.95f, 1f, 0.98f),
            vignetteColor = Color.black, vignetteIntensity = 0.1f, vignetteSmoothness = 0.75f,
            motionBlurIntensity = 0.1f, motionBlurClamp = 0.1f, enableDOF = false,
            temperature = 21f,
            environmentIntensity = 1.0f,
            lightKelvin = 6200f
        },
        new SkyPhase {
            phaseName = "Sunset", duration = 60f,
            sunColor = new Color(1f, 0.5f, 0.1f), sunIntensity = 1.2f, targetSunAngleX = 15f,
            fogColor = new Color(0.9f, 0.5f, 0.3f), fogDensity = 0.012f,
            bloomIntensity = 7f, bloomThreshold = 0.9f, bloomScatter = 0.85f, bloomTint = new Color(1f, 0.6f, 0.2f),
            postExposure = 0.2f, contrast = -10f, saturation = 25f, hueShift = 5f, colorFilter = new Color(1f, 0.85f, 0.7f),
            vignetteColor = new Color(0.3f, 0.05f, 0f), vignetteIntensity = 0.3f, vignetteSmoothness = 0.85f,
            motionBlurIntensity = 0.09f, motionBlurClamp = 0.1f, enableDOF = false,
            temperature = 28f,
            environmentIntensity = 0.9f,
            lightKelvin = 3200f
        },
        new SkyPhase {
            phaseName = "Second Sunset", duration = 60f,
            sunColor = new Color(1f, 0.3f, 0.05f), sunIntensity = 0.8f, targetSunAngleX = 5f,
            fogColor = new Color(0.7f, 0.3f, 0.2f), fogDensity = 0.018f,
            bloomIntensity = 8f, bloomThreshold = 0.8f, bloomScatter = 0.88f, bloomTint = new Color(1f, 0.4f, 0.1f),
            postExposure = 0.1f, contrast = -5f, saturation = 30f, hueShift = 8f, colorFilter = new Color(1f, 0.7f, 0.5f),
            vignetteColor = new Color(0.4f, 0.05f, 0f), vignetteIntensity = 0.4f, vignetteSmoothness = 0.88f,
            motionBlurIntensity = 0.1f, motionBlurClamp = 0.1f, enableDOF = false,
            temperature = 25f,
            environmentIntensity = 0.7f,
            lightKelvin = 2600f
        },
        new SkyPhase {
            phaseName = "Berserk Red Moon", duration = 90f,
            sunColor = new Color(0.8f, 0.05f, 0.05f), sunIntensity = 0.5f, targetSunAngleX = 350f,
            fogColor = new Color(0.4f, 0.02f, 0.02f), fogDensity = 0.025f,
            bloomIntensity = 10f, bloomThreshold = 0.7f, bloomScatter = 0.9f, bloomTint = new Color(1f, 0.1f, 0.1f),
            postExposure = -0.2f, contrast = 15f, saturation = 40f, hueShift = 15f, colorFilter = new Color(1f, 0.5f, 0.5f),
            vignetteColor = new Color(0.6f, 0f, 0f), vignetteIntensity = 0.55f, vignetteSmoothness = 0.95f,
            motionBlurIntensity = 0.18f, motionBlurClamp = 0.15f, enableDOF = false,
            temperature = 35f,
            environmentIntensity = 0.6f,
            lightKelvin = 2000f
        },
        new SkyPhase {
            phaseName = "Night", duration = 120f,
            sunColor = new Color(0.1f, 0.1f, 0.3f), sunIntensity = 0.05f, targetSunAngleX = 300f,
            fogColor = new Color(0.05f, 0.05f, 0.15f), fogDensity = 0.02f,
            bloomIntensity = 2f, bloomThreshold = 0.8f, bloomScatter = 0.7f, bloomTint = new Color(0.5f, 0.5f, 1f),
            postExposure = -0.5f, contrast = 10f, saturation = -15f, hueShift = 0f, colorFilter = new Color(0.6f, 0.6f, 0.9f),
            vignetteColor = Color.black, vignetteIntensity = 0.5f, vignetteSmoothness = 0.9f,
            motionBlurIntensity = 0.05f, motionBlurClamp = 0.08f, enableDOF = false,
            temperature = 15f,
            environmentIntensity = 0.25f,
            lightKelvin = 9000f
        },
        new SkyPhase {
            phaseName = "Sunrise", duration = 60f,
            sunColor = new Color(1f, 0.7f, 0.4f), sunIntensity = 0.9f, targetSunAngleX = 30f,
            fogColor = new Color(0.9f, 0.7f, 0.6f), fogDensity = 0.008f,
            bloomIntensity = 6f, bloomThreshold = 1.0f, bloomScatter = 0.85f, bloomTint = new Color(1f, 0.8f, 0.5f),
            postExposure = 0.1f, contrast = -12f, saturation = 20f, hueShift = -5f, colorFilter = new Color(1f, 0.9f, 0.8f),
            vignetteColor = new Color(0.2f, 0.05f, 0f), vignetteIntensity = 0.2f, vignetteSmoothness = 0.8f,
            motionBlurIntensity = 0.1f, motionBlurClamp = 0.1f, enableDOF = false,
            temperature = 19f,
            environmentIntensity = 0.85f,
            lightKelvin = 3500f
        },
    };

    [Header("Global Settings")]
    public float timeSpeed = 1f;
    public Light directionalLight;
    public Volume globalVolume;
    public bool updateDynamicGI = true;
    public float giUpdateInterval = 0.5f;
    public bool updateFog = true;
    [Tooltip("Uncheck to keep original sun/directional light unchanged")]
    public bool controlDirectionalLight = false;

    public static bool PauseFogUpdates = false;
    public static float CurrentTemperature = 25f;

    private Bloom bloom;
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private DepthOfField depthOfField;
    private MotionBlur motionBlur;

    private float currentTimeInPhase = 0f;
    private int currentPhaseIndex = 0;
    private float giUpdateTimer = 0f;
    private Material blendedSkyboxMaterial;

    void Start()
    {
        if (phases == null || phases.Length == 0)
        {
            Debug.LogError("DayNightController: No sky phases assigned!");
            enabled = false;
            return;
        }

        if (phases[0].skyMaterial != null)
        {
            blendedSkyboxMaterial = new Material(phases[0].skyMaterial);
            blendedSkyboxMaterial.name = "RuntimeBlendedSkybox";
            RenderSettings.skybox = blendedSkyboxMaterial;
        }
        else
        {
            Debug.LogWarning("DayNightController: First phase has no sky material.");
        }

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out bloom);
            globalVolume.profile.TryGet(out colorAdjustments);
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out depthOfField);
            globalVolume.profile.TryGet(out motionBlur);
        }
        else
        {
            Debug.LogWarning("DayNightController: No Global Volume assigned.");
        }
    }

    void Update()
    {
        if (phases.Length == 0) return;

        currentTimeInPhase += Time.deltaTime * timeSpeed;

        int nextPhaseIndex = (currentPhaseIndex + 1) % phases.Length;
        SkyPhase currentPhase = phases[currentPhaseIndex];
        SkyPhase nextPhase = phases[nextPhaseIndex];

        if (currentTimeInPhase >= currentPhase.duration)
        {
            currentTimeInPhase -= currentPhase.duration;
            currentPhaseIndex = nextPhaseIndex;
            Debug.Log("Switched to phase: " + phases[currentPhaseIndex].phaseName);
            nextPhaseIndex = (currentPhaseIndex + 1) % phases.Length;
            currentPhase = phases[currentPhaseIndex];
            nextPhase = phases[nextPhaseIndex];
        }

        float t = currentTimeInPhase / currentPhase.duration;

        // 1. Skybox
        Material targetMat = t < 0.5f ? currentPhase.skyMaterial : nextPhase.skyMaterial;
        if (targetMat != null && RenderSettings.skybox != targetMat)
        {
            RenderSettings.skybox = targetMat;
            DynamicGI.UpdateEnvironment();
        }

        // 2. Sun
        if (directionalLight != null && controlDirectionalLight)
        {
            directionalLight.color = Color.Lerp(currentPhase.sunColor, nextPhase.sunColor, t);
            directionalLight.intensity = Mathf.Lerp(currentPhase.sunIntensity, nextPhase.sunIntensity, t);
            directionalLight.useColorTemperature = true;
            directionalLight.colorTemperature = Mathf.Lerp(currentPhase.lightKelvin, nextPhase.lightKelvin, t);
            int prevPhaseIndex = (currentPhaseIndex - 1 + phases.Length) % phases.Length;
            float startAngleX = phases[prevPhaseIndex].targetSunAngleX;
            float endAngleX = currentPhase.targetSunAngleX;
            if (endAngleX < startAngleX && (startAngleX - endAngleX) > 180f) endAngleX += 360f;
            float currentAngleX = Mathf.Lerp(startAngleX, endAngleX, t);
            Vector3 euler = directionalLight.transform.rotation.eulerAngles;
            directionalLight.transform.rotation = Quaternion.Euler(currentAngleX, euler.y, euler.z);
        }

        // 3. Fog
        if (updateFog && !PauseFogUpdates)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = Color.Lerp(currentPhase.fogColor, nextPhase.fogColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(currentPhase.fogDensity, nextPhase.fogDensity, t);
        }

        // 4. Bloom
        if (bloom != null)
        {
            bloom.intensity.Override(Mathf.Lerp(currentPhase.bloomIntensity, nextPhase.bloomIntensity, t));
            bloom.threshold.Override(Mathf.Lerp(currentPhase.bloomThreshold, nextPhase.bloomThreshold, t));
            bloom.scatter.Override(Mathf.Lerp(currentPhase.bloomScatter, nextPhase.bloomScatter, t));
            bloom.tint.Override(Color.Lerp(currentPhase.bloomTint, nextPhase.bloomTint, t));
        }

        // 5. Color Adjustments
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.Override(Mathf.Lerp(currentPhase.postExposure, nextPhase.postExposure, t));
            colorAdjustments.contrast.Override(Mathf.Lerp(currentPhase.contrast, nextPhase.contrast, t));
            colorAdjustments.saturation.Override(Mathf.Lerp(currentPhase.saturation, nextPhase.saturation, t));
            colorAdjustments.hueShift.Override(Mathf.Lerp(currentPhase.hueShift, nextPhase.hueShift, t));
            colorAdjustments.colorFilter.Override(Color.Lerp(currentPhase.colorFilter, nextPhase.colorFilter, t));
        }

        // 6. Vignette
        if (vignette != null)
        {
            vignette.color.Override(Color.Lerp(currentPhase.vignetteColor, nextPhase.vignetteColor, t));
            vignette.intensity.Override(Mathf.Lerp(currentPhase.vignetteIntensity, nextPhase.vignetteIntensity, t));
            vignette.smoothness.Override(Mathf.Lerp(currentPhase.vignetteSmoothness, nextPhase.vignetteSmoothness, t));
        }

        // 7. Motion Blur
        if (motionBlur != null)
        {
            motionBlur.intensity.Override(Mathf.Lerp(currentPhase.motionBlurIntensity, nextPhase.motionBlurIntensity, t));
            motionBlur.clamp.Override(Mathf.Lerp(currentPhase.motionBlurClamp, nextPhase.motionBlurClamp, t));
        }

        // 8. Depth of Field
        if (depthOfField != null)
        {
            bool useDOF = t > 0.5f ? nextPhase.enableDOF : currentPhase.enableDOF;
            depthOfField.active = useDOF;
            if (useDOF)
            {
                depthOfField.focusDistance.Override(Mathf.Lerp(currentPhase.dofFocusDistance, nextPhase.dofFocusDistance, t));
                depthOfField.focalLength.Override(Mathf.Lerp(currentPhase.dofFocalLength, nextPhase.dofFocalLength, t));
                depthOfField.aperture.Override(Mathf.Lerp(currentPhase.dofAperture, nextPhase.dofAperture, t));
            }
        }

        // 9. Temperature
        CurrentTemperature = Mathf.Lerp(currentPhase.temperature, nextPhase.temperature, t);

        // 10. Environment Lighting Intensity
        RenderSettings.ambientIntensity = Mathf.Lerp(currentPhase.environmentIntensity, nextPhase.environmentIntensity, t);

        // 11. Dynamic GI
        if (updateDynamicGI)
        {
            giUpdateTimer += Time.deltaTime;
            if (giUpdateTimer >= giUpdateInterval)
            {
                giUpdateTimer = 0f;
                DynamicGI.UpdateEnvironment();
            }
        }
    }

    void OnDestroy()
    {
        if (blendedSkyboxMaterial != null)
            Destroy(blendedSkyboxMaterial);
    }
}
