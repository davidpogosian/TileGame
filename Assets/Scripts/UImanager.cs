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
    public GameObject ttBackground;
    public GameObject ttUpgrade1;
    public GameObject ttUpgrade2;
    public GameObject ttUpgrade3;

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

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(100, 100, 100, 300));

        if (GUILayout.Button("Wall"))
        {
            player.GetComponent<PlayerC>().structIndex = 1;
            LocalTile.cost = 25;
        }
        if (GUILayout.Button("Ball Tower"))
        {
            player.GetComponent<PlayerC>().structIndex = 0;
            LocalTile.cost = 50;
        }
        if (GUILayout.Button("Squig"))
        {
            player.GetComponent<PlayerC>().structIndex = 2;
            LocalTile.cost = 100;
        }

        GUILayout.EndArea();
    }

    public void ToggleTT()
    {
        TTup = true;
        ttBackground.SetActive(!ttBackground.activeSelf);
        ttUpgrade1.SetActive(!ttUpgrade1.activeSelf);
        ttUpgrade2.SetActive(!ttUpgrade2.activeSelf);
        ttUpgrade3.SetActive(!ttUpgrade3.activeSelf);
        ttButton.SetActive(!ttButton.activeSelf);
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

    public void Upgrade1()
    {
        if (player.GetComponent<PlayerC>().gold >= 200 && player.GetComponent<PlayerC>().upgrade1 == false)
        {
            player.GetComponent<PlayerC>().upgrade1 = true;
            player.GetComponent<PlayerC>().gold -= 200;
            BuySomethingServerRpc("Vitality", PlayerPrefs.GetString("guid"));
        }
    }

    public void Upgrade2()
    {
        if (player.GetComponent<PlayerC>().gold >= 200 && player.GetComponent<PlayerC>().upgrade2 == false)
        {
            player.GetComponent<PlayerC>().upgrade2 = true;
            player.GetComponent<PlayerC>().gold -= 200;
            BuySomethingServerRpc("Strength", PlayerPrefs.GetString("guid"));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BuySomethingServerRpc(string item, string guid)
    {
        int player_id = db.Pullid("SELECT id FROM Players WHERE guid = '" + guid + "';");
        int upgrade_id = db.Pullid("SELECT id FROM Upgrades WHERE upgrade = '" + item + "';");

        db.RunSql(string.Format("INSERT INTO PandU (player_id, upgrade_id) VALUES ({0},{1});", player_id, upgrade_id));

    }
}
