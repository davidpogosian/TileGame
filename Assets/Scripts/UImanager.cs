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
}
