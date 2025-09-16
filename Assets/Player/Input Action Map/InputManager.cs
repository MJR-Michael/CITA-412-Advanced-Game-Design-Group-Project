using UnityEngine;

public class InputManager : MonoBehaviour
{
    //Singleton
    public static InputManager Instance;

    //Player controls
    Controls controls;

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

    private void OnDestroy()
    {
        controls.Player.Disable();
        controls.UI.Disable();
    }


    //Control Schemes
    private void EnableDefaultControls()
    {
        SetControlSchemeToPlayer();
    }

    public void SetControlSchemeToUI()
    {
        controls.Player.Disable();
        controls.UI.Enable();
    }

    public void SetControlSchemeToPlayer()
    {
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
}
