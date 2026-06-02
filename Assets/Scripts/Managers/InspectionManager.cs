using UnityEngine;
using AdventureCardGame.Cards;

namespace AdventureCardGame.Managers
{
    public class InspectionManager : MonoBehaviour
    {
        public static InspectionManager Instance { get; private set; }

        private GameObject inspectedCardClone;

        public bool IsInspecting => inspectedCardClone != null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void InspectCard(GameObject originalCard)
        {
            // Close any existing inspection
            if (IsInspecting)
            {
                CloseInspection();
                return; // If we clicked another card while inspecting, we just close the old one. Optionally we could inspect the new one.
            }

            if (originalCard == null) return;
            if (Camera.main == null) return;

            // Clone the card
            inspectedCardClone = Instantiate(originalCard);
            inspectedCardClone.name = originalCard.name + "_InspectionClone";

            // Put it on a top sorting layer or just move it close to camera
            // 0.8f units in front of the camera
            Vector3 targetPos = Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
            inspectedCardClone.transform.position = targetPos;
            
            // Rotate it to face the camera perfectly
            // If the card UI is on the XY plane facing -Z, matching the camera's rotation exactly
            // will make the card's -Z point towards the camera, meaning it perfectly faces the camera and is upright.
            inspectedCardClone.transform.rotation = Camera.main.transform.rotation;

            // Disable its interactable component so clicking the clone doesn't trigger inspection again
            var interactable = inspectedCardClone.GetComponent<CardInteractable>();
            if (interactable != null)
            {
                Destroy(interactable);
            }
            
            // Disable its collider so it doesn't block other raycasts (or keep it if you want to click it to close)
            var collider = inspectedCardClone.GetComponent<Collider>();
            if (collider != null)
            {
                // We leave the collider so the InputManager can hit it, but it has no CardInteractable.
                // InputManager will see IsInspecting is true and close it anyway.
            }
        }

        public void CloseInspection()
        {
            if (inspectedCardClone != null)
            {
                Destroy(inspectedCardClone);
                inspectedCardClone = null;
            }
        }
    }
}
