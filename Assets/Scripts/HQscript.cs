using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HQscript : NetworkBehaviour
{
    public NetworkVariable<int> hqHealth = new NetworkVariable<int>();
    void Start()
    {
        hqHealth.Value = 100;
    }

    public void TakeDamage(int mag)
    {
        hqHealth.Value -= mag;
        if (hqHealth.Value <= 0)
        {
            Debug.Log("client " + NetworkObject.OwnerClientId + " lost!");
        }
    }
}
