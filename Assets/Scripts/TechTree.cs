using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TechTree : NetworkBehaviour
{
    GameObject player;
    DBmanager db;
    public bool turnOff = true;
    private void Start()
    {
        if (!NetworkManager.Singleton.IsClient) { return; }
        StartCoroutine(WaitForPlayer());
    }
    void OnGUI()
    {
        if (player == null) { return; }
        if (turnOff == true) { return; }
        GUILayout.BeginArea(new Rect(10, 100, 100, 300));

        if (GUILayout.Button("Upgrade1"))
        {
            if (player.GetComponent<PlayerC>().gold >= 200 && player.GetComponent<PlayerC>().wallUpgrade == false)
            {
                player.GetComponent<PlayerC>().wallUpgrade = true;
                player.GetComponent<PlayerC>().gold -= 200;
                BuySomethingServerRpc("Vitality", PlayerPrefs.GetString("guid"));
            }
        }
        if (GUILayout.Button("Upgrade2"))
        {
            if (player.GetComponent<PlayerC>().gold >= 200 && player.GetComponent<PlayerC>().towerUpgrade == false)
            {
                player.GetComponent<PlayerC>().towerUpgrade = true;
                player.GetComponent<PlayerC>().gold -= 200;
                BuySomethingServerRpc("Strength", PlayerPrefs.GetString("guid"));
            }
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
        yield return new WaitUntil(() => NetworkManager.Singleton.LocalClient.PlayerObject != null); // necessary?
        player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

        var dbObj = GameObject.Find("DBmanager(Clone)");
        if (dbObj == null)
        {
            Debug.LogError("DBManager is not instancieted");
        }
        db = dbObj.GetComponent<DBmanager>();
    }
    [ServerRpc(RequireOwnership = false)]
    public void BuySomethingServerRpc(string item, string guid)
    {
        int player_id = db.Pullid("SELECT id FROM Players WHERE guid = '" + guid + "';");
        int upgrade_id = db.Pullid("SELECT id FROM Upgrades WHERE upgrade = '" + item + "';");

        db.RunSql(string.Format("INSERT INTO PandU (player_id, upgrade_id) VALUES ({0},{1});", player_id, upgrade_id));

    }
}
