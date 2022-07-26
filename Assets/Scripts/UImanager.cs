using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using UnityEngine.UI;

public class UImanager : NetworkBehaviour
{
    public GameObject moneyDisplay;
    public GameObject timerDisplay;
    public GameObject goldDisplay;
    float timeLeft = 5;

    public GameObject ttButton;

    public Button wall;
    public Button tower;
    public Button squig;

    public GameObject techWrapper;

    bool TTup = false;

    public bool gameStarted = false;
    GameObject player;
    DBmanager db;

    private void Start()
    {
        if (!NetworkManager.Singleton.IsClient) { return; }
        StartCoroutine(WaitForPlayer());
    }

    private void Update()
    {
        if (PlayerC.clientConnected)
        {
            moneyDisplay.GetComponent<TMP_Text>().text = "Money: " + PlayerPrefs.GetInt("money");
            goldDisplay.GetComponent<TMP_Text>().text = "Gold: " + NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<PlayerC>().gold;

            GameObject[] squigs = GameObject.FindGameObjectsWithTag("Squig");
            foreach (GameObject squig in squigs) // not very efficient
            {
                squig.transform.Find("EnemyCanvas").Find("HealthSlider").GetComponent<Slider>().value = (float)squig.GetComponent<SquigBehaviour>().squigHP.Value / 100;
            }

            if (Input.GetKeyDown(KeyCode.Escape) && TTup == true)
            {
                TTup = false;
                ToggleTT();
            }

            if (!gameStarted)
            {
                timeLeft -= Time.deltaTime;
                timeLeft = Mathf.Clamp(timeLeft, 0, 60);
                TimeSpan time = TimeSpan.FromSeconds(timeLeft);
                timerDisplay.GetComponent<TMP_Text>().text = "Until Next Wave: " + time.Seconds.ToString();

                if (timeLeft == 0)
                {
                    gameStarted = true; // just shut off timer
                }
            }
        }
    }

    public void ToggleTT()
    {
        TTup = true;
        techWrapper.SetActive(!techWrapper.activeSelf);
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

    public void Upgrade1() // unlock struct index 1 aka wall
    {
        if (player.GetComponent<PlayerC>().gold >= 200 && player.GetComponent<PlayerC>().wallUpgrade == false)
        {
            player.GetComponent<PlayerC>().wallUpgrade = true;
            player.GetComponent<PlayerC>().gold -= 200;
            BuySomethingServerRpc("Wall", PlayerPrefs.GetString("guid"));

            Unlock(1);
        }
    }

    public void Upgrade2()
    {
        if (player.GetComponent<PlayerC>().gold >= 200 && player.GetComponent<PlayerC>().towerUpgrade == false)
        {
            player.GetComponent<PlayerC>().towerUpgrade = true;
            player.GetComponent<PlayerC>().gold -= 200;
            BuySomethingServerRpc("Tower", PlayerPrefs.GetString("guid"));

            Unlock(2);
        }
    }

    public void Upgrade3()
    {
        if (player.GetComponent<PlayerC>().gold >= 200 && player.GetComponent<PlayerC>().squigUpgrade == false)
        {
            player.GetComponent<PlayerC>().squigUpgrade = true;
            player.GetComponent<PlayerC>().gold -= 200;
            BuySomethingServerRpc("Squig", PlayerPrefs.GetString("guid"));

            Unlock(3);
        }
    }

    public void Unlock(int upgrade)
    {
        switch (upgrade)
        {
            case 1:
                wall.interactable = true;
                break;
            case 2:
                tower.interactable = true;
                break;
            case 3:
                squig.interactable = true;
                break;
        }
    }

    public void WallButton()
    {
        player.GetComponent<PlayerC>().structIndex = 1;
        LocalTile.cost = 25;
    }

    public void TowerButton()
    {
        player.GetComponent<PlayerC>().structIndex = 2;
        LocalTile.cost = 50;
    }

    public void SquigButton()
    {
        player.GetComponent<PlayerC>().structIndex = 3;
        LocalTile.cost = 100;
    }
    [ServerRpc(RequireOwnership = false)]
    public void BuySomethingServerRpc(string item, string guid)
    {
        int player_id = db.Pullid("SELECT id FROM Players WHERE guid = '" + guid + "';");
        int upgrade_id = db.Pullid("SELECT id FROM Upgrades WHERE upgrade = '" + item + "';");

        db.RunSql(string.Format("INSERT INTO PandU (player_id, upgrade_id) VALUES ({0},{1});", player_id, upgrade_id));

    }
}
