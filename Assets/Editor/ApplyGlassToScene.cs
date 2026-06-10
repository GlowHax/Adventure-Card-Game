using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class ApplyGlassToScene
{
    [MenuItem("Tools/Apply Glass To Scene Objects")]
    public static void Execute()
    {
        string matPath = "Assets/Materials/GlassMat.mat";
        Material glassMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        
        if (glassMat == null) return;

        // Ensure URP Transparent settings are PERFECTLY set
        glassMat.SetFloat("_Surface", 1.0f); // Transparent
        glassMat.SetFloat("_Blend", 0.0f); // Alpha
        glassMat.SetColor("_BaseColor", new Color(1.0f, 0.95f, 0.85f, 0.15f)); // Slightly more visible alpha
        glassMat.SetFloat("_Smoothness", 0.95f);
        glassMat.SetFloat("_Metallic", 0.0f); // Glass is strictly non-metallic! This prevents the mirror-like blue sky reflection
        glassMat.SetFloat("_Cull", 0.0f); // Both faces
        glassMat.SetFloat("_ZWrite", 0.0f); 
        
        glassMat.SetOverrideTag("RenderType", "Transparent");
        glassMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        glassMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        glassMat.SetInt("_ZWrite", 0);
        glassMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        glassMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        glassMat.DisableKeyword("_ALPHATEST_ON");
        glassMat.EnableKeyword("_ALPHABLEND_ON"); // THIS IS CRITICAL FOR URP LIT TRANSPARENCY!
        glassMat.renderQueue = (int)RenderQueue.Transparent;

        EditorUtility.SetDirty(glassMat);
        AssetDatabase.SaveAssets();

        // Apply to everything in the scene that looks like a CoinBox
        MeshRenderer[] allRenderers = GameObject.FindObjectsOfType<MeshRenderer>();
        int count = 0;
        foreach (MeshRenderer mr in allRenderers)
        {
            if (mr.gameObject.name.Contains("CoinBox"))
            {
                mr.sharedMaterial = glassMat;
                EditorUtility.SetDirty(mr.gameObject);
                count++;
            }
        }
        
        Debug.Log("Applied Glass Material to " + count + " CoinBoxes in the scene.");
    }
}
