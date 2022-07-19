using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyHealth : MonoBehaviour
{
    Vector3 camPos;

    private void Start()
    {
        if (NetworkManager.Singleton.LocalClient == null || NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            camPos = new Vector3(0, 20, 0);
        }
        else
        {
            camPos = NetworkManager.Singleton.LocalClient.PlayerObject.transform.position;
        }
    }
    void Update()
    {
        transform.LookAt(camPos);
    }
}
