using UnityEngine;
using UnityEditor;

public class WaterVolumeBuilder : EditorWindow
{
    [MenuItem("Tools/Generate Water Volume")]
    public static void GenerateVolume()
    {
        // Use the currently selected object as the water, or fallback to searching by name
        GameObject waterObj = Selection.activeGameObject;
        
        if (waterObj == null) waterObj = GameObject.Find("water");
        if (waterObj == null) waterObj = GameObject.Find("Water");
        
        if (waterObj == null)
        {
            Debug.LogError("No water object found! Please click on your water in the scene, then run this tool again.");
            return;
        }

        // Get the water mesh bounds to size the cube correctly
        MeshFilter meshFilter = waterObj.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("The 'water' object does not have a MeshFilter. Cannot determine size.");
            return;
        }

        Bounds bounds = meshFilter.sharedMesh.bounds;
        Vector3 waterSize = Vector3.Scale(bounds.size, waterObj.transform.lossyScale);

        // Delete existing volume if any
        GameObject existingVol = GameObject.Find("PhysicalWaterVolume");
        if (existingVol != null) DestroyImmediate(existingVol);

        // Create new cube
        GameObject volume = GameObject.CreatePrimitive(PrimitiveType.Cube);
        volume.name = "PhysicalWaterVolume";
        
        // Remove MeshRenderer to make it invisible
        DestroyImmediate(volume.GetComponent<MeshRenderer>());

        // Setup BoxCollider as trigger
        BoxCollider boxCol = volume.GetComponent<BoxCollider>();
        boxCol.isTrigger = true;

        // Position and scale it
        // We want the top of the box to align with the water surface.
        // Let's make the box 20 units deep.
        float depth = 20f;
        volume.transform.position = waterObj.transform.position + new Vector3(0, -depth / 2f, 0);
        
        // We set the scale directly
        volume.transform.localScale = new Vector3(waterSize.x, depth, waterSize.z);

        // Attach WaterVolume script
        volume.AddComponent<WaterVolume>();

        // Set parent to water so they move together
        volume.transform.SetParent(waterObj.transform, true);

        Selection.activeGameObject = volume;
        Debug.Log("Successfully generated PhysicalWaterVolume under 'water'!");
    }
}
