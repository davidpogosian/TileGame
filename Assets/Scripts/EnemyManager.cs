using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyManager : MonoBehaviour
{
    public static List<Node> all_Nodes = new List<Node>();
    public List<Node> MakeNodeList(List<Vector3> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Node new_Node = new();
            new_Node.pos = positions[i];
            new_Node.myTileIndex = i;
            all_Nodes.Add(new_Node);
        }
        return all_Nodes;
    }

    public void OccupyNode(int tileID)
    {
        foreach (Node node in all_Nodes)
        {
            if (node.myTileIndex == tileID)
            {
                node.occupied = true;
                foreach (GameObject go in GameObject.FindGameObjectsWithTag("Squig"))
                {
                    if (go.GetComponent<SquigBehaviour>().currentPath.Contains(node))
                    {
                        go.GetComponent<SquigBehaviour>().obstruction = true;
                    }
                }
            }
        }
    }
}
