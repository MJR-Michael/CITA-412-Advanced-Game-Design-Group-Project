using StarterAssets;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    FirstPersonController playerFirstPersonController;

    public void FreezeMovement()
    {
        playerFirstPersonController.CanMove = false;
    }

    public void EnableMovement()
    {
        playerFirstPersonController.CanMove = true;
    }

}
