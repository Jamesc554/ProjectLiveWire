namespace Map.Buildings
{
    public interface IEnergyProducer : IEnergyReceiver
    {
        void ProduceEnergy(int amount);
    }
}