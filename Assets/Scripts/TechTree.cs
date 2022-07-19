using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TechTree : NetworkBehaviour
{
    GameObject player;
    private void Start()
    {
        if (!NetworkManager.Singleton.IsClient) { return; }
        StartCoroutine(WaitForPlayer());
    }
    void OnGUI()
    {
        if (player == null) { return; }
        GUILayout.BeginArea(new Rect(10, 100, 100, 300));

        if (GUILayout.Button("Tech Tree"))
        {
            Debug.Log("keypress");
        }
        if (GUILayout.Button("Fire Tower"))
        {
            Debug.Log("fire tower");
            player.GetComponent<PlayerC>().structIndex = 1;
            LocalTile.cost = 75;
        }
        if (GUILayout.Button("Ball Tower"))
        {
            player.GetComponent<PlayerC>().structIndex = 0;
            LocalTile.cost = 50;
        }

        GUILayout.EndArea();
    }

    IEnumerator WaitForPlayer()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton.LocalClient.PlayerObject != null);
        player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
    }
}
