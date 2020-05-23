using System;
using System.Collections.Generic;
using System.Linq;
using Map;
using Map.Buildings;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Lobby
{
    [RequireComponent(typeof(NetworkManagerVirus))]
    public class NetworkGamePlayerVirus : NetworkBehaviour
    {
        [SyncVar]
        private string displayName = "Loading...";

        [SyncVar]
        private Color playerColor;

        private List<NetworkIdentity> _nodeControllerIdentities;

        private NetworkManagerVirus room;

        private NetworkManagerVirus NetManager
        {
            get
            {
                if (room != null)
                {
                    return room;
                }

                return room = NetworkManager.singleton as NetworkManagerVirus;
            }
        }

        [SerializeField] private List<NodeController> selectedNodes = new List<NodeController>();

        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);
            NetManager.GamePlayers.Add(this);
            CmdGetNodeControllers();
        }

        public override void OnStartServer()
        {
            playerColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }

        public override void OnStopClient()
        {
            NetManager.GamePlayers.Remove(this);
        }

        [Server]
        public void SetDisplayName(string displayName)
        {
            this.displayName = displayName;
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public Color GetColor()
        {
            return playerColor;
        }

        public void AddSelectedNode(NetworkIdentity networkId)
        {
            selectedNodes.Add(networkId.GetComponent<NodeController>());
        }

        public List<NodeController> GetSelectedNodes()
        {
            return selectedNodes;
        }

        private void Update()
        {
            if (hasAuthority)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    foreach (var node in selectedNodes)
                    {
                        CmdBuildNode(node.netIdentity, 1);
                    }
                }
                
                if (Input.GetKeyDown(KeyCode.W))
                {
                    foreach (var node in selectedNodes)
                    {
                        CmdBuildNode(node.netIdentity, 2);
                    }
                }
                
                if (Input.GetKeyDown(KeyCode.E))
                {
                    foreach (var node in selectedNodes)
                    {
                        CmdBuildNode(node.netIdentity, 3);
                    }
                }

                if (Input.GetKeyDown(KeyCode.B))
                {
                    CmdBalanceSelected();
                }
            }
        }

        [Command]
        public void CmdBuildNode(NetworkIdentity networkIdentity, int componentId)
        {
            NodeController nodeController = networkIdentity.GetComponent<NodeController>();
            if (nodeController.GetNode().GetController().GetComponentId() != 0 || nodeController.GetNode().GetNodeType() != Components.ComponentData[componentId].FoundationRequirement)
                return;
                        
            if (nodeController.GetEnergy() > Components.ComponentData[componentId].BuildCost)
            {
                int newEnergyLevel = nodeController.GetEnergy() - Components.ComponentData[componentId].BuildCost;
                ((BaseNode)nodeController.GetNode()).SetComponentData(componentId);
                nodeController.SetEnergy(newEnergyLevel);
                // nodeController.EnableHChevrons();
                // nodeController.EnableVChevrons();
            }
        }

        [Command]
        public void CmdBalanceSelected()
        {
            int total = 0, average = 0;
            bool balanced = false;
            List<NodeSendJob> sendJobs = new List<NodeSendJob>();
            Dictionary<NodeController, int> predictedAmounts = new Dictionary<NodeController, int>();
            
            foreach (var node in selectedNodes)
            {
                total += node.GetEnergy();
                predictedAmounts.Add(node, node.GetEnergy());
            }

            average = Mathf.FloorToInt(total / selectedNodes.Count);
            
            int checksWithoutChange = 0;
            while (!balanced || checksWithoutChange <= 5)
            {
                foreach (var node in selectedNodes)
                {
                    balanced = true;
                    checksWithoutChange++;
                    
                    if (predictedAmounts[node] > average)
                    {
                        foreach (var nodeB in selectedNodes)
                        {
                            if (predictedAmounts[node] <= average)
                                break;
                            
                            if (predictedAmounts[nodeB] < average)
                            {
                                int deltaEnergy = average - predictedAmounts[nodeB];
                                int overageEnergy = predictedAmounts[node] - average;
                                
                                sendJobs.Add(new NodeSendJob(node, nodeB, overageEnergy > deltaEnergy ? deltaEnergy : overageEnergy));
                                predictedAmounts[node] -= overageEnergy > deltaEnergy ? deltaEnergy : overageEnergy;
                                predictedAmounts[nodeB] += overageEnergy > deltaEnergy ? deltaEnergy : overageEnergy;
                                checksWithoutChange = 0;
                            }
                        }
                    }
                    else if (predictedAmounts[node] < average)
                        balanced = false;
                }
            }

            // foreach (var node in selectedNodes)
            // {
            //     if (predictedAmounts[node] >= average + (average + predictedAmounts.Count) / average)
            //     {
            //         int i = 0;
            //         while (predictedAmounts[node] >= average + (average + predictedAmounts.Count) / average)
            //         {
            //             NodeController nodeB = selectedNodes[i];
            //             if (node == nodeB)
            //                 continue;
            //             
            //             if (predictedAmounts[node] <= average + (average + predictedAmounts.Count) / average)
            //                 break;
            //
            //             if (predictedAmounts[nodeB] <= average + (average + predictedAmounts.Count) / average)
            //                 continue;
            //
            //             sendJobs.Add(new NodeSendJob(node, nodeB, 1));
            //             predictedAmounts[node] -= 1;
            //             predictedAmounts[nodeB] += 1;
            //
            //             i++;
            //         }
            //     }
            // }

            foreach (var job in sendJobs)
            {
                job.source.SendEnergy(job.amount, job.destination);
            }
        }

        [Command]
        public void CmdSelectNode(NetworkIdentity networkId)
        {
            if (NetManager.HasGameStarted())
            {
                NodeController controller = networkId.GetComponent<NodeController>();
                if (controller.GetNode() != null)
                {
                    if (NodeReferences.InteractableNodeTypes.Contains(controller.GetNode().GetNodeType()))
                    {
                        if (controller.GetOwner() == netIdentity)
                        {
                            selectedNodes.Add(controller);
                            TargetSelectNode(networkId);
                        }
                    }
                }
            }
        }

        [TargetRpc]
        public void TargetSelectNode(NetworkIdentity networkId)
        {
            selectedNodes.Add(networkId.GetComponent<NodeController>());
            networkId.GetComponent<SpriteRenderer>().color = Color.green;
        }
        
        [TargetRpc]
        public void TargetUnselectNodes()
        {
            UnselectNodes();
        }
        
        [Command]
        public void CmdUnselectNodes()
        {
            UnselectNodes();
            TargetUnselectNodes();
        }

        private void UnselectNodes()
        {
            foreach (var node in selectedNodes)
            {
                node.GetComponent<SpriteRenderer>().color = playerColor;
            }
            
            selectedNodes.Clear();
        }

        [Command]
        public void CmdMoveEnergy(NetworkIdentity destination, int amount, bool deselect)
        {
            if (NetManager.HasGameStarted())
            {
                NodeController dest = destination.GetComponent<NodeController>();
            
                if (dest.GetNode() != null && selectedNodes.Count > 0)
                {
                    if (NodeReferences.InteractableNodeTypes.Contains(dest.GetNode().GetNodeType()))
                    {
                        foreach (var source in selectedNodes)
                        {
                            source.SendEnergy(amount, dest);
                        }
                    }

                    if (deselect)
                    {
                        UnselectNodes();
                        TargetUnselectNodes();
                    }
                }
            }
        }

        #region Server

        [Command]
        public void CmdGetNodeControllers()
        {
            List<NetworkIdentity> ncIdentities = new List<NetworkIdentity>();
            foreach (var nodeController in NetManager.Nodes)
            {
                ncIdentities.Add(nodeController.netIdentity);
            }
            TargetUpdateNodeControllers(ncIdentities.ToArray());
        }

        #endregion

        #region Client

        [TargetRpc]
        public void TargetUpdateNodeControllers(NetworkIdentity[] networkIdentities)
        {
            _nodeControllerIdentities = networkIdentities.ToList();
        }

        public List<NodeController> GetAllNodeControllers()
        {
            List<NodeController> nodeControllers = new List<NodeController>();
            foreach (var netId in _nodeControllerIdentities)
            {
               nodeControllers.Add(netId.GetComponent<NodeController>()); 
            }

            return nodeControllers;
        }

        #endregion
    }

    class NodeSendJob
    {
        public NodeController source, destination;
        public int amount;

        public NodeSendJob(NodeController source, NodeController destination, int amount)
        {
            this.source = source;
            this.destination = destination;
            this.amount = amount;
        }
    }
}
