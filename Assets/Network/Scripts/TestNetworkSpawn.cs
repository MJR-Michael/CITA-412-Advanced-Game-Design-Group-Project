using UnityEngine;
using PurrNet;

public class TestNetworkSpawn : NetworkIdentity
{
    [SerializeField] private NetworkIdentity _networkIdentity;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        // If we're not the host stop here.
        if (!isServer)
            return;
    
        Instantiate(_networkIdentity,Vector3.zero, Quaternion.identity);
    }
}
