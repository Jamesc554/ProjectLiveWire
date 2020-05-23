using Mirror;

namespace Map.Buildings
{
    public interface IEnergyHolder
    {
        void SetEnergy(int amount);
        void AddEnergy(int amount);
        void TakeEnergy(int amount);
        int GetEnergy();
    }
}