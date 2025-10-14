using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public enum InputControlScheme
    {
        Player,
        UI
    }

    [System.Serializable]
    public class SceneControlBinding
    {
        public SceneReferenceSO scene;
        public InputControlScheme inputControlScheme;
    }


    [SerializeField, Tooltip("Stores reference to scenes to handle their initial logic for loading into the scene. For example, " +
        "loading into the main menu should enable UI control scheme, and loading into the game scene should enable play control scheme")]
    SceneControlBinding[] scenesInBuildIndex;


    //Singleton
    public static InputManager Instance;

    //Player controls
    Controls controls;

    bool initialized = false;

    private void Awake()
    {
        //Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Warning: Multiple input managers found in scene!");
            Destroy(gameObject);
        }

        //Craete player controls
        controls = new Controls();

        initialized = true;

        //Set controls to default
        EnableDefaultControls();
    }

    //Input debugging
    /*
     
    private void Update()
    {
        Debug.Log($"Player jumped: {InputManager.Instance.PlayerJump()}");
        Debug.Log($"Player Moved: {InputManager.Instance.GetPlayerMovement()}");
        Debug.Log($"Player looked: {InputManager.Instance.GetPlayerLook()}");
        Debug.Log($"Player primary fire: {InputManager.Instance.GetPrimaryFireInput()}");
        Debug.Log($"Player secondary fire: {InputManager.Instance.getSecondaryFireInput()}");
        Debug.Log($"Player ability 1: {InputManager.Instance.PlayerAbility1()}");
        Debug.Log($"Player ability 2: {InputManager.Instance.PlayerAbility2()}");
        Debug.Log($"Player ability 3: {InputManager.Instance.PlayerAbility3()}");
        Debug.Log($"Player paused: {InputManager.Instance.PlayerPause()}");

        if (InputManager.Instance.PlayerPause())
        {
            Debug.Log("Player paused game. Switching controls scheme to UI");
        }

        Debug.Log($"UI Navigation: {InputManager.Instance.GetUINavigation()}");
        Debug.Log($"UI select: {InputManager.Instance.UISelect()}");
        Debug.Log($"UI back: {InputManager.Instance.UIBack()}");

        if (InputManager.Instance.UIBack())
        {
            Debug.Log("Leaving UI controls... returning to player controls.");
        }
    }

    */

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        if (!initialized) { return; }

        controls.Player.Disable();
        controls.UI.Disable();
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (!initialized) { return; }

        controls.Player.Disable();
        controls.UI.Disable();
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        controls.Dispose();
    }

    //Subscription events
    private void HandleSceneLoaded(Scene newScene, LoadSceneMode arg1)
    {
        bool controlSchemeApplied = false;

        //Check if the scene has a certain control scheme to initialize as
        foreach (SceneControlBinding sceneControlBinding in scenesInBuildIndex)
        {
            //Get the scene name
            string sceneName = sceneControlBinding.scene.SceneName;

            //Check if the scene name is the same as the new scene's name
            if (newScene.name.CompareTo(sceneName) != 0) continue;

            //Control scheme will be applied
            controlSchemeApplied = true;

            //Enable the corresponing input scheme based on the scene's control binding
            switch (sceneControlBinding.inputControlScheme)
            {
                case InputControlScheme.Player:
                    SetControlSchemeToPlayer();
                    break;
                case InputControlScheme.UI:
                    SetControlSchemeToUI(); 
                    break;
                default:
                    EnableDefaultControls();
                    break;
            }
        }

        //Check if the control scheme was applied
        if (!controlSchemeApplied)
        {
            //Apply default controls on scene load
            Debug.LogWarning($"Warning: No control scheme given for scene, '{newScene.name}'");
            EnableDefaultControls();
        }
    }


    //Control Schemes
    private void EnableDefaultControls()
    {
        SetControlSchemeToPlayer();
    }

    public void SetControlSchemeToUI()
    {
        //When the player is in the UI menus, they should see their mouse (or the mouse sprite if we add one) and their mouse position must be unlocked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        controls.Player.Disable();
        controls.UI.Enable();
    }

    public void SetControlSchemeToPlayer()
    {
        //When the player is playing the game, they should not see their mouse, and the mouse position must be locked in place.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controls.Player.Enable();
        controls.UI.Disable();
    }


    //Getting Player Input
    public Vector2 GetPlayerMovement()
    {
        //Debug.Log($"{controls.Player.Move.ReadValue<Vector2>()}");
        return controls.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetPlayerLook()
    {
        //Debug.Log($"{controls.Player.Look.ReadValue<Vector2>()}");
        return controls.Player.Look.ReadValue<Vector2>();
    }

    public bool PlayerJump()
    {
        //Debug.Log($"{controls.Player.Jump.ReadValue<float>()}");
        return controls.Player.Jump.ReadValue<float>() == 1f ? true : false;
    }

    public bool PlayerAbility1()
    {
        return controls.Player.Ability1.ReadValue<float>() == 1f? true : false;
    }

    public bool PlayerAbility2()
    {
        return controls.Player.Ability2.ReadValue<float>() == 1f ? true : false;
    }

    public bool PlayerAbility3()
    {
        return controls.Player.Ability3.ReadValue<float>() == 1f ? true : false;
    }

    public bool PlayerPause()
    {
        return controls.Player.Pause.ReadValue<float>() == 1f? true : false;
    }

    public bool PlayerOpenInventory()
    {
        return controls.Player.Inventory.ReadValue<float>() == 1f? true : false;
    }

    public float GetPrimaryFireInput()
    {
        return controls.Player.PrimaryFire.ReadValue<float>();
    }

    public float getSecondaryFireInput()
    {
        return controls.Player.SecondaryFire.ReadValue<float>();
    }


    //Getting UI Input
    public Vector2 GetUINavigation()
    {
        return controls.UI.Navigation.ReadValue<Vector2>();
    }

    public bool UISelect()
    {
        return controls.UI.Select.ReadValue<float>() == 1f? true : false;
    }

    public bool UIBack()
    {
        return controls.UI.Back.ReadValue<float>() == 1f ? true : false;
    }

    public bool UICloseInventory()
    {
        return controls.UI.Inventory.ReadValue<float>() == 1f ? true : false;
    }
}
