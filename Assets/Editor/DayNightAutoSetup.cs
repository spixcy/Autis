using UnityEngine;
using UnityEditor;

public class DayNightAutoSetup : EditorWindow
{
    [MenuItem("Tools/Auto-Fill Day-Night Phases")]
    public static void AutoFillPhases()
    {
        DayNightController controller = Object.FindAnyObjectByType<DayNightController>();
        
        if (controller == null)
        {
            Debug.LogError("Could not find the DayNightController in the scene! Make sure it is attached to a GameObject.");
            return;
        }

        Undo.RecordObject(controller, "Auto-Fill Day-Night Phases");

        // Set up the array exactly in the chronological order requested
        controller.phases = new DayNightController.SkyPhase[9];

        // 1. Day Bright
        controller.phases[0] = new DayNightController.SkyPhase
        {
            phaseName = "1. Day Bright",
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Fantasy Skybox FREE/finallist/day.mat"),
            duration = 120f,
            targetSunAngleX = 90f, // Sun high in the sky
            sunColor = new Color(1f, 0.95f, 0.9f),
            sunIntensity = 1.2f,
            fogColor = new Color(0.6f, 0.7f, 0.8f),
            fogDensity = 0.005f
        };

        // 2. About to Rain
        controller.phases[1] = new DayNightController.SkyPhase
        {
            phaseName = "2. About to Rain",
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Fantasy Skybox FREE/finallist/about to rain.mat"),
            duration = 40f,
            targetSunAngleX = 110f, // Sun moving slightly down
            sunColor = new Color(0.7f, 0.7f, 0.7f), // Getting cloudy/gray
            sunIntensity = 0.8f,
            fogColor = new Color(0.5f, 0.5f, 0.55f),
            fogDensity = 0.01f
        };

        // 3. Rain
        controller.phases[2] = new DayNightController.SkyPhase
        {
            phaseName = "3. Rain",
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Fantasy Skybox FREE/finallist/rain.mat"),
            duration = 60f,
            targetSunAngleX = 130f, 
            sunColor = new Color(0.5f, 0.5f, 0.6f), // Darker gray
            sunIntensity = 0.4f,
            fogColor = new Color(0.4f, 0.4f, 0.45f),
            fogDensity = 0.02f // Thicker fog for rain
        };

        // 4. After the Rain
        controller.phases[3] = new DayNightController.SkyPhase
        {
            phaseName = "4. After the Rain",
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Fantasy Skybox FREE/finallist/after the rain.mat"),
            duration = 40f,
            targetSunAngleX = 150f, 
            sunColor = new Color(0.8f, 0.85f, 0.9f), // Fresh, clean light
            sunIntensity = 0.9f,
            fogColor = new Color(0.6f, 0.65f, 0.7f),
            fogDensity = 0.01f
        };

        // 5. Sunset
        controller.phases[4] = new DayNightController.SkyPhase
        {
            phaseName = "5. Sunset",
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Fantasy Skybox FREE/finallist/sunset.mat"),
            duration = 45f,
            targetSunAngleX = 175f, // Sun touching the horizon
            sunColor = new Color(1f, 0.5f, 0.2f), // Orange
            sunIntensity = 0.8f,
            fogColor = new Color(0.8f, 0.4f, 0.2f),
            fogDensity = 0.015f
        };

        // 6. Second Sunset
        controller.phases[5] = new DayNightController.SkyPhase
        {
            phaseName = "6. Second Sunset",
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Fantasy Skybox FREE/finallist/second sunset.mat"),
            duration = 30f,
            targetSunAngleX = 185f, // Sun just below horizon
            sunColor = new Color(0.6f, 0.2f, 0.4f), // Purple/Pink dusk
            sunIntensity = 0.3f,
            fogColor = new Color(0.4f, 0.2f, 0.3f),
            fogDensity = 0.015f
        };

        // 7. Beserk (Red Moon)
        controller.phases[6] = new DayNightController.SkyPhase
        {
            phaseName = "7. Beserk (Red Moon)",
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Fantasy Skybox FREE/finallist/beserk.mat"),
            duration = 50f,
            targetSunAngleX = 230f, // Moon rising
            sunColor = new Color(0.8f, 0.1f, 0.1f), // Deep Red
            sunIntensity = 0.5f,
            fogColor = new Color(0.3f, 0.05f, 0.05f), // Creepy red fog
            fogDensity = 0.02f
        };

        // 8. Night
        controller.phases[7] = new DayNightController.SkyPhase
        {
            phaseName = "8. Night",
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Fantasy Skybox FREE/finallist/night.mat"),
            duration = 150f,
            targetSunAngleX = 300f, // Moon high in the sky
            sunColor = new Color(0.2f, 0.3f, 0.5f), // Moonlight blue
            sunIntensity = 0.2f,
            fogColor = new Color(0.05f, 0.05f, 0.1f),
            fogDensity = 0.01f
        };

        // 9. Sunrise
        controller.phases[8] = new DayNightController.SkyPhase
        {
            phaseName = "9. Sunrise",
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Fantasy Skybox FREE/finallist/sunrise.mat"),
            duration = 60f,
            targetSunAngleX = 360f, // Sun coming back up (360 is same as 0)
            sunColor = new Color(1f, 0.7f, 0.4f), // Warm morning orange/yellow
            sunIntensity = 0.7f,
            fogColor = new Color(0.7f, 0.5f, 0.3f),
            fogDensity = 0.015f
        };

        // Mark the script as modified so Unity saves the changes
        EditorUtility.SetDirty(controller);
        
        // Focus the object in the inspector so the user can see it
        Selection.activeGameObject = controller.gameObject;
        
        Debug.Log("Successfully auto-filled all 9 Day/Night phases with their correct skybox materials!");
    }
}
