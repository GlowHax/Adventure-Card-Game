using UnityEngine;
using UnityEditor;

public class ApplyLivingRoomLook
{
    [MenuItem("Tools/Apply Living Room")]
    public static void Execute()
    {
        AssetDatabase.Refresh();
        
        // 1. Create Material
        string texPath = "Assets/Textures/WoodTexture.png";
        Texture2D woodTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (woodTex == null) 
        {
            Debug.LogError("Wood texture not found at " + texPath);
            return;
        }
        
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
            
        string matPath = "Assets/Materials/TableWoodMat.mat";
        Material woodMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (woodMat == null)
        {
            woodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(woodMat, matPath);
        }
        woodMat.shader = Shader.Find("Universal Render Pipeline/Lit");
        woodMat.SetTexture("_BaseMap", woodTex);
        woodMat.SetFloat("_Smoothness", 0.4f); // URP uses _Smoothness instead of _Glossiness
        woodMat.SetFloat("_Metallic", 0.0f);
        woodMat.SetColor("_BaseColor", new Color(0.85f, 0.75f, 0.65f)); // URP uses _BaseColor
        
        EditorUtility.SetDirty(woodMat);
        // 2. Find Table and reset others
        GameObject tableObj = GameObject.Find("MainTable");
        GameObject coinBox = GameObject.Find("CoinBox");
        GameObject honorBowl = GameObject.Find("HonorBowl");

        if (tableObj != null)
        {
            var mr = tableObj.GetComponent<MeshRenderer>();
            if (mr != null) 
            {
                mr.sharedMaterial = woodMat;
                
                // Setze Tiling so, dass die Textur genau einmal über den ganzen Tisch gestreckt wird
                // Da der Tisch ca. 8.4 x 4.4 groß ist, berechnen wir die passenden UV-Skalierungen
                float tileX = 1f;
                float tileY = 1f;
                
                if (mr.bounds.size.x > 0 && mr.bounds.size.z > 0)
                {
                    // Wir nehmen an, dass die UVs im Verhältnis zur Größe stehen.
                    // Wenn wir Tiling reduzieren, strecken wir das Bild.
                    tileX = 1f / (mr.bounds.size.x / 10f); // just a heuristic 
                    tileY = 1f / (mr.bounds.size.z / 10f);
                }
                
                // Die neue Textur ist echtes, flaches Holz. Wir reduzieren die Kachelung massiv auf 0.1x0.1,
                // um die Holzmaserung extrem groß zu ziehen, sodass der Tisch nicht mehr gigantisch wirkt.
                woodMat.mainTextureScale = new Vector2(0.1f, 0.1f); 
                Debug.Log("Applied material to " + tableObj.name);
            }
        }
        else
        {
            Debug.LogError("Table not found!");
        }

        // Reset the bowls and boxes to a default material so they don't look like wood
        Material defaultMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        defaultMat.SetColor("_BaseColor", new Color(0.7f, 0.7f, 0.7f)); // Light gray
        
        if (coinBox != null)
        {
            var mr = coinBox.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = defaultMat;
        }
        if (honorBowl != null)
        {
            var mr = honorBowl.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = defaultMat;
        }
        
        // 3. Adjust Lighting
        Light dirLight = null;
        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light.type == LightType.Directional)
            {
                dirLight = light;
                break;
            }
        }
        
        if (dirLight != null)
        {
            dirLight.color = new Color(1.0f, 0.85f, 0.7f); // warm yellow/orange
            dirLight.intensity = 0.9f;
            dirLight.shadowStrength = 0.8f;
            Debug.Log("Adjusted Directional Light.");
        }
        
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.4f, 0.3f, 0.2f); // warm ambient
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }
}
