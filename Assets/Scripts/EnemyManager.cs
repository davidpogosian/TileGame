using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyManager : MonoBehaviour
{
    public static List<Node> all_Nodes = new List<Node>();
    List<Vector3> positions = new List<Vector3>();

    private void Start()
    {
        positions = ServerBoss.serverPivots;
        for (int i = 0; i < positions.Count; i++)
        {
            Node new_Node = new();
            new_Node.pos = positions[i];
            new_Node.myTileIndex = i;
            all_Nodes.Add(new_Node);
        }
        FindNeighbors(all_Nodes);
    }
    public List<Node> MakeNodeList(List<Vector3> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Node new_Node = new();
            new_Node.pos = positions[i];
            new_Node.myTileIndex = i;
            all_Nodes.Add(new_Node);
        }
        FindNeighbors(all_Nodes);
        return all_Nodes;
    }

    public void FindNeighbors(List<Node> nodes)
    {
        foreach (Node node in nodes)
        {
            node.neighbors = new List<Node>();
            node.neighbors.Clear();
            foreach (Node potentialNeighbor in nodes)
            {
                if (50 >= Mathf.Pow(node.pos.x - potentialNeighbor.pos.x, 2) + Mathf.Pow(node.pos.z - potentialNeighbor.pos.z, 2))
                {
                    node.neighbors.Add(potentialNeighbor);
                }
            }
        }
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
