using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Map.Buildings
{
    public static class Components
    {
        public static Dictionary<int, ComponentData> ComponentData = new Dictionary<int, ComponentData>()
        {
            // Base Node
            {0, new ComponentData
            {
                EnergyCap = 24,
                BuildCost = 0,
                FoundationRequirement = NodeType.NODE,
                OnCreation = (self) =>
                {
                    self.GetController().EnableHChevrons(false);
                    self.GetController().EnableVChevrons(false);
                },
                OnReceiveEnergy = (amount, sender, self) => {
                    if (sender == self.GetOwner())
                    {
                        self.AddEnergy(amount);
                        self.OnEnergyReceivedCall();
                    }
                    else
                    {
                        if (amount > self.GetEnergy())
                        {
                            self.SetOwner(sender);
                            self.SetEnergy(0);
                            self.AddEnergy(amount);
                        }
                        else
                        {
                            self.TakeEnergy(amount);
                        }
                    }
                },
                OnTransmitEnergy = (amount, destination, self) =>
                {
                    if (self.GetEnergy() <= amount)
                        return;
            
                    if (destination == self.GetController())
                        return;

                    List<Node> path = AStar.FindPath(self, destination.GetNode());
                    if (path == null)
                    {
                        Debug.Log("Path is null");
                        return;
                    }
            
                    self.TakeEnergy(amount);

                    var tempGO = GameObject.Instantiate(self.GetController().EnergyPrefab,
                        new Vector3(self.GetPosition().x - 5.5f,
                            self.GetPosition().y - 9.5f, 0), Quaternion.identity);
                    EnergyPathfinder e = tempGO.GetComponent<EnergyPathfinder>();
                    e.Spawn(path, destination, amount, self.GetOwner());
                    NetworkServer.Spawn(tempGO);
                }
            }},
            // Production Node
            {1, new ComponentData
            {
                EnergyCap = 16,
                BuildCost = 15,
                FoundationRequirement = NodeType.DATA_BUS,
                OnCreation = (self) =>
                {
                    self.GetController().EnableHChevrons(true);
                    self.GetController().EnableVChevrons(true);
                },
                OnAction = (amount, self) => {
                    List<Node> neighbours = self.GetNodeNeighbours();
                    foreach (var node in neighbours)
                    {
                        if (node is IEnergyReceiver)
                        {
                            self.AddEnergy(1);
                            self.TransmitEnergy(node.GetController(), amount);
                        }
                    }
                },
                OnTransmitEnergy = (amount, destination, self) => {
                    if (self.GetEnergy() <= amount)
                        return;
            
                    if (destination == self.GetController())
                        return;

                    List<Node> path = AStar.FindPath(self, destination.GetNode());
                    if (path == null)
                    {
                        Debug.Log("Path is null");
                        return;
                    }
            
                    self.TakeEnergy(amount);

                    var tempGO = GameObject.Instantiate(self.GetController().EnergyPrefab,
                        new Vector3(self.GetPosition().x - 5.5f,
                            self.GetPosition().y - 9.5f, 0), Quaternion.identity);
                    EnergyPathfinder e = tempGO.GetComponent<EnergyPathfinder>();
                    e.Spawn(path, destination, amount, self.GetOwner());
                    NetworkServer.Spawn(tempGO);
                },
                OnReceiveEnergy = (amount, sender, self) => {
                    if (sender == self.GetOwner())
                    {
                        self.AddEnergy(amount);
                        self.OnEnergyReceivedCall();
                    }
                    else
                    {
                        if (amount > self.GetEnergy())
                        {
                            self.SetOwner(sender);
                            self.SetEnergy(0);
                            self.AddEnergy(amount);
                        }
                        else
                        {
                            self.TakeEnergy(amount);
                        }
                    }
                },
            }},
            // Battery Node
            {2, new ComponentData
            {
                EnergyCap = 48,
                BuildCost = 10,
                FoundationRequirement = NodeType.NODE,
                OnCreation = (self) =>
                {
                    self.GetController().EnableHChevrons(true);
                    self.GetController().EnableVChevrons(false);
                },
                OnReceiveEnergy = (amount, sender, self) => {
                    if (sender != self.GetOwner())
                    {
                        self.SetOwner(sender);
                        self.AddEnergy(amount);
                
                        self.OnEnergyTransmittedCall();
                    }
                    else
                    {
                        self.AddEnergy(amount);
                    }
                },
                OnTransmitEnergy = (amount, destination, self) => {
                    if (self.GetEnergy() <= amount)
                        return;
            
                    if (destination == self.GetController())
                        return;

                    List<Node> path = AStar.FindPath(self, destination.GetNode());
                    if (path == null)
                    {
                        Debug.Log("Path is null");
                        return;
                    }
            
                    self.TakeEnergy(amount);

                    var tempGO = GameObject.Instantiate(self.GetController().EnergyPrefab,
                        new Vector3(self.GetPosition().x - 5.5f,
                            self.GetPosition().y - 9.5f, 0), Quaternion.identity);
                    EnergyPathfinder e = tempGO.GetComponent<EnergyPathfinder>();
                    e.Spawn(path, destination, amount, self.GetOwner());
                    NetworkServer.Spawn(tempGO);
                },
            }},
            // Barrier Node
            {3, new ComponentData
            {
                EnergyCap = 64,
                BuildCost = 10,
                FoundationRequirement = NodeType.NODE,
                OnCreation = (self) =>
                {
                    self.GetController().EnableHChevrons(false);
                    self.GetController().EnableVChevrons(true);
                },
                OnReceiveEnergy = (amount, sender, self) => {
                    if (sender == self.GetOwner())
                    {
                        self.AddEnergy(amount * 2);
                        self.OnEnergyReceivedCall();
                    }
                    else
                    {
                        if (amount > self.GetEnergy())
                        {
                            self.SetOwner(sender);
                            self.SetEnergy(0);
                            self.AddEnergy(amount);
                        }
                        else
                        {
                            self.TakeEnergy(amount);
                        }
                    }
                }
            }}
        };
    }
}