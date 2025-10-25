using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Lean.Gui;

/// <summary>
/// Gestiona la SALA DE ESPERA antes de iniciar el juego
/// Solo se usa en RoomScene
/// </summary>
public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("UI - Información de Sala")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;

    [Header("UI - Lista de Jugadores")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerItemPrefab;

    [Header("UI - Botones")]
    [SerializeField] private LeanButton playButton;
    [SerializeField] private LeanButton leaveButton;
    [SerializeField] private LeanButton updateButton;

    [Header("UI - Configuración de Economía (Solo Host)")]
    [SerializeField] private GameObject economySettingsPanel;
    [SerializeField] private TMP_InputField denominacionMinimaInput;
    [SerializeField] private TMP_InputField valorMediterraneoInput;

    [Header("Configuración")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private int minPlayersToStart = 2;

    private Dictionary<int, GameObject> playerListItems = new Dictionary<int, GameObject>();

    private void Start()
    {
        // Verificar que estamos en una sala
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[RoomManager] No estamos en una sala, regresando al lobby");
            SceneManager.LoadScene("LobbyScene");
            return;
        }

        // Configurar botones
        SetupButtons();

        // Actualizar UI inicial
        UpdateRoomInfo();
        UpdatePlayerList();
        UpdateButtonVisibility();

        // Configurar panel de economía
        SetupEconomyPanel();
    }

    #region CONFIGURACIÓN

    private void SetupButtons()
    {
        if (playButton != null)
        {
            playButton.OnClick.AddListener(OnPlayButtonClicked);
        }

        if (leaveButton != null)
        {
            leaveButton.OnClick.AddListener(OnLeaveButtonClicked);
        }

        if (updateButton != null)
        {
            updateButton.OnClick.AddListener(OnUpdateButtonClicked);
        }
    }

    private void SetupEconomyPanel()
    {
        if (economySettingsPanel != null)
        {
            economySettingsPanel.SetActive(PhotonNetwork.IsMasterClient);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            LoadEconomySettings();
        }
    }

    private void LoadEconomySettings()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("DenominacionMinima"))
        {
            int denominacion = (int)PhotonNetwork.CurrentRoom.CustomProperties["DenominacionMinima"];
            if (denominacionMinimaInput != null)
                denominacionMinimaInput.text = denominacion.ToString();
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("ValorMediterraneo"))
        {
            int valor = (int)PhotonNetwork.CurrentRoom.CustomProperties["ValorMediterraneo"];
            if (valorMediterraneoInput != null)
                valorMediterraneoInput.text = valor.ToString();
        }
    }

    public void SaveEconomySettings()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int denominacion = 1;
        int valorMediterraneo = 60;

        if (denominacionMinimaInput != null && int.TryParse(denominacionMinimaInput.text, out int den))
        {
            denominacion = Mathf.Max(1, den);
        }

        if (valorMediterraneoInput != null && int.TryParse(valorMediterraneoInput.text, out int val))
        {
            valorMediterraneo = Mathf.Max(1, val);
        }

        Hashtable roomProperties = new Hashtable
        {
            { "DenominacionMinima", denominacion },
            { "ValorMediterraneo", valorMediterraneo }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        Debug.Log($"[RoomManager] Economía configurada: Denominación={denominacion}, Mediterráneo={valorMediterraneo}");
    }

    #endregion

    #region LISTA DE JUGADORES

    private void UpdateRoomInfo()
    {
        if (roomNameText != null)
        {
            roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        }

        if (playerCountText != null)
        {
            playerCountText.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        }
    }

    private void UpdatePlayerList()
    {
        foreach (var item in playerListItems.Values)
        {
            if (item != null)
                Destroy(item);
        }
        playerListItems.Clear();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            CreatePlayerItem(player);
        }
    }

    private void CreatePlayerItem(Player player)
    {
        if (playerItemPrefab == null || playerListContainer == null)
            return;

        GameObject playerItem = Instantiate(playerItemPrefab, playerListContainer);
        TMP_Text playerText = playerItem.GetComponentInChildren<TMP_Text>();

        if (playerText != null)
        {
            string displayName = player.NickName;

            if (player.IsMasterClient)
            {
                displayName += " (Host)";
            }

            playerText.text = displayName;
        }

        playerListItems[player.ActorNumber] = playerItem;
    }

    private void UpdateButtonVisibility()
    {
        bool isMaster = PhotonNetwork.IsMasterClient;

        if (playButton != null)
        {
            playButton.gameObject.SetActive(isMaster);

            if (isMaster)
            {
                bool canStart = PhotonNetwork.CurrentRoom.PlayerCount >= minPlayersToStart;
                playButton.interactable = canStart;
            }
        }

        if (leaveButton != null)
        {
            leaveButton.gameObject.SetActive(true);
        }
    }

    #endregion

    #region BOTONES

    private void OnPlayButtonClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[RoomManager] Solo el creador puede iniciar el juego");
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < minPlayersToStart)
        {
            Debug.LogWarning($"[RoomManager] Se necesitan al menos {minPlayersToStart} jugadores");
            return;
        }

        // Guardar configuración de economía
        SaveEconomySettings();

        // Cerrar sala
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // Cambiar estado
        Hashtable gameStateProps = new Hashtable
        {
            { "EstadoJuego", "EnJuego" },
            { "GameStartTime", PhotonNetwork.Time } // Guardar tiempo de inicio
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(gameStateProps);

        Debug.Log("[RoomManager] Iniciando juego...");

        // Cargar escena de juego
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    private void OnLeaveButtonClicked()
    {
        Debug.Log("[RoomManager] Saliendo de la sala...");
        PhotonNetwork.LeaveRoom();
    }

    private void OnUpdateButtonClicked()
    {
        UpdatePlayerList();
        UpdateRoomInfo();
    }

    #endregion

    #region CALLBACKS

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[RoomManager] Jugador entró: {newPlayer.NickName}");
        UpdatePlayerList();
        UpdateRoomInfo();
        UpdateButtonVisibility();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[RoomManager] Jugador salió: {otherPlayer.NickName}");
        UpdatePlayerList();
        UpdateRoomInfo();
        UpdateButtonVisibility();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"[RoomManager] Nuevo host: {newMasterClient.NickName}");
        UpdateButtonVisibility();
        SetupEconomyPanel();
        UpdatePlayerList();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[RoomManager] Saliste de la sala");
        SceneManager.LoadScene("LobbyScene");
    }

    #endregion

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.OnClick.RemoveAllListeners();

        if (leaveButton != null)
            leaveButton.OnClick.RemoveAllListeners();

        if (updateButton != null)
            updateButton.OnClick.RemoveAllListeners();
    }
}