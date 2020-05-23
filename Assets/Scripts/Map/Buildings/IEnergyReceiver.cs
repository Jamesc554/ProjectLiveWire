using System;
using Mirror;

namespace Map.Buildings
{
    public interface IEnergyReceiver : IEnergyHolder
    {
        event Action<int> OnEnergyReceived;
        void ReceiveEnergy(int amount, NetworkIdentity sender);
        void ReceiveEnergy(int amount);
    }
}