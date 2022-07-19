using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SquigBehaviour : NetworkBehaviour
{
    public NetworkVariable<int> squigHP = new NetworkVariable<int>();

    bool moving = false;
    public bool followingPath = false;
    public List<Node> currentPath = new List<Node>();
    public int onStep = 0;
    bool blocked = false;
    public bool obstruction = false;

    private void Start()
    {
        if (!IsServer) { return; }
        squigHP.Value = 100;
    }
    
    void Update()
    {
        if (!IsServer) { return; }

        if (transform.position != Vector3.zero && !blocked)
        {
            if (!followingPath)
            {

                currentPath = AStarPath(transform.position);
                followingPath = true;
                onStep = 0;
                if (currentPath == null) // no path
                {
                    blocked = true;
                    return;
                }
            }

            if (moving == false)
            {
                StartCoroutine(Move(currentPath[onStep], currentPath[onStep + 1]));
                onStep++;
            }
        }
        else
        {
            DeclareVacantClientRpc(currentPath[currentPath.Count-2].myTileIndex); // cleanup

            GameObject.Find("ServerBoss(Clone)").GetComponent<ServerBoss>().hqHealth.Value -= 10;
            Debug.Log("new hp is: " + GameObject.Find("ServerBoss(Clone)").GetComponent<ServerBoss>().hqHealth.Value);
            Destroy(gameObject); // not good
        }


    }
    IEnumerator Move(Node a, Node b)
    {
        moving = true;

        DeclareOccupiedClientRpc(b.myTileIndex);

        float intrpl = 0;
        while (intrpl != 1)
        {
            intrpl += 30f * Time.deltaTime / (Mathf.Pow(a.pos.x - b.pos.x, 2) + Mathf.Pow(a.pos.z - b.pos.z, 2));
            intrpl = Mathf.Clamp(intrpl, 0, 1);
            transform.position = Vector3.Lerp(a.pos, b.pos, intrpl);
            yield return null;
        }

        DeclareVacantClientRpc(a.myTileIndex);
        
        if (obstruction)
        {
            currentPath = AStarPath(transform.position);
            onStep = 0;
            obstruction = false;
        }
        moving = false;
    }   

    public void TakeDamage(int magnitude, ulong clientID)
    {
        squigHP.Value -= magnitude;
        if (squigHP.Value <= 0)
        {
            Destroy(gameObject);
            Debug.Log("squig killed by " + clientID);
            
            foreach (KeyValuePair <ulong, Unity.Netcode.NetworkClient> client in NetworkManager.Singleton.ConnectedClients)
            {
                if (clientID == client.Key)
                {
                    client.Value.PlayerObject.gameObject.GetComponent<PlayerC>().gold += 50;
                }
            }
        }
    }

    private List<Node> AStarPath(Vector3 start)
    {
        bool complete = false;
        List<Node> openNodes = new List<Node>();
        List<Node> closedNodes = new List<Node>();
        Node targetNode = new Node();
        Node startNode = new Node();

        foreach (Node node in EnemyManager.all_Nodes)
        {
            if (node.pos == start)
            {
                startNode = node;
                openNodes.Add(startNode);
            }
            if (node.pos == Vector3.zero)
            {
                targetNode = node;
            }
        }

        while (!complete)
        {
            if (openNodes.Count == 0)
            {
                Debug.Log("no exit");
                break;
            }
            Node currentNode = FindCheapestNode(openNodes);
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            if (currentNode == targetNode)
            {
                complete = true;
                List<Node> path = new();
                List<Node> tempPath = new();

                while (currentNode.parent != null)
                {
                    tempPath.Add(currentNode);
                    currentNode = currentNode.parent;
                }

                path.Add(startNode);                                        // manually add start position

                for (int i = tempPath.Count-1; i >= 0; i--)
                {
                    path.Add(tempPath[i]);
                }
                return path;
            }
            else
            {
                foreach (Node n in EnemyManager.all_Nodes)
                {
                    if (50 >= Mathf.Pow(currentNode.pos.x - n.pos.x, 2) + Mathf.Pow(currentNode.pos.z - n.pos.z, 2))
                    {
                        if (!closedNodes.Contains(n))
                        {
                            float distToStart = (Mathf.Abs(n.pos.x - startNode.pos.x) / 5 + Mathf.Abs(n.pos.z - startNode.pos.z) / 5) * 25;

                            if (distToStart < n.g_cost || !openNodes.Contains(n))
                            {
                                n.parent = currentNode;
                                float hCost = (Mathf.Abs(n.pos.x - targetNode.pos.x) / 5 + Mathf.Abs(n.pos.z - targetNode.pos.z) / 5) * 25;
                                n.cost =  distToStart + hCost;

                                if (!openNodes.Contains(n))
                                {
                                    openNodes.Add(n);
                                }
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("backdoor");
        return null; // otherwise error
    }

    private Node FindCheapestNode(List<Node> nodes)
    {
        float minCost = 9999;
        int cheapestIndex = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].cost < minCost)
            {
                minCost = nodes[i].cost;
                cheapestIndex = i;
            }
        }
        return nodes[cheapestIndex];
    }

    [ClientRpc]
    private void DeclareOccupiedClientRpc(int index)  // private bc of same rpc on PlayerC
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerC>().tiles[index].GetComponent<LocalTile>().occupied = true;
    }

    [ClientRpc]
    private void DeclareVacantClientRpc(int index)
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerC>().tiles[index].GetComponent<LocalTile>().occupied = false;
    }
}
public class Node
{
    public Vector3 pos;
    public float cost;
    public Node parent;
    public float g_cost; // g = cost from start
    public float h_cost; // h = heuristic cost to target

    public int myTileIndex;
}
