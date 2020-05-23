using System;

namespace Map.Buildings
{
    public interface IEnergyTransmitter : IEnergyHolder
    {
        event Action<int> OnEnergyTransmitted;
        void TransmitEnergy(NodeController destination, int amount);
    }
}