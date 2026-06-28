using UnityEngine;

/// <summary>
/// Editor utility — attach this temporarily to a tree to auto-fix
/// its CapsuleCollider so the player can't walk through it.
/// It sets a proper radius and removes itself after.
/// You only need to run this ONCE per tree in the editor.
/// </summary>
[ExecuteInEditMode]
public class FixTreeCollider : MonoBehaviour
{
    [Tooltip("Radius of the solid blocking collider in metres. " +
             "0.4 works well for Spruce trees.")]
    public float solidRadius = 0.4f;
    public float solidHeight = 10f;
    public Vector3 solidCenter = new Vector3(0, 5, 0);

    [ContextMenu("Fix Collider Now")]
    public void FixCollider()
    {
        CapsuleCollider[] caps = GetComponents<CapsuleCollider>();

        // Remove all existing capsule colliders
        foreach (var c in caps)
            DestroyImmediate(c);

        // Add a proper solid one
        CapsuleCollider solid = gameObject.AddComponent<CapsuleCollider>();
        solid.isTrigger = false;
        solid.radius    = solidRadius;
        solid.height    = solidHeight;
        solid.center    = solidCenter;
        solid.direction = 1; // Y axis

        Debug.Log($"[FixTreeCollider] Fixed collider on {name}. " +
                  "You can now remove this script component.");
    }
}
