using UnityEngine;

public class Character2 : Player
{
    protected override void HandleDeath()
    {
        Debug.Log("Player died");
    }
}
