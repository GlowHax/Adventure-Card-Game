using UnityEngine;
using UnityEditor;

public class CheckHierarchy
{
    [MenuItem("Tools/Check Hierarchy")]
    public static void Execute()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CardPrefab_Member.prefab");
        if (prefab == null) 
        {
            Debug.LogError("Prefab not found!");
            return;
        }
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        PrintTransform(prefab.transform, "", sb);
        Debug.Log(sb.ToString());
    }
    
    static void PrintTransform(Transform t, string indent, System.Text.StringBuilder sb)
    {
        sb.AppendLine(indent + t.name + " (" + (t.GetComponent<Canvas>() != null ? "Canvas" : "No Canvas") + ", " + (t.GetComponent<MeshRenderer>() != null ? "Mesh" : "No Mesh") + ", " + (t.GetComponent<Collider>() != null ? "Collider" : "No Collider") + ")");
        foreach (Transform child in t)
        {
            PrintTransform(child, indent + "  ", sb);
        }
    }
}
