using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FireTower : NetworkBehaviour
{
    private void Start()
    {
        switch (gameObject.GetComponent<NetworkObject>().OwnerClientId)
        {
            case 0:
                transform.Find("Cylinder").GetComponent<Renderer>().material.color = Color.blue;
                break;
            case 1:
                transform.Find("Cylinder").GetComponent<Renderer>().material.color = Color.red;
                break;
            case 2:
                transform.Find("Cylinder").GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case 3:
                transform.Find("Cylinder").GetComponent<Renderer>().material.color = Color.green;
                break;
        }
    }
}
