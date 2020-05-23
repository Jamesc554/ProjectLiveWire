using System;
using System.Collections.Generic;
using Lobby;
using Map;
using Map.Buildings;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    private NetworkManagerVirus _netManager;
    [SerializeField] private bool gameStarted;
    [SerializeField] private float timeTillProduction = 1.5f;
    [SerializeField] private float productionTime = 1.5f;
    
    [SerializeField] private TMP_Text[] scoreTexts = new TMP_Text[2];
    [SerializeField] private Slider scoreSlider;

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

    public void Update()
    {
        if (isServer)
        {
            timeTillProduction -= Time.deltaTime;
            if (timeTillProduction <= 0)
            {
                foreach (var node in NetManager.Nodes)
                {
                    if (node.GetNode() is BaseNode baseNode)
                    {
                        baseNode.DoAction(1);
                    }
                }

                timeTillProduction = productionTime;
            }
        }
    }

    public bool HasGameStarted()
    {
        return gameStarted;
    }

    public override void OnStartServer()
    {
        NetworkManagerVirus.OnServerStopped += CleanUpServer;
        NetworkManagerVirus.OnServerReadied += CheckToStart;
    }

    #region Server

    [ServerCallback]
    private void OnDestroy()
    {
        CleanUpServer();
    }

    [Server]
    private void CleanUpServer()
    {
        gameStarted = false;
        NetworkManagerVirus.OnServerStopped -= CleanUpServer;
        NetworkManagerVirus.OnServerReadied -= CheckToStart;
    }

    [ServerCallback]
    public void StartGame()
    {
        // Give Ownership and Energy to starting nodes - 1 For each player
        if (NetManager.StartingNodes.Count >= NetManager.GamePlayers.Count)
        {
            for (int i = 0; i < NetManager.GamePlayers.Count; i++)
            {
                NodeController nc = NetManager.StartingNodes[i];
                nc.SetOwner(NetManager.GamePlayers[i].netIdentity, NetManager.GamePlayers[i].GetColor());
                nc.SetEnergy(50);
            }
        }

        gameStarted = true;
        NetManager.BeginGame();
        
        // Tell clients the game has started, visuals?
        RpcStartGame();
    }
    
    [Server]
    private void CheckToStart(NetworkConnection client)
    {
        foreach (var player in NetManager.GamePlayers)
        {
            if (!player.connectionToClient.isReady)
                return;
            
            StartGame();
        }
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcStartGame()
    {
        Debug.Log("Game Started");
    }
    
    public override void OnStartClient()
    {
        NetworkIdentity ni = NetworkClient.connection.identity;
        NetworkGamePlayerVirus player = ni.GetComponent<NetworkGamePlayerVirus>();
        
        player.CmdGetNodeControllers();
        //UpdateScoreDisplay();
        
        //NodeController.OnEnergyChanged += UpdateScoreDisplay;
    }

    // private void UpdateScoreDisplay()
    // {
    //     NetworkIdentity ni = NetworkClient.connection.identity;
    //     NetworkGamePlayerVirus player = ni.GetComponent<NetworkGamePlayerVirus>();
    //     
    //     player.CmdGetNodeControllers();
    //     
    //     Dictionary<string, int> scores = new Dictionary<string, int>();
    //     foreach (var nodeController in player.GetAllNodeControllers())
    //     {
    //         if (nodeController.GetOwner().Equals(string.Empty))
    //             continue;
    //
    //         if (scores.ContainsKey(nodeController.GetOwner()))
    //             scores[nodeController.GetOwner()] += nodeController.GetEnergy();
    //         else 
    //             scores.Add(nodeController.GetOwner(), nodeController.GetEnergy());
    //     }
    //     
    //     int[] sliderScores = new int[2];
    //     int i = 0;
    //     foreach (var score in scores)
    //     {
    //         scoreTexts[i].text = score.Value.ToString();
    //         sliderScores[i] = score.Value;
    //         i++;
    //     }
    //
    //     scoreSlider.value = sliderScores[0] / (float)sliderScores[1];
    // }

    #endregion
}