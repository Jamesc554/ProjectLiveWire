using System;
using System.Collections.Generic;
using System.Linq;
using Map;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

namespace Lobby
{
	public class NetworkManagerVirus : NetworkManager
	{
		[SerializeField] private int minPlayers = 2;
		[Scene] [SerializeField] private string menuScene = string.Empty;
		[Scene] [SerializeField] private string gameScene = string.Empty;

		[Header("Room")] 
		[SerializeField] private NetworkRoomPlayerVirus roomPlayerPrefab;
		[Header("Game")] 
		[SerializeField] private NetworkGamePlayerVirus gamePlayerPrefab;
		[SerializeField] private GridManager gridManager;
		[SerializeField] private GameObject GlobalOwnerPrefab;
		[SerializeField] public NetworkIdentity GlobalOwner;
		[SerializeField] private bool gameStarted;

		public static event Action OnClientConnected;
		public static event Action OnClientDisconnected;
		public static event Action<NetworkConnection> OnServerReadied;
		public static event Action OnServerStopped;
		
		// Game Events
		public static event Action OnEnergyChanged;

		public List<NetworkRoomPlayerVirus> RoomPlayers { get; } = new List<NetworkRoomPlayerVirus>();
		public List<NetworkGamePlayerVirus> GamePlayers { get; } = new List<NetworkGamePlayerVirus>();
		public List<NodeController> StartingNodes { get; } = new List<NodeController>();
		public List<NodeController> Nodes { get; } = new List<NodeController>();

		public void SetGridManager(GridManager gridManager)
		{
			this.gridManager = gridManager;
		}

		public GridManager GetGridManager()
		{
			return gridManager;
		}
		
		public override void OnStartServer()
		{
			spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
			GameObject go = Instantiate(GlobalOwnerPrefab);
			GlobalOwner = go.GetComponent<NetworkIdentity>();
			NetworkServer.Spawn(go);
		}

		public override void OnStartClient()
		{
			var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

			foreach (var prefab in spawnablePrefabs)
			{
				ClientScene.RegisterPrefab(prefab);
			}
		}

		public override void OnClientConnect(NetworkConnection conn)
		{
			base.OnClientConnect(conn);
			
			OnClientConnected?.Invoke();
		}

		public override void OnClientDisconnect(NetworkConnection conn)
		{
			base.OnClientDisconnect(conn);
			
			OnClientDisconnected?.Invoke();
		}

		public override void OnServerConnect(NetworkConnection conn)
		{
			base.OnServerConnect(conn);

			if (numPlayers >= maxConnections)
			{
				conn.Disconnect();
				return;
			}

			if (SceneManager.GetActiveScene().path != menuScene)
			{
				conn.Disconnect();
				return;
			}
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			//base.OnServerAddPlayer(conn);

			if (SceneManager.GetActiveScene().path == menuScene)
			{
				bool isLeader = RoomPlayers.Count == 0;
				NetworkRoomPlayerVirus roomPlayerInstance = Instantiate(roomPlayerPrefab);


				roomPlayerInstance.IsLeader = isLeader;
				
				NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
			}
		}

		public override void OnServerDisconnect(NetworkConnection conn)
		{
			if (conn.identity != null)
			{
				var player = conn.identity.GetComponent<NetworkRoomPlayerVirus>();

				RoomPlayers.Remove(player);

				NotifyPlayersOfReadyState();
			}
			
			base.OnServerDisconnect(conn);
		}

		public override void OnStopServer()
		{
			OnServerStopped?.Invoke();

			RoomPlayers.Clear();
			GamePlayers.Clear();
		}

		public void NotifyPlayersOfReadyState()
		{
			foreach (var player in RoomPlayers)
			{
				player.HandleReadyToStart(IsReadyToStart());
			}
		}

		private bool IsReadyToStart()
		{
			if (numPlayers < minPlayers) return false;

			foreach (var player in RoomPlayers)
			{
				if (!player.IsReady)
				{
					return false;
				}
			}

			return true;
		}

		public void StartGame()
		{
			if (SceneManager.GetActiveScene().path == menuScene)
			{
				if (!IsReadyToStart())
					return;
				
				ServerChangeScene(gameScene);
			}
		}

		public override void ServerChangeScene(string newSceneName)
		{
			if (SceneManager.GetActiveScene().path == menuScene)
			{
				for (int i = RoomPlayers.Count - 1; i >= 0; i--)
				{
					var conn = RoomPlayers[i].connectionToClient;
					NetworkGamePlayerVirus gamePlayerInstance = Instantiate(gamePlayerPrefab);
					gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
					
					NetworkServer.Destroy(conn.identity.gameObject);
					NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject, true);
				}
			}
			base.ServerChangeScene(newSceneName);
		}

		public override void OnServerReady(NetworkConnection conn)
		{
			base.OnServerReady(conn);
			
			OnServerReadied?.Invoke(conn);
		}

		public void BeginGame()
		{
			gameStarted = true;
		}

		public bool HasGameStarted()
		{
			return gameStarted;
		}
	}
}
