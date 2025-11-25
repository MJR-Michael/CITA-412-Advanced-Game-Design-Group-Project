using UnityEngine;
using PurrNet;

public class TestNetworkHealth : NetworkIdentity
{
    [SerializeField] private SyncVar<int> _health = new(100);       //SyncVar can hold any value, not just integers. It can also be a color, Vector3, etc
    [SerializeField] private int _localHealth = 100;                // not reccomended because we normally want one source of truth which happens in serverRPC

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            SetHealth(_localHealth - 20);
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            TakeDamage(10);
        }
    }

    [ObserversRpc(bufferLast:true)]                                     
    private void SetHealth(int health)
    {
        _localHealth = health;          //we have to make sure that we set health to a value rather than increment it down because with the bufferlast parameter, 
                                        // it buffers the last instruction, NOT the last state of each value. 
                                        // So if the client joins late and we called to reduce the health of a player 5 times by 10.
                                        // Bufferlast will only capture "reduce health by 10" once.
        Debug.Log(_health.value);       //
    }


    [ServerRpc]
    private void TakeDamage(int damage)
    {
        _health.value -= damage;        //remember health is a SyncVar, that CONTAINS an int value, so we have to use _health.value instead of just _health
        Debug.Log(_health.value);       //client side you wont be able to see the Debug logs but on the server you can. this is because the client is sending instructions to the server with ServerRPC.
    }

}