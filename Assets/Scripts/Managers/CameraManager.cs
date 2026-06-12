using UnityEngine;
using Unity.Cinemachine;

namespace AdventureCardGame.Managers
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [Header("Virtual Cameras")]
        public CinemachineCamera camCombatView;
        public CinemachineCamera camEncounter;
        public CinemachineCamera camDiceRoll;
        public CinemachineCamera camTreasure;
        public CinemachineCamera camPlayerView;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void SwitchToCombatView()
        {
            SetAllCamerasPriority(0);
            if (camCombatView != null) camCombatView.Priority = 10;
        }

        public void SwitchToEncounter()
        {
            SetAllCamerasPriority(0);
            if (camEncounter != null) camEncounter.Priority = 10;
        }

        public void SwitchToDiceRoll()
        {
            SetAllCamerasPriority(0);
            if (camDiceRoll != null) camDiceRoll.Priority = 10;
        }

        public void SwitchToTreasure()
        {
            SetAllCamerasPriority(0);
            if (camTreasure != null) camTreasure.Priority = 10;
        }

        public void SwitchToPlayerView()
        {
            SetAllCamerasPriority(0);
            if (camPlayerView != null) camPlayerView.Priority = 10;
        }

        private void SetAllCamerasPriority(int priority)
        {
            if (camCombatView != null) camCombatView.Priority = priority;
            if (camEncounter != null) camEncounter.Priority = priority;
            if (camDiceRoll != null) camDiceRoll.Priority = priority;
            if (camTreasure != null) camTreasure.Priority = priority;
            if (camPlayerView != null) camPlayerView.Priority = priority;
        }

        public bool IsPlayerViewActive => camPlayerView != null && camPlayerView.Priority > 0;
    }
}
