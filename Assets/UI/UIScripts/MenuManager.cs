using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [System.Serializable]
    public class Menu
    {
        public string name;
        public GameObject panel;
    }

    [Header("Menu Setup")]
    public List<Menu> menus;
    [Tooltip("The name of your main menu panel.")]
    public string mainMenuName = "MainMenu";

    [Header("Input Settings")]
    [Tooltip("The key used to return to the main menu.")]
    public Key returnToMainKey = Key.Escape;

    private GameObject currentMenu;

    void Start()
    {
        ShowMenu(mainMenuName);
    }

    void Update()
    {
        // If no keyboard is connected or initialized, do nothing
        if (Keyboard.current == null)
            return;

        // Check if the assigned key was pressed this frame
        if (Keyboard.current[returnToMainKey].wasPressedThisFrame)
        {
            // If we're not on the main menu, go back to it
            if (currentMenu == null || currentMenu.name != mainMenuName)
            {
                ShowMenu(mainMenuName);
            }
            // Optional: add quit/confirmation behavior if already on main menu
        }
    }

    public void ShowMenu(string menuName)
    {
        foreach (var m in menus)
        {
            bool shouldShow = m.name == menuName;
            m.panel.SetActive(shouldShow);
            if (shouldShow) currentMenu = m.panel;
        }
    }

    public void BackToMain()
    {
        ShowMenu(mainMenuName);
    }
}
