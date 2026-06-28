using UnityEngine;
using UnityEditor;
using System.IO;

public class URPMaterialUpgrader : EditorWindow
{
    [MenuItem("Tools/Upgrade Low Poly Materials to URP")]
    public static void UpgradeMaterials()
    {
        string folderPath = "Assets/Devtricked/Low-Poly Forest Survival Starter Pack lite";
        
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("[URPMaterialUpgrader] Could not find the Low Poly pack folder: " + folderPath);
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
        Shader urpShader = Shader.Find("Universal Render Pipeline/Simple Lit");

        if (urpShader == null)
        {
            Debug.LogError("[URPMaterialUpgrader] Could not find URP Simple Lit shader. Ensure URP is installed.");
            return;
        }

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader != urpShader)
            {
                // Backup old properties commonly found in Built-In shaders
                Color oldColor = Color.white;
                Texture oldTexture = null;

                if (mat.HasProperty("_Color")) oldColor = mat.GetColor("_Color");
                if (mat.HasProperty("_MainTex")) oldTexture = mat.GetTexture("_MainTex");

                // Switch shader to URP
                mat.shader = urpShader;

                // Apply old properties to new URP property names
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", oldColor);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", oldTexture);

                EditorUtility.SetDirty(mat);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[URPMaterialUpgrader] Successfully upgraded {count} materials in the Low Poly pack to URP Simple Lit!");
    }
}
