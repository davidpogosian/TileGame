using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEditor;
using UnityEngine.EventSystems;
public class LocalTile : MonoBehaviour 
{
    Renderer rend;
    GameObject player;

    public int myIndex;
    public bool occupied = false;
    public static int cost = 50;

    private void Start()
    {        
        player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        rend = GetComponent<Renderer>();
        myIndex = player.GetComponent<PlayerC>().tiles.IndexOf(gameObject);

        //if (myIndex == 60) // occupy center
        //{
        //    occupied = true;
        //}
        //if (myIndex == 120) // occupy spawn
        //{
        //    occupied = true;
        //}
        List<Vector3> occupiedByHq = new List<Vector3>();
        foreach (GameObject hq in GameObject.FindGameObjectsWithTag("HeadQuarters"))
        {
            occupiedByHq.Add(hq.transform.position);
        }

        foreach (Vector3 pos in occupiedByHq)
        {
            if (pos.x == transform.position.x && pos.z == transform.position.z)
            {
                occupied = true;
            }
        }
    }
    private void OnMouseEnter()
    {
        rend.material.color = Color.white;
    }
    private void OnMouseExit()
    {
        rend.material.color = Color.black;
    }
    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; } // ui now blocks clicks

        if (player.GetComponent<PlayerC>().gold >= cost && occupied == false)
        {
            player.GetComponent<PlayerC>().gold -= cost;
            player.GetComponent<PlayerC>().ClickedServerRpc(player.GetComponent<PlayerC>().structIndex, transform.position, NetworkManager.Singleton.LocalClientId, myIndex); // build
        }
    }

    
}
