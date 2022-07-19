using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyManager : MonoBehaviour
{
    public static List<Node> all_Nodes = new List<Node>();
    public void MakeNodeList(List<Vector3> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Node new_Node = new();
            new_Node.pos = positions[i];
            new_Node.myTileIndex = i;
            all_Nodes.Add(new_Node);
        }
    }

    public void Delete(int tileID)
    {
        Node deletedNode = new();
        foreach (Node node in all_Nodes)
        {
            node.parent = null;
            if (node.myTileIndex == tileID)
            {
                deletedNode = node;                
            }
        }

        all_Nodes.Remove(deletedNode);        

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Squig"))
        {
            if (go.GetComponent<SquigBehaviour>().currentPath.Contains(deletedNode))
            {
                go.GetComponent<SquigBehaviour>().obstruction = true;
            }
        }
    }
}
