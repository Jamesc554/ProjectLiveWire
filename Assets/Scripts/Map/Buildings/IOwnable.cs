using System;
using Mirror;

namespace Map.Buildings
{
    public interface IOwnable
    {
        event Action OnOwnerChanged;
        NetworkIdentity GetOwner();
        void SetOwner(NetworkIdentity owner);
    }
}