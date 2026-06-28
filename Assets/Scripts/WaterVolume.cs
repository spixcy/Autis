using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using StarterAssets;

[RequireComponent(typeof(BoxCollider))]
public class WaterVolume : MonoBehaviour
{
    [Header("Underwater Physics")]
    [Tooltip("The gravity applied to the player while underwater. Closer to 0 means floatier.")]
    public float underwaterGravity = -4.0f;
    [Tooltip("Player walking speed in water.")]
    public float underwaterMoveSpeed = 2.0f;
    [Tooltip("Player sprinting speed in water.")]
    public float underwaterSprintSpeed = 3.0f;

    [Header("Underwater Visuals")]
    public Color waterColor = new Color(0.1f, 0.4f, 0.6f);
    public float fogDensity = 0.05f;

    private Volume underwaterVolume;

    // Cache default physics values
    private float defaultGravity;
    private float defaultMoveSpeed;
    private float defaultSprintSpeed;
    
    // Cache default visual values
    private bool defaultFogState;
    private Color defaultFogColor;
    private float defaultFogDensity;

    private FirstPersonController playerController;

    void Start()
    {
        // 1. Create Post-Processing Volume Dynamically
        GameObject volObj = new GameObject("Water_PostProcessingVolume");
        volObj.transform.SetParent(this.transform);
        underwaterVolume = volObj.AddComponent<Volume>();
        underwaterVolume.isGlobal = true;
        underwaterVolume.weight = 0f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        
        ColorAdjustments colorAdj = ScriptableObject.CreateInstance<ColorAdjustments>();
        colorAdj.colorFilter.Override(waterColor);
        colorAdj.postExposure.Override(-0.5f);
        profile.components.Add(colorAdj);
        
        DepthOfField dof = ScriptableObject.CreateInstance<DepthOfField>();
        dof.mode.Override(DepthOfFieldMode.Bokeh);
        dof.focusDistance.Override(0.1f);
        dof.focalLength.Override(50f);
        dof.aperture.Override(2.8f);
        profile.components.Add(dof);

        Vignette vig = ScriptableObject.CreateInstance<Vignette>();
        vig.intensity.Override(0.4f);
        vig.color.Override(new Color(0f, 0.1f, 0.2f));
        profile.components.Add(vig);

        underwaterVolume.profile = profile;

        // Force camera settings globally once, if camera is found
        if (Camera.main != null)
        {
            var cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData != null)
            {
                cameraData.renderPostProcessing = true;
                cameraData.requiresDepthOption = CameraOverrideOption.On;
            }
        }
    }

    private int collidersInside = 0;

    void OnTriggerEnter(Collider other)
    {
        // Check if the entering object has the FirstPersonController script
        FirstPersonController fpc = other.GetComponent<FirstPersonController>();
        if (fpc == null) fpc = other.GetComponentInParent<FirstPersonController>();

        if (fpc != null)
        {
            collidersInside++;
            
            // Only capture defaults and apply effects if this is the FIRST collider entering
            if (collidersInside == 1)
            {
                playerController = fpc;
                
                // Save original physics
                defaultGravity = playerController.Gravity;
                defaultMoveSpeed = playerController.MoveSpeed;
                defaultSprintSpeed = playerController.SprintSpeed;

                // Apply underwater physics
                playerController.Gravity = underwaterGravity;
                playerController.MoveSpeed = underwaterMoveSpeed;
                playerController.SprintSpeed = underwaterSprintSpeed;

                // Apply Visuals
                underwaterVolume.weight = 1f;

                defaultFogState = RenderSettings.fog;
                defaultFogColor = RenderSettings.fogColor;
                defaultFogDensity = RenderSettings.fogDensity;

                // Stop the day/night cycle from changing the fog
                DayNightController.PauseFogUpdates = true;

                RenderSettings.fog = true;
                RenderSettings.fogColor = waterColor;
                RenderSettings.fogDensity = fogDensity;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        FirstPersonController fpc = other.GetComponent<FirstPersonController>();
        if (fpc == null) fpc = other.GetComponentInParent<FirstPersonController>();

        if (fpc != null)
        {
            collidersInside--;
            
            // Only restore if ALL colliders have exited
            if (collidersInside <= 0)
            {
                collidersInside = 0; // prevent negative
                
                if (playerController != null)
                {
                    // Restore original physics
                    playerController.Gravity = defaultGravity;
                    playerController.MoveSpeed = defaultMoveSpeed;
                    playerController.SprintSpeed = defaultSprintSpeed;
                    playerController = null;
                }

                // Restore Visuals
                underwaterVolume.weight = 0f;
                RenderSettings.fog = defaultFogState;
                RenderSettings.fogColor = defaultFogColor;
                RenderSettings.fogDensity = defaultFogDensity;
                
                // Allow the day/night cycle to resume controlling the fog
                DayNightController.PauseFogUpdates = false;
            }
        }
    }
}
