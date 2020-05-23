using System;
using System.Collections;
using System.Collections.Generic;
using Lobby;
using Map;
using Map.Buildings;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class GridManager : NetworkBehaviour
{
    private Node[,] _grid;

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private GameObject[] horizontalProductionPrefabs = new GameObject[4];
    [SerializeField] private GameObject[] verticalProductionPrefabs = new GameObject[4];
    [SerializeField] private List<NodeTypePair> NodeTypePairs;
    [SerializeField] private Dictionary<NodeType, GameObject> nodePrefabs = new Dictionary<NodeType, GameObject>();

    private NetworkManagerVirus _netManager;

    private NetworkManagerVirus NetManager
    {
        get
        {
            if (_netManager != null)
            {
                return _netManager;
            }

            return _netManager = NetworkManager.singleton as NetworkManagerVirus;
        }
    }
    
    private void Awake()
    {
        NetManager.SetGridManager(this);
        foreach (var nodeTypePair in NodeTypePairs)
        {
            nodePrefabs.Add(nodeTypePair.NodeType, nodeTypePair.Prefab);
        }
        ParseMap(tilemap);
        GenerateMap();
    }

    public void AddNodePrefab(NodeType nodeType, GameObject nodePrefab)
    {
        nodePrefabs.Add(nodeType, nodePrefab);
    }

    public void ParseMap(Tilemap tilemap)
    {
        _grid = TilemapParser.ParseMap(tilemap);
    }

    public Node GetNodeAt(int x, int y)
    {
        return _grid[x, y];
    }

    public GameObject GetHorizontalProductionPrefab(int level)
    {
        return horizontalProductionPrefabs[level];
    }
    
    public GameObject GetVerticalProductionPrefab(int level)
    {
        return verticalProductionPrefabs[level];
    }

    [Server]
    public void GenerateMap()
    {
        for (int y = 0; y < _grid.GetLength(1); y++)
        {
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                Node node = _grid[x, y];
                if (node != null)
                {
                    if (node.GetNodeType() != NodeType.BLANK)
                    {
                        GameObject nodeGO = Instantiate(nodePrefabs[node.GetNodeType()], new Vector3(x - 5.5f, y - 9.5f, 0), nodePrefabs[node.GetNodeType()].transform.rotation);
                        NodeController nc = nodeGO.GetComponent<NodeController>();
                        nc.SetNode(node);

                        if (node.GetNodeType() == NodeType.START_NODE)
                        {
                            NetManager.StartingNodes.Add(nc);
                            node.SetNodeType(NodeType.NODE);
                        }

                        if (node is BaseNode baseNode)
                        {
                            baseNode.SetComponentData(0);
                        }

                        NetworkServer.Spawn(nodeGO);
                        nc.color = node.GetNodeType() != NodeType.DATA_BUS ? Color.white : Color.red;
                    }
                }
            }
        }
    }
}
