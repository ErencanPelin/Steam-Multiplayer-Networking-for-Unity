using Network.Framework;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using Unity.Netcode;

namespace Multiplayer.Runtime.UI
{
    [AddComponentMenu("Network/UI/UI Lobby Menu"), DisallowMultipleComponent]
    public class LobbyMenuController : MonoBehaviour
    {
        [Header("References")]
      //  [SerializeField] private Button m_MapBtn;
     //   [SerializeField] private Button m_ModeBtn;
     //   [SerializeField] private Button m_SettingsBtn;
        [SerializeField] private Button m_ReturnHomeBtn;
        [SerializeField] private Button m_CopyLinkBtn;
        [SerializeField] private Button m_StartGameBtn;

    //    [Header("Members")]
    //    [SerializeField] private Transform m_MemberRect;
    //    [SerializeField] private UIMember m_MemberPrfeab;

        private void Start()
        {
            m_ReturnHomeBtn.onClick.AddListener(ReturnHome);
            m_CopyLinkBtn.onClick.AddListener(CopyLink);
            m_StartGameBtn.onClick.AddListener(OnStartGameClicked);

            NW_NetworkManager.OnConnectionCompleted += OnConnectionCompleted;
            NW_NetworkManager.OnMemberDataChangedEvent += OnMemberDataChanged;
            NW_NetworkManager.OnMemberJoinedEvent += OnMemberJoined;
            NW_NetworkManager.OnMemberLeftEvent += OnMemberLeft;
            NW_NetworkManager.OnMemberKickedEvent += OnMemberKicked;
            NW_NetworkManager.OnMemberBannedEvent += OnMemberBanned;
        }

        private void OnEnable() => RefreshUI();

        private void OnDestroy()
        {
            m_ReturnHomeBtn.onClick.RemoveAllListeners();
            m_CopyLinkBtn.onClick.RemoveAllListeners();
            m_StartGameBtn.onClick.RemoveAllListeners();

            NW_NetworkManager.OnConnectionCompleted -= OnConnectionCompleted;
            NW_NetworkManager.OnMemberDataChangedEvent -= OnMemberDataChanged;
            NW_NetworkManager.OnMemberJoinedEvent -= OnMemberJoined;
            NW_NetworkManager.OnMemberLeftEvent -= OnMemberLeft;
            NW_NetworkManager.OnMemberKickedEvent -= OnMemberKicked;
            NW_NetworkManager.OnMemberBannedEvent -= OnMemberBanned;
        }

        private void ReturnHome() => NW_NetworkManager.Instance.RequestDisconnect();

        private void OnStartGameClicked() => StartGameServerRpc();

        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId)
                return;

            ///TODO: Check if everybody is ready!

            NW_ServerManager.Instance.StartGame();
        }

        private void RefreshUI()
        {
            var isActive = NetworkManager.Singleton.IsServer;

        //    m_MapBtn.gameObject.SetActive(isActive);
        //    m_ModeBtn.gameObject.SetActive(isActive);
        //    m_SettingsBtn.gameObject.SetActive(isActive);
            m_StartGameBtn.gameObject.SetActive(isActive);
            m_CopyLinkBtn.gameObject.SetActive(isActive);
            m_CopyLinkBtn.gameObject.SetActive(isActive);

            var lobby = NW_NetworkManager.Instance.CurrentLobby;

            if (!lobby.HasValue)
                return;

            /*for (var i = 0; i < m_MemberRect.childCount; i++)
                Destroy(m_MemberRect.GetChild(i).gameObject); // Delete old members!*/

            if (lobby.Value.Members == null || lobby.Value.MemberCount == 0)
                return;

            /*foreach (var member in lobby.Value.Members)
                CreateUIMember(member); // Spawn new members!*/

            /*void CreateUIMember(Friend friend)
            {
                var member = Instantiate(m_MemberPrfeab, m_MemberRect);
                member.Init(friend);
            }*/
        }

        private void CopyLink()
        {
            var lobby = NW_NetworkManager.Instance.CurrentLobby;

            if (lobby.HasValue)
            {
                GUIUtility.systemCopyBuffer = NW_SteamExtensions.GetShareableLink(lobby.Value);
            }
        }

        private void OnConnectionCompleted(NW_NetworkExtensions.ConnectStatus status) => RefreshUI();

        #region Steam Callbacks

        private void OnMemberJoined(Friend friend) => RefreshUI();

        private void OnMemberDataChanged(Friend friend) => RefreshUI();

        private void OnMemberLeft(Friend friend) => RefreshUI();

        private void OnMemberKicked(Friend friend, Friend user) => RefreshUI();

        private void OnMemberBanned(Friend friend, Friend user) => RefreshUI();

        #endregion
    }
}
