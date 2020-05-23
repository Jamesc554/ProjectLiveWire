using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Map.Buildings
{
    public class BaseNode : Node, IEnergyReceiver, IEnergyTransmitter, IOwnable
    {
        public event Action OnOwnerChanged;
        public event Action<int> OnEnergyReceived;
        public event Action<int> OnEnergyTransmitted;
        
        private int _energy;
        private NetworkIdentity _owner;

        private ComponentData _componentData;

        public BaseNode(Vector2Int position, NodeType nodeType) : base(position, nodeType)
        {
            OnEnergyReceived += CheckEnergyLimit;
        }

        public void SetComponentData(int id)
        {
            _componentData = Components.ComponentData[id];
            _componentData.Create(this);
            GetController().SetComponentId(id);
        }

        public void TakeEnergy(int amount)
        {
            _energy -= amount;
        }

        public int GetEnergy()
        {
            return _energy;
        }

        public void SetEnergy(int energy)
        {
            _energy = Math.Min(energy, _componentData.EnergyCap);
            
            OnEnergyReceived?.Invoke(GetEnergy());
        }

        public void AddEnergy(int amount)
        {
            _energy += amount;
            if (GetEnergy() > _componentData.EnergyCap)
            {
                _energy = _componentData.EnergyCap;
            }
        }

        public void DoAction(int amount)
        {
            _componentData.DoAction(amount, this);
        }

        public virtual void ReceiveEnergy(int amount, NetworkIdentity sender)
        {
            _componentData.ReceiveEnergy(amount, sender, this);
            OnEnergyReceived?.Invoke(GetEnergy());
        }

        public virtual void ReceiveEnergy(int amount)
        {
            _componentData.ReceiveEnergy(amount, GetOwner(), this);
            OnEnergyReceived?.Invoke(GetEnergy());
        }

        public virtual void TransmitEnergy(NodeController destination, int amount)
        {
            _componentData.TransmitEnergy(amount, destination, this);
            OnEnergyTransmitted?.Invoke(GetEnergy());
        }

        public NetworkIdentity GetOwner()
        {
            return _owner;
        }

        public void SetOwner(NetworkIdentity owner)
        {
            _owner = owner;
            OnOwnerChanged?.Invoke();
        }

        public void OnEnergyTransmittedCall()
        {
            OnEnergyTransmitted?.Invoke(GetEnergy());
        }

        public void OnEnergyReceivedCall()
        {
            OnEnergyReceived?.Invoke(GetEnergy());
        }

        protected void CheckEnergyLimit(int amount)
        {
            if (GetEnergy() > _componentData.EnergyCap)
            {
                SetEnergy(_componentData.EnergyCap);
            }
        }
    }
}