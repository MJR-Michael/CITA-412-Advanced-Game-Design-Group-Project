using UnityEngine;
using PurrNet;

public class TestNetworkColor : NetworkIdentity
{
    [SerializeField] Color _color;
    [SerializeField] Renderer _renderer;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
            SetColor(_color);

    }

    [ServerRpc]
    // doing it this ways allows us to set local parameters from different clients
    // if the color for client1 is yellow, and client 1 activates SetColor. Then the server will show yellow even if the server has Blue as their color.
    //This is because ServerRPC is client -> server instructions. and in our script we're taking the reference of whats taken in the serialized field of the client.


    //  [ObserverRPC(bufferLast:true)]        this allows any new clients who have joined after an instruction was sent to the client, to still recieve those new instructions.
    //  This is a simple level of syncronizing


    private void SetColor(Color color)
    {
        _renderer.material.color = color;
    }
}