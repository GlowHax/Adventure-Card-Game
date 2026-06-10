using UnityEngine;
using UnityEditor;

public class ApplyCardMaterial
{
    [MenuItem("Tools/Apply Card Base Material")]
    public static void Execute()
    {
        AssetDatabase.Refresh();

        string albedoPath = "Assets/Textures/PaperAlbedo.png";
        string heightmapPath = "Assets/Textures/PaperHeightmap.png";

        // Setup Normal Map Importer
        TextureImporter importer = AssetImporter.GetAtPath(heightmapPath) as TextureImporter;
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.NormalMap || !importer.convertToNormalmap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.convertToNormalmap = true;
                importer.heightmapScale = 0.05f; // Very subtle bump for smooth paper
                importer.SaveAndReimport();
            }
        }

        Texture2D albedoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
        Texture2D normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(heightmapPath);

        if (albedoTex == null || normalTex == null)
        {
            Debug.LogError("Textures not found!");
            return;
        }

        string matPath = "Assets/Materials/CardBaseMat.mat";
        Material cardMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (cardMat == null)
        {
            cardMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(cardMat, matPath);
        }

        cardMat.shader = Shader.Find("Universal Render Pipeline/Lit");
        cardMat.SetTexture("_BaseMap", albedoTex);
        cardMat.SetTexture("_BumpMap", null); // Remove normal map to ensure no artificial noise
        cardMat.SetFloat("_Smoothness", 0.25f); // Smooth high-quality cardstock
        cardMat.SetFloat("_Metallic", 0.0f);
        cardMat.SetColor("_BaseColor", Color.white); 
        
        // Disable normal map keyword
        cardMat.DisableKeyword("_NORMALMAP");

        EditorUtility.SetDirty(cardMat);
        AssetDatabase.SaveAssets();

        // Apply to Prefabs
        string[] prefabs = {
            "Assets/Prefabs/CardPrefab_Member.prefab",
            "Assets/Prefabs/CardPrefab_Monster.prefab",
            "Assets/Prefabs/CardPrefab_Event.prefab",
            "Assets/Prefabs/CardPrefab_Equipment.prefab",
            "Assets/Prefabs/CardPrefab_Treasure.prefab"
        };

        foreach (string path in prefabs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                Transform meshTransform = prefab.transform.Find("CardMesh");
                if (meshTransform != null)
                {
                    MeshRenderer mr = meshTransform.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.sharedMaterial = cardMat;
                        EditorUtility.SetDirty(prefab);
                        Debug.Log("Applied to " + prefab.name);
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Card Base Material applied successfully.");
    }
}
