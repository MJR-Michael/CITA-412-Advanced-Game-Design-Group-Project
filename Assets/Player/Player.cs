using StarterAssets;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    FirstPersonController playerFirstPersonController;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FreezeMovement()
    {
        playerFirstPersonController.CanMove = false;
    }

    public void EnableMovement()
    {
        playerFirstPersonController.CanMove = true;
    }

}
