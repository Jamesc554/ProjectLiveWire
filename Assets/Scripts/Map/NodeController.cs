using System;
using System.Collections.Generic;
using Lobby;
using Map.Buildings;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Map
{
    public class NodeController : NetworkBehaviour
    {
        [Header("Node Display")]
        [SerializeField] private GameObject chevronsH;
        [SerializeField] private GameObject chevronsV;
        [SerializeField] private Image energyDisplayImage;

        [SyncVar(hook = nameof(HandleChevronHUpdate))] private bool _chevronHEnabled;
        [SyncVar(hook = nameof(HandleChevronVUpdate))] private bool _chevronVEnabled;
        
        public static event Action OnEnergyChanged;
        [SerializeField] private Node node;
        [SyncVar]
        private int componentId;

        [SerializeField] public GameObject EnergyPrefab;
        
        [SerializeField] private TMP_Text EnergyDisplay;

        [SyncVar(hook = nameof(HandleColorChanged))]
        public Color color;

        [SyncVar(hook = nameof(HandleEnergyChanged))]
        public int energy;

        private SpriteRenderer cachedRenderer;
        
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

        #region Server

        #region Callbacks

        public override void OnStartServer()
        {
            if (node.GetNodeType() != NodeType.HOR_CONNECTION || node.GetNodeType() != NodeType.VERT_CONNECTION || node.GetNodeType() != NodeType.BLANK)
                NetManager.Nodes.Add(this);
        }

        #endregion

        [Server]
        public void SetComponentId(int id)
        {
            componentId = id;
        }
        
        [Server]
        public int GetComponentId()
        {
            return componentId;
        }
        
        [Server]
        private List<Node> GetNeighbours()
        {
            return node.GetNeighbours();
        }
        
        [Server]
        public void UpdateConnections()
        {
            foreach (var connection in GetNeighbours())
            {
                connection.GetController().color = connection.GetController().GetColorFromNeighbours();
            }
        }
        
        [Server]
        private Color GetColorFromNeighbours()
        {
            List<Node> nodes = GetNeighbours();
            
            if (nodes.Count < 2)
                return nodes[0].GetController().color;
            
            return Color.Lerp(nodes[0].GetController().color, nodes[1].GetController().color, 0.5f);
        }
        
        [Server]
        public void SetOwner(NetworkIdentity newOwner, Color newColor)
        {
            if (node is IOwnable ownable)
            {
                ownable.SetOwner(newOwner);
                Debug.Log("New Owner!");
            }
            else
            {
                Debug.LogError($"{newOwner.GetComponent<NetworkGamePlayerVirus>().GetDisplayName()} Tried to take ownership of unownable node!");
            }
        }

        [Server]
        private void UpdateColours()
        {
            if (node is IOwnable ownable)
            {
                color = ownable.GetOwner().GetComponent<NetworkGamePlayerVirus>().GetColor();
                UpdateConnections();
            }
        }
        
        [Server]
        public void AddEnergy(int amount, NetworkIdentity sender)
        {
            if (node is IEnergyReceiver receiver)
            {
                receiver.ReceiveEnergy(amount, sender);
            }
            
            OnEnergyChanged?.Invoke();
            
        }

        [Server]
        public void SendEnergy(int amount, NodeController destination)
        {
            if (node is IEnergyTransmitter transmitter)
            {
                transmitter.TransmitEnergy(destination, amount);
            }
            
            OnEnergyChanged?.Invoke();
        }

        [Server]
        public void SetEnergy(int amount)
        {
            if (node is IEnergyReceiver receiver)
            {
                receiver.SetEnergy(amount);
            } 
            else 
            {
                Debug.LogError("Tried to put energy in node which cannot receive energy!");
            }
        }
        
        [Server]
        public void SetNode(Node node)
        {
            if (this.node != null)
            {
                if (this.node is IOwnable ownableO)
                {
                    ownableO.OnOwnerChanged -= UpdateColours;
                }

                if (this.node is IEnergyTransmitter transmitterO)
                {
                    transmitterO.OnEnergyTransmitted -= SetSyncedEnergy;
                }

                if (this.node is IEnergyReceiver receiverO)
                {
                    receiverO.OnEnergyReceived -= SetSyncedEnergy;
                }
            }

            this.node = node;
            this.node.SetController(this);

            if (node is IOwnable ownable)
            {
                ownable.OnOwnerChanged += UpdateColours;
            }
            
            if (node is IEnergyTransmitter transmitter)
            {
                transmitter.OnEnergyTransmitted += SetSyncedEnergy;
            }

            if (node is IEnergyReceiver receiver)
            {
                receiver.OnEnergyReceived += SetSyncedEnergy;
            }

            if (node is IEnergyHolder holder)
            {
                energy = holder.GetEnergy();
            }
        }

        private void SetSyncedEnergy(int amount)
        {
            energy = amount;
        }

        [Server]
        public Node GetNode()
        {
            return node;
        }

        #endregion

        #region Client

        #region Unity Functions

        private void OnDestroy()
        {
            Destroy(cachedRenderer);
        }
        
        public void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(1))
            {
                NetworkIdentity ni = NetworkClient.connection.identity;
                NetworkGamePlayerVirus player = ni.GetComponent<NetworkGamePlayerVirus>();
                
                player.CmdMoveEnergy(gameObject.GetComponent<NetworkIdentity>(), 1, !Input.GetKey(KeyCode.LeftShift));
            }
        }

        #endregion

        [Client]
        public void HandleColorChanged(Color oldValue, Color newValue)
        {
            if (cachedRenderer == null)
                cachedRenderer = GetComponent<SpriteRenderer>();

            cachedRenderer.color = newValue;

            if (GetNode() is BaseNode)
            {
                chevronsH.GetComponent<SpriteRenderer>().color = Color.Lerp(newValue, Color.white, 0.75f);
                chevronsV.GetComponent<SpriteRenderer>().color = Color.Lerp(newValue, Color.white, 0.75f);
                energyDisplayImage.color = newValue;
            }
        }
        
        [Client]
        public void HandleEnergyChanged(int oldValue, int newValue)
        {
            if (newValue == 0)
            {
                EnergyDisplay.text = string.Empty;
                energyDisplayImage.fillAmount = 0;
            }
            else
            {
                EnergyDisplay.text = newValue.ToString();
                float fillAmount = (float)newValue / Components.ComponentData[componentId].EnergyCap;
                energyDisplayImage.fillAmount = fillAmount;
            }
        }

        [Client]
        public void HandleChevronHUpdate(bool oldValue, bool newValue)
        {
            chevronsH.SetActive(newValue);
        }
        
        [Client]
        public void HandleChevronVUpdate(bool oldValue, bool newValue)
        {
            chevronsV.SetActive(newValue);
        }

        #endregion

        #region Shared

        public Color GetColor()
        {
            return color;
        }
        
        public NetworkIdentity GetOwner()
        {
            if (node is IOwnable ownable)
            {
                if (ownable.GetOwner() != null)
                    return ownable.GetOwner();
            }
            
            return NetManager.GlobalOwner;
        }
        
        public int GetEnergy()
        {
            if (node is IEnergyHolder holder)
            {
                return holder.GetEnergy();
            }

            return 0;
        }

        public void EnableHChevrons(bool val)
        {
            _chevronHEnabled = val;
        }
        
        public void EnableVChevrons(bool val)
        {
            _chevronVEnabled = val;
        }

        #endregion
    }
}