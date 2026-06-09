using UnityEngine;

namespace AdventureCardGame.Cards
{
    [CreateAssetMenu(fileName = "Event_Ausgleich", menuName = "Cards/Events/Ausgleich")]
    public class AusgleichEventData : EventCardData
    {
        public override System.Collections.IEnumerator ExecuteEvent(GameObject eventCardObject)
        {
            if (Managers.CameraManager.Instance != null)
            {
                Managers.CameraManager.Instance.SwitchToPlayerView();
                yield return new WaitForSeconds(1.0f); // Wait for camera to pan to the player area
            }
            
            if (Managers.PlayerManager.Instance != null)
            {
                if (Managers.PlayerManager.Instance.Gold == 0)
                {
                    Debug.Log("Ausgleich: Spieler hat 0 Gold. Erhält +2 Gold.");
                    Managers.PlayerManager.Instance.AddGold(2);
                }
                else
                {
                    Debug.Log("Ausgleich: Spieler hat >= 1 Gold. Verliert -2 Gold.");
                    Managers.PlayerManager.Instance.AddGold(-2);
                }
            }

            // Wait briefly to let the player read the new gold amount before returning to whatever is next
            yield return new WaitForSeconds(2.0f);
        }
    }
}
