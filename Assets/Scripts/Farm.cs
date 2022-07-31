using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Farm : NetworkBehaviour
{
    PlayerC myPlayer;

    public float growthFactor = 1;
    public NetworkVariable<bool> ripe = new NetworkVariable<bool>(false);
    public NetworkVariable<int> index = new();
    void Start()
    {       
        if (!IsServer) { return; }
        myPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerC>();
        StartCoroutine(Grow());
    }


    public IEnumerator Grow()
    {
        while (growthFactor != 5)
        {
            growthFactor += 1 * Time.deltaTime;
            growthFactor = Mathf.Clamp(growthFactor, 1, 5);
            transform.localScale = new Vector3(1, growthFactor, 1);
            yield return null;
        }
        if (growthFactor == 5)
        {
            ripe.Value = true;
        }
    }

    public void Harvest()
    {
        ripe.Value = false;
        growthFactor = 1;
        StartCoroutine(Grow());
    }

    [ServerRpc]
    public void HarvestServerRpc(int index, ulong clientID)
    {
        // call harvest on server's version of the farm
        NetworkManager.ConnectedClients[clientID].PlayerObject.GetComponent<PlayerC>().farms[index].GetComponent<Farm>().Harvest();
    }
}
