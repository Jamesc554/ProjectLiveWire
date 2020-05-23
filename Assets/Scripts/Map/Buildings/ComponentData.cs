using System;
using Mirror;

namespace Map.Buildings
{
    public class ComponentData
    {
        public int BuildCost;
        public NodeType FoundationRequirement;
        public int EnergyCap;

        public Action<BaseNode> OnCreation;
        public Action<int, NetworkIdentity, BaseNode> OnReceiveEnergy;
        public Action<int, NodeController, BaseNode> OnTransmitEnergy;
        public Action<int, BaseNode> OnAction;

        public void Create(BaseNode self)
        {
            OnCreation?.Invoke(self);
        }
        
        public void ReceiveEnergy(int amount, NetworkIdentity sender, BaseNode self)
        {
            OnReceiveEnergy?.Invoke(amount, sender, self);
        }
        
        public void TransmitEnergy(int amount, NodeController destination, BaseNode self)
        {
            OnTransmitEnergy?.Invoke(amount, destination, self);
        }
        
        public void DoAction(int amount, BaseNode self)
        {
            OnAction?.Invoke(amount, self);
        }
    }
}