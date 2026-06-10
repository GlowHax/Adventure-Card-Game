using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class ApplyGlassMaterial
{
    [MenuItem("Tools/Apply Glass Material")]
    public static void Execute()
    {
        string matPath = "Assets/Materials/GlassMat.mat";
        Material glassMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        
        if (glassMat == null)
        {
            glassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(glassMat, matPath);
        }

        glassMat.shader = Shader.Find("Universal Render Pipeline/Lit");
        
        // Setup URP Transparent Glass
        glassMat.SetFloat("_Surface", 1); // 1 = Transparent
        glassMat.SetFloat("_Blend", 0); // 0 = Alpha
        // Warm tint to match the room, almost completely transparent (0.05 Alpha)
        glassMat.SetColor("_BaseColor", new Color(1.0f, 0.95f, 0.85f, 0.05f)); 
        glassMat.SetFloat("_Smoothness", 0.98f); // Extremely smooth for sharp reflections
        glassMat.SetFloat("_Metallic", 0.4f); // Metallic increases reflection intensity
        glassMat.SetFloat("_Cull", 0); // Render both faces
        glassMat.SetFloat("_ZWrite", 0); 
        glassMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        glassMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        glassMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        glassMat.EnableKeyword("_ALPHAPREMULTIPLY_ON"); // Helps with glass rendering
        glassMat.renderQueue = (int)RenderQueue.Transparent;

        EditorUtility.SetDirty(glassMat);
        AssetDatabase.SaveAssets();

        // Apply to CoinBox Prefab
        string prefabPath = "Assets/Prefabs/Rewards/CoinBox.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            // Apply to all MeshRenderers in the prefab (usually there's one on the root or children)
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var mr in renderers)
            {
                mr.sharedMaterial = glassMat;
                EditorUtility.SetDirty(prefab);
                Debug.Log("Applied Glass material to CoinBox renderer: " + mr.gameObject.name);
            }
        }
        else
        {
            Debug.LogError("CoinBox prefab not found at " + prefabPath);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Glass Material applied successfully.");
    }
}
