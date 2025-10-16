using System;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class InventoryOverlayCanvas : MonoBehaviour
{
    public static InventoryOverlayCanvas Instance;
    Canvas canvas;

    private void Awake()
    {
        //Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        canvas = GetComponent<Canvas>();

        //Hide canvas by default
        canvas.enabled = false;
    }

    private void Update()
    {
        //Check to see if the player opened the inventory
        if (InputManager.Instance.PlayerOpenInventory())
        {
            //Handle the logic for opening the UI inventory
            //Debug.Log("Opening inventory");
            HandleInventoryOpened();
        }
        else if (InputManager.Instance.UICloseInventory())
        {
            //Debug.Log("Closing inventory");
            HandleInventoryClosed();
        }
    }

    private void HandleInventoryClosed()
    {
        canvas.enabled = false;
        InputManager.Instance.SetControlSchemeToPlayer();
    }

    private void HandleInventoryOpened()
    {
        canvas.enabled = true;
        InputManager.Instance.SetControlSchemeToUI();
    }
}
