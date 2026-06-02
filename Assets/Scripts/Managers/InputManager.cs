using UnityEngine;
using UnityEngine.InputSystem;

namespace AdventureCardGame.Managers
{
    public class InputManager : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null && Camera.allCamerasCount > 0)
            {
                mainCamera = Camera.allCameras[0];
            }
        }

        private void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleClick();
            }
        }

        private void HandleClick()
        {
            // Verhindern, dass man durch offene 2D UI-Popups klickt (Inspection Screen)
            if (InspectionManager.Instance != null && InspectionManager.Instance.IsInspecting)
            {
                InspectionManager.Instance.CloseInspection();
                return;
            }

            if (mainCamera == null) return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                var interactable = hit.collider.GetComponent<Cards.CardInteractable>();
                if (interactable != null)
                {
                    interactable.OnClick();
                }
            }
        }
    }
}
