using UnityEngine;

public class Character3 : Player
{
    protected override void HandleDeath()
    {
        Debug.Log("Player died");
    }
}
