using UnityEngine;
using UnityEngine.UI;

namespace AdventureCardGame.Cards
{
    public class CanvasToRenderTexture : MonoBehaviour
    {
        private RenderTexture rt;
        private GameObject quad;
        private Camera renderCam;
        private Canvas canvas;

        void Start()
        {
            canvas = GetComponentInChildren<Canvas>();
            if (canvas == null || canvas.renderMode != RenderMode.WorldSpace) return;

            if (canvas.name.EndsWith("_RenderTextureApplied")) return;
            canvas.name += "_RenderTextureApplied";

            RectTransform rect = canvas.GetComponent<RectTransform>();
            
            // 1. Create Render Texture
            int width = 768;
            int height = Mathf.RoundToInt(width * (rect.rect.height / rect.rect.width));
            
            rt = new RenderTexture(width, height, 24);
            rt.Create();

            // 2. Create the Quad to display it in the world
            quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Canvas3DQuad";
            
            quad.transform.SetParent(canvas.transform.parent, false);
            quad.transform.localPosition = canvas.transform.localPosition;
            quad.transform.localRotation = canvas.transform.localRotation;
            
            quad.transform.localScale = new Vector3(
                rect.rect.width * canvas.transform.localScale.x,
                rect.rect.height * canvas.transform.localScale.y,
                1f);
                
            Material litMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            litMat.SetTexture("_BaseMap", rt);
            litMat.SetFloat("_Smoothness", 0.35f); // Let it shine slightly
            quad.GetComponent<MeshRenderer>().sharedMaterial = litMat;
            
            Destroy(quad.GetComponent<Collider>());

            // 3. Setup Camera (Parent it to the card root, not the scaled Canvas)
            GameObject camObj = new GameObject("UICamera");
            camObj.transform.SetParent(canvas.transform.parent, false);
            camObj.transform.localPosition = canvas.transform.localPosition + new Vector3(0, 0, -1f); 
            camObj.transform.localRotation = canvas.transform.localRotation;
            
            renderCam = camObj.AddComponent<Camera>();
            renderCam.orthographic = true;
            // The orthographic size is half the world-space height of the canvas
            renderCam.orthographicSize = rect.rect.height * canvas.transform.localScale.y * 0.5f; 
            renderCam.targetTexture = rt;
            renderCam.clearFlags = CameraClearFlags.SolidColor;
            
            // Background should be transparent or match card background. Let's use a dark gray just in case.
            renderCam.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f); 
            renderCam.nearClipPlane = 0.01f;
            renderCam.farClipPlane = 2f;
            
            // 4. Isolate the Canvas and Camera
            int uiLayer = LayerMask.NameToLayer("UI");
            SetLayerRecursively(canvas.gameObject, uiLayer);
            renderCam.cullingMask = 1 << uiLayer;
            
            // 5. Move both Canvas and Camera far away so they don't overlap with the physical game board
            float uniqueOffset = gameObject.GetInstanceID() % 2000f;
            Vector3 hiddenPos = new Vector3(uniqueOffset * 10f, -5000f - uniqueOffset * 10f, 0);
            
            canvas.transform.localPosition = hiddenPos;
            camObj.transform.localPosition = hiddenPos + new Vector3(0, 0, -1f);
        }

        void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (null == child) continue;
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        
        void OnDestroy()
        {
            if (rt != null) rt.Release();
        }
    }
}
