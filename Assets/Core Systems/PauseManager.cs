using System;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    //For testing purposes only. Unpausing the game will need to be hooked up externally
    [SerializeField]
    bool resumeGame = false;

    public event Action OnGamePaused;
    public event Action OnGameResumed;

    public static PauseManager Instance;
    public bool isPaused;
    bool isMultiplayerMatch;

    private void Awake()
    {
        //Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //Handle logic for if game is a multiplayer game here


        //Handle pause/unpaused logic
        OnGamePaused += HandleGamePaused;
        OnGameResumed += HandleGameResumed;
    }

    void Update()
    {
        //Testing purposes only. Remove when fully implementing pause system
        if (resumeGame)
        {
            ResumeGame();
            resumeGame = false;
        }

        if (isMultiplayerMatch) return; //Cannot pause in multiplayer

        //If playing the game, and player pauses, pause the game
        if (InputManager.Instance.PlayerPause())
        {
            OnGamePaused?.Invoke();
        }
    }

    //Allow for resuming the game by other scripts
    public void ResumeGame() => OnGameResumed?.Invoke();

    private void HandleGamePaused()
    {
        Debug.Log("Game paused");
        InputManager.Instance.SetControlSchemeToUI();
        Time.timeScale = 0;
        isPaused = true;
    }

    private void HandleGameResumed()
    {
        InputManager.Instance.SetControlSchemeToPlayer();
        Time.timeScale = 1;
        isPaused = false;
    }
}
