using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class FarmCollider : NetworkBehaviour
{
    Farm myParent;
    PlayerC myPlayer;
    void Start()
    {
        if (!IsOwner) { return; }     
        myPlayer = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerC>();
        myParent = transform.parent.GetComponent<Farm>();
    }

    private void OnMouseOver()
    {
        if (!IsOwner) { return; }
        if (Input.GetMouseButtonDown(0) && myParent.ripe.Value)
        {
            myPlayer.gold += 25;
            myParent.HarvestServerRpc(myParent.index.Value, NetworkManager.LocalClientId);
        }
    }
}
