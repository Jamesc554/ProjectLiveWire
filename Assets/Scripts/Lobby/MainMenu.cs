using UnityEngine;

namespace Lobby
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private NetworkManagerVirus networkManager;

        [Header("UI")] 
        [SerializeField] private GameObject landingPagePanel;

        public void HostLobby()
        {
            networkManager.networkAddress = "192.168.0.45";
            networkManager.StartHost();
            
            landingPagePanel.SetActive(false);
        }
    }
}