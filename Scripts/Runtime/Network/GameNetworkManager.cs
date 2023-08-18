using System;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

namespace Multiplayer.Runtime.Network
{
    public class GameNetworkManager : MonoBehaviour
    {
        public static GameNetworkManager Instance { get; private set; } = null;
        public Lobby? curLobby { get; private set; } = null;
        
        private FacepunchTransport transport;

        private void Awake()
        {
            if (Instance is not null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            transport = GetComponent<FacepunchTransport>();

            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
            SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        }

        private void OnDestroy()
        {
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
            SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
            if (NetworkManager.Singleton is null) return;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
        
        public async void StartHost(int maxHosts = 30)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.StartHost();
            
            curLobby = await SteamMatchmaking.CreateLobbyAsync(maxHosts);
        }

        public void OnApplicationQuit() => Disconnect();

        public void StartClient(SteamId id)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

            transport.targetSteamId = id;
            
            if (NetworkManager.Singleton.StartClient())
                Debug.Log("Joined");
        }
        
        public void Disconnect()
        {
            curLobby?.Leave();

            if (NetworkManager.Singleton is null) return;
            NetworkManager.Singleton.Shutdown();
        }
        
        #region Steam Callbacks
        private void OnGameLobbyJoinRequested(Lobby lobby, SteamId id) => StartClient(id);

        private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
        {
        }

        private void OnLobbyInvite(Friend friend, Lobby lobby) => Debug.Log($"New Invite from {friend.Name}");

        private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
        {
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
        }

        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            if (result is not Result.OK) Debug.LogError($"Lobby failed to connect {result}");

            lobby.SetFriendsOnly();
            //lobby.SetPrivate();
            //lobby.SetPublic();

            lobby.SetData("gameName", "lobbyName");
            lobby.SetJoinable(true);
            
            Debug.Log("Lobby successfully created!");
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            if (NetworkManager.Singleton.IsHost) return;
            StartClient(lobby.Id);
        }
        
        #endregion

        #region UnityCallbacks

        private void OnServerStarted() => Debug.Log("Server Started!");
        private void OnClientConnectedCallback(ulong clientID) => Debug.Log($"Client connected! id={clientID}");

        private void OnClientDisconnectCallback(ulong clientID)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            Debug.Log($"Client disconnected! id={clientID}");
        }

        #endregion
    }
}
