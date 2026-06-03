using UnityEngine;
using System;

namespace AdventureCardGame.Managers
{
    public enum GameState
    {
        Idle,           // Waiting for player action
        ActionPhase,    // Player is dragging cards/equipping
        Combat          // Dice is rolling, input locked
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        public GameState CurrentState { get; private set; }
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        
        private void Start()
        {
            // Force initial camera state
            if (CameraManager.Instance != null) CameraManager.Instance.SwitchToEncounter();
            CurrentState = GameState.Idle;
            OnStateChanged?.Invoke(GameState.Idle);
        }
        
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            CurrentState = newState;
            Debug.Log($"Game State changed to: {newState}");
            
            // Handle Camera Transitions
            if (CameraManager.Instance != null)
            {
                switch (newState)
                {
                    case GameState.Idle:
                        CameraManager.Instance.SwitchToEncounter();
                        break;
                    case GameState.ActionPhase:
                        CameraManager.Instance.SwitchToCombatView();
                        break;
                    case GameState.Combat:
                        CameraManager.Instance.SwitchToDiceRoll();
                        break;
                }
            }
            
            OnStateChanged?.Invoke(newState);
        }
    }
}
