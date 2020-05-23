using System;
using Mirror.Examples.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class JoinLobbyMenu : MonoBehaviour
    {
        [SerializeField] private NetworkManagerVirus networkManager;

        [Header("UI")] 
        [SerializeField] private GameObject landingPagePanel;
        [SerializeField] private TMP_InputField ipAddressInputField;
        [SerializeField] private Button joinButton;

        private void OnEnable()
        {
            NetworkManagerVirus.OnClientConnected += HandleClientConnected;
            NetworkManagerVirus.OnClientDisconnected += HandleClientDisconnected;

            if (PlayerPrefs.HasKey("ServerIp"))
                ipAddressInputField.text = PlayerPrefs.GetString("ServerIp");
            
        }

        private void OnDisable()
        {
            NetworkManagerVirus.OnClientConnected -= HandleClientConnected;
            NetworkManagerVirus.OnClientDisconnected -= HandleClientDisconnected;
        }

        public void JoinLobby()
        {
            string ipAddress = ipAddressInputField.text;
            PlayerPrefs.SetString("ServerIp", ipAddress);
            PlayerPrefs.Save();

            networkManager.networkAddress = ipAddress;
            networkManager.StartClient();

            joinButton.interactable = false;
        }

        private void HandleClientConnected()
        {
            joinButton.interactable = true;
            
            gameObject.SetActive(false);
            landingPagePanel.SetActive(false);
        }

        private void HandleClientDisconnected()
        {
            joinButton.interactable = true;
        }
    }
}