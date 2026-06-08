using UnityEngine;
using UnityEngine.InputSystem;

namespace AdventureCardGame.Managers
{
    public class InputManager : MonoBehaviour
    {
        private Camera mainCamera;
        
        // Drag & Drop State
        private Cards.CardInteractable draggedCard;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Transform originalParent;
        private float pointerDownTime;

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
            if (Mouse.current == null || mainCamera == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandlePointerDown();
            }
            else if (Mouse.current.leftButton.isPressed && draggedCard != null)
            {
                HandlePointerDrag();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && draggedCard != null)
            {
                HandlePointerUp();
            }
        }

        private void HandlePointerDown()
        {
            // Close Inspection if open
            if (InspectionManager.Instance != null && InspectionManager.Instance.IsInspecting)
            {
                InspectionManager.Instance.CloseInspection();
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider.TryGetComponent(out Mechanics.ClickableDeck deck))
                {
                    deck.OnClick();
                    return;
                }
                
                if (hit.collider.TryGetComponent(out Mechanics.ClickableEventCard eventCard))
                {
                    eventCard.OnClick();
                    return;
                }

                var treasureDeck = hit.collider.GetComponentInParent<Mechanics.ClickableTreasureDeck>();
                if (treasureDeck != null)
                {
                    treasureDeck.OnClick();
                    return;
                }

                // Fallback: If component is missing but we hit something named TreasureDeck
                if (hit.collider.name.Contains("TreasureDeck") || (hit.collider.transform.parent != null && hit.collider.transform.parent.name.Contains("TreasureDeck")))
                {
                    var table = FindAnyObjectByType<Managers.TableLayoutManager>();
                    if (table != null && table.CanDrawTreasure)
                    {
                        table.DrawTreasure();
                        return;
                    }
                }

                // Block all normal card interactions during the entire combat routine
                if (CombatManager.Instance != null && CombatManager.Instance.IsCombatRunning)
                    return;

                var interactable = hit.collider.GetComponent<Cards.CardInteractable>();
                if (interactable != null)
                {
                    // Check if it's face up (Canvas enabled)
                    Canvas c = interactable.GetComponentInChildren<Canvas>();
                    if (c != null && !c.enabled) return;

                    // Grab the card
                    draggedCard = interactable;
                    originalPosition = draggedCard.transform.position;
                    originalRotation = draggedCard.transform.rotation;
                    originalParent = draggedCard.transform.parent;
                    pointerDownTime = Time.time;
                    
                    // Optional: disable physics or interactable logic if needed
                    // For now, just remember we are dragging it
                }
            }
        }

        private void HandlePointerDrag()
        {
            var display = draggedCard.GetComponent<Cards.CardDisplay>();
            if (display != null && !(display.cardData is Cards.MemberCardData))
            {
                return; // Only allow dragging Member cards
            }

            // Use the original height plus a small offset
            float currentDragHeight = originalPosition.y + 0.2f;
            Plane dragPlane = new Plane(Vector3.up, new Vector3(0, currentDragHeight, 0));
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (dragPlane.Raycast(ray, out float distance))
            {
                Vector3 targetPos = ray.GetPoint(distance);
                draggedCard.transform.position = Vector3.Lerp(draggedCard.transform.position, targetPos, Time.deltaTime * 15f);
                // Maintain the original flat rotation, but add a slight tilt to show it's picked up
                draggedCard.transform.rotation = Quaternion.Lerp(draggedCard.transform.rotation, originalRotation * Quaternion.Euler(-15f, 0, 0), Time.deltaTime * 10f);
            }
        }

        private void HandlePointerUp()
        {
            // Check what we dropped it on
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            // Ignore the dragged card itself by temporarily disabling its collider
            Collider draggedCollider = draggedCard.GetComponent<Collider>();
            if (draggedCollider != null) draggedCollider.enabled = false;

            bool attacked = false;

            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
            foreach (var hit in hits)
            {
                var targetInteractable = hit.collider.GetComponent<Cards.CardInteractable>();
                if (targetInteractable != null && targetInteractable != draggedCard)
                {
                    var targetDisplay = targetInteractable.GetComponent<Cards.CardDisplay>();
                    var draggedDisplay = draggedCard.GetComponent<Cards.CardDisplay>();
                    
                    // If dragged a Member onto a Monster
                    if (draggedDisplay != null && draggedDisplay.cardData is Cards.MemberCardData memberData &&
                        targetDisplay != null && targetDisplay.cardData is Cards.MonsterCardData monsterData)
                    {
                        if (CombatManager.Instance != null)
                        {
                            attacked = true;
                            // Snap back immediately visually
                            draggedCard.transform.position = originalPosition;
                            draggedCard.transform.rotation = originalRotation;
                            
                            CombatManager.Instance.ResolveCombat(draggedCard.gameObject, targetInteractable.gameObject);
                            break; // Stop checking other hits once we successfully attacked
                        }
                    }
                }
            }

            if (draggedCollider != null) draggedCollider.enabled = true;

            if (!attacked)
            {
                // Snap back or inspect
                // If it was a quick click, inspect it
                if (Time.time - pointerDownTime < 0.25f)
                {
                    draggedCard.transform.position = originalPosition;
                    draggedCard.transform.rotation = originalRotation;
                    draggedCard.OnClick();
                }
                else
                {
                    // Snap back
                    draggedCard.transform.position = originalPosition;
                    draggedCard.transform.rotation = originalRotation;
                }
            }

            draggedCard = null;
        }
    }
}
