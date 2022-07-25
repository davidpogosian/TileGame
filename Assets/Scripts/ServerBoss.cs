using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ServerBoss : NetworkBehaviour
{
    public GameObject headQuarters;
    public GameObject tile;
    public GameObject dbmanager;
    public GameObject squig;
    public GameObject techTree;
    public GameObject enemyManager;
    public GameObject UI;

    public GameObject player;

    public NetworkVariable<int> hqHealth = new NetworkVariable<int>();
    public static List<Vector3> serverPivots = new List<Vector3>();

    private void Start()
    {        
        if (!NetworkManager.Singleton.IsServer) //race?
        {
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

            if (NetworkManager.LocalClientId % 2 == 0)
            {
                GameObject hq = Instantiate(headQuarters, new Vector3(-25f, 5f, -25f), Quaternion.identity);
                hq.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[1]); // hq for 2nd player
            }
            else
            {
                GameObject hq = Instantiate(headQuarters, new Vector3(25f, 5f, 25f), Quaternion.identity);
                hq.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[0]); // hq for first player
            }
            
            return;
        }
        
        hqHealth.Value = 100;
        

        //GameObject hq2 = Instantiate(headQuarters, new Vector3(25f, 5f, 25f), Quaternion.identity);
        //hq2.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[1]); // hq for 2nd player

        GameObject db = Instantiate(dbmanager, Vector3.zero, Quaternion.identity);
        db.GetComponent<NetworkObject>().Spawn();
        
        //GameObject tt = Instantiate(techTree, Vector3.zero, Quaternion.identity);
        //tt.GetComponent<NetworkObject>().Spawn();

        GameObject ui = Instantiate(UI, Vector3.zero, Quaternion.identity);
        ui.GetComponent<NetworkObject>().Spawn();

        for (int i = -5; i <= 5; i++)
        {
            for (int j = -5; j <= 5; j++)
            {
                serverPivots.Add(new Vector3(5 * i, 0, 5 * j)); //this needs to be elsewhere
            }
        }

        GameObject em = Instantiate(enemyManager, Vector3.zero, Quaternion.identity);
        em.GetComponent<EnemyManager>().MakeNodeList(serverPivots);

        StartCoroutine(Recess());

        if (NetworkManager.Singleton.IsClient)
        {
            GameObject pl = Instantiate(player, new Vector3(0, 20, 0), Quaternion.identity);
            pl.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);

            GameObject hq = Instantiate(headQuarters, new Vector3(-25f, 5f, -25f), Quaternion.identity);
            hq.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[0]); // hq for first player
        }        
    }
    IEnumerator Recess()
    {
        yield return new WaitForSeconds(5);

        //GameObject go = Instantiate(squig, new Vector3(25,0,25), Quaternion.identity);
        //go.GetComponent<NetworkObject>().Spawn();

        //for (float j = 0; j < 2.5; j = j + 0.5f) // 5 rounds
        //{
        //    for (float i = j; i < j + 0.5; i = i + 0.1f)
        //    {
        //        GameObject go = Instantiate(squig, 50 * new Vector3(Mathf.Cos(i * Mathf.PI), 0, Mathf.Sin(i * Mathf.PI)), Quaternion.identity);
        //        go.GetComponent<NetworkObject>().Spawn();
        //    }
        //    yield return new WaitForSeconds(5);
        //}
    }
    public void DeleteNode(int tileID)
    {
        GameObject.Find("EnemyManager(Clone)").GetComponent<EnemyManager>().OccupyNode(tileID);
    }

    public void TakeDamage(int mag)
    {
        hqHealth.Value -= mag;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong clientID)
    {
        GameObject pl = Instantiate(player, new Vector3(0, 20, 0), Quaternion.identity);
        pl.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
    }
}
