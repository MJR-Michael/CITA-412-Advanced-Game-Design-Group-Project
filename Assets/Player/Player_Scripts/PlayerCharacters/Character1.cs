using UnityEngine;

public class Character1 : Player
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void HandleDeath()
    {
        Debug.Log("Player died");
    }
}
