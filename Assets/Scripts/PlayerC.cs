using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerC : NetworkBehaviour
{
    public static bool clientConnected = false;
    float hor;
    float vert;
    float scrollmulti = -5;
    public int gold = 0;
    public int structIndex = 0;

    DBmanager db;
    public GameObject tile;
    public GameObject building;
    public GameObject pom;
    public GameObject wall;
    public GameObject squig;

    public bool wallUpgrade = false;
    public bool towerUpgrade = false;
    public bool squigUpgrade = false;

    public List<GameObject> tiles = new List<GameObject>();
    Vector3 myPosition = new();
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        
        gold = 1000;
    }

    private void Start()
    {
        if (!IsOwner) { return; }
        SetUpPlayer();

        for (int i = -5; i <= 5; i++)
        {
            for (int j = -5; j <= 5; j++)
            {
                tiles.Add(Instantiate(tile, new Vector3(5 * i, 0, 5 * j), Quaternion.identity));
            }
        }

        myPosition = transform.position;
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        hor = Input.GetAxis("Horizontal");
        vert = Input.GetAxis("Vertical");

        myPosition += 10 * Time.deltaTime * new Vector3(hor, Input.mouseScrollDelta.y * scrollmulti, vert);

        myPosition.x = Mathf.Clamp(myPosition.x, -52.5f, 52.5f);
        myPosition.y = Mathf.Clamp(myPosition.y, 10, 30);
        myPosition.z = Mathf.Clamp(myPosition.z, -60f, 40f);

        transform.position = myPosition;        
    }
    private void SetUpPlayer()
    {
        //yield return new WaitUntil(() => NetworkManager.Singleton.LocalClient != null); // is this necessary
        clientConnected = true;
        Camera.main.transform.parent = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.transform;
        Camera.main.transform.localPosition = Vector3.zero;
        Camera.main.transform.rotation = new Quaternion(0.42f, 0, 0, 0.91f);

        var dbObj = GameObject.Find("DBmanager(Clone)");
        if (dbObj == null)
        {
            Debug.LogError("DBManager is not instancieted");
            return;
        }
        db = dbObj.GetComponent<DBmanager>();

        db.AddPlayerServerRpc(PlayerPrefs.GetString("guid"));               // add guid to Players table

        db.PullPlayerUpgradesServerRpc(PlayerPrefs.GetString("guid"), NetworkManager.LocalClient.ClientId);


        /*db.CreateTServerRpc();*/ // make sure table is on server
                                   //db.PullEntryServerRpc(PlayerPrefs.GetString("guid"));
                                   //db.DisplayEntriesServerRpc();

        //db.GetComponent<DBmanager>().EditEntryServerRpc(PlayerPrefs.GetString("guid"), 0);

    }


    [ServerRpc]
    public void ClickedServerRpc(int structIndex, Vector3 pos, ulong clientID, int tileIndex)
    {
        //GameObject go = Instantiate(building, pos, Quaternion.identity);
        //go.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);

        //GameObject bullet = Instantiate(pom, pos + new Vector3(0, 6, 0), Quaternion.identity, go.transform);
        //bullet.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);

        //GameObject go = Instantiate(building, pos, Quaternion.identity); // works but not parented
        //go.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);

        //GameObject bullet = Instantiate(pom, pos + new Vector3(0, 6, 0), Quaternion.identity);
        //bullet.transform.parent = go.transform;
        //bullet.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
        if (structIndex == 0) { return; }

        if (structIndex != 3)
        {
            GameObject.Find("ServerBoss(Clone)").GetComponent<ServerBoss>().DeleteNode(tileIndex);
            DeclareOccupiedClientRpc(tileIndex);
        }
        
        


        switch (structIndex)
        {
            case 1:
                GameObject ft = Instantiate(wall, pos, Quaternion.identity);
                ft.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
                break;
            case 2:
                GameObject go = Instantiate(building, pos, Quaternion.identity);
                GameObject bullet = Instantiate(pom, pos + new Vector3(0, 6, 0), Quaternion.identity);
                go.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
                bullet.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
                bullet.transform.parent = go.transform;
                break;
            case 3:
                GameObject sq = Instantiate(squig, pos, Quaternion.identity);
                sq.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
                break;
        }
    }

    [ServerRpc]
    public void DeclareOccupiedServerRpc(int index)
    {
        DeclareOccupiedClientRpc(index);
    }
    [ClientRpc]
    public void DeclareOccupiedClientRpc(int index)
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerC>().tiles[index].GetComponent<LocalTile>().occupied = true;
    }
    private void OnApplicationQuit()
    {
        if (!IsOwner) { return; }
        //db.EditEntryServerRpc(PlayerPrefs.GetString("guid"), PlayerPrefs.GetInt("money")); // save money        
    }
}
