using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
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

    public void ToggleTT()
    {
        TTup = true;
        ttBackground.SetActive(!ttBackground.activeSelf);
        ttUpgrade1.SetActive(!ttUpgrade1.activeSelf);
        ttUpgrade2.SetActive(!ttUpgrade2.activeSelf);
        ttUpgrade3.SetActive(!ttUpgrade3.activeSelf);
        ttButton.SetActive(!ttButton.activeSelf);
    }
}
