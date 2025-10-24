using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using ExitGames.Client.Photon;

/// <summary>
/// Gestiona la lógica dentro de una sala: 
/// - Mostrar jugadores
/// - Botón JUGAR (solo creador)
/// - Botón SALIR (todos)
/// - Iniciar juego
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
    [SerializeField] private Button playButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button updateButton;

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
            Debug.LogWarning("No estamos en una sala, regresando al lobby");
            SceneManager.LoadScene("LobbyScene"); // Ajusta el nombre de tu escena de lobby
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

    /// <summary>
    /// Configura los botones de la sala
    /// </summary>
    private void SetupButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        if (leaveButton != null)
        {
            leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        }

        if (updateButton != null)
        {
            updateButton.onClick.AddListener(OnUpdateButtonClicked);
        }
    }

    /// <summary>
    /// Configura el panel de economía
    /// </summary>
    private void SetupEconomyPanel()
    {
        if (economySettingsPanel != null)
        {
            // Solo el creador ve el panel de configuración
            economySettingsPanel.SetActive(PhotonNetwork.IsMasterClient);
        }

        // Cargar valores actuales de la sala
        if (PhotonNetwork.IsMasterClient)
        {
            LoadEconomySettings();
        }
    }

    /// <summary>
    /// Carga la configuración de economía actual
    /// </summary>
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

    /// <summary>
    /// Guarda la configuración de economía (solo host)
    /// </summary>
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

        // Actualizar propiedades de la sala
        Hashtable roomProperties = new Hashtable
        {
            { "DenominacionMinima", denominacion },
            { "ValorMediterraneo", valorMediterraneo }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        Debug.Log($"Economía configurada: Denominación={denominacion}, Mediterráneo={valorMediterraneo}");
    }

    /// <summary>
    /// Actualiza la información de la sala
    /// </summary>
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

    /// <summary>
    /// Actualiza la lista de jugadores
    /// </summary>
    private void UpdatePlayerList()
    {
        // Limpiar items anteriores
        foreach (var item in playerListItems.Values)
        {
            if (item != null)
                Destroy(item);
        }
        playerListItems.Clear();

        // Crear items para cada jugador
        foreach (var player in PhotonNetwork.PlayerList)
        {
            CreatePlayerItem(player);
        }
    }

    /// <summary>
    /// Crea un item de jugador en la lista
    /// </summary>
    private void CreatePlayerItem(Player player)
    {
        if (playerItemPrefab == null || playerListContainer == null)
            return;

        GameObject playerItem = Instantiate(playerItemPrefab, playerListContainer);
        TMP_Text playerText = playerItem.GetComponentInChildren<TMP_Text>();

        if (playerText != null)
        {
            string displayName = player.NickName;

            // Marcar al creador de la sala
            if (player.IsMasterClient)
            {
                displayName += " (Host)";
            }

            playerText.text = displayName;
        }

        playerListItems[player.ActorNumber] = playerItem;
    }

    /// <summary>
    /// Actualiza la visibilidad de los botones según el rol del jugador
    /// </summary>
    private void UpdateButtonVisibility()
    {
        bool isMaster = PhotonNetwork.IsMasterClient;

        // Botón JUGAR: Solo visible para el creador
        if (playButton != null)
        {
            playButton.gameObject.SetActive(isMaster);

            // Solo se puede jugar si hay suficientes jugadores
            if (isMaster)
            {
                bool canStart = PhotonNetwork.CurrentRoom.PlayerCount >= minPlayersToStart;
                playButton.interactable = canStart;
            }
        }

        // Botón SALIR: Visible para todos
        if (leaveButton != null)
        {
            leaveButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Maneja el click en el botón JUGAR (solo host)
    /// </summary>
    private void OnPlayButtonClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Solo el creador puede iniciar el juego");
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < minPlayersToStart)
        {
            Debug.LogWarning($"Se necesitan al menos {minPlayersToStart} jugadores");
            ShowNotification($"Se necesitan al menos {minPlayersToStart} jugadores para iniciar");
            return;
        }

        // Guardar configuración de economía antes de iniciar
        SaveEconomySettings();

        // Cerrar la sala para nuevos jugadores
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // Cambiar estado del juego
        Hashtable gameStateProps = new Hashtable { { "EstadoJuego", "EnJuego" } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(gameStateProps);

        Debug.Log("Iniciando juego...");

        // Cargar escena de juego para todos
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    /// <summary>
    /// Maneja el click en el botón SALIR
    /// </summary>
    private void OnLeaveButtonClicked()
    {
        Debug.Log("Saliendo de la sala...");
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Maneja el click en el botón ACTUALIZAR
    /// </summary>
    private void OnUpdateButtonClicked()
    {
        UpdatePlayerList();
        UpdateRoomInfo();
        ShowNotification("Lista actualizada");
    }

    // Callbacks de Photon

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Jugador entró: {newPlayer.NickName}");
        UpdatePlayerList();
        UpdateRoomInfo();
        UpdateButtonVisibility();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Jugador salió: {otherPlayer.NickName}");
        UpdatePlayerList();
        UpdateRoomInfo();
        UpdateButtonVisibility();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Nuevo host: {newMasterClient.NickName}");
        UpdateButtonVisibility();
        SetupEconomyPanel();
        UpdatePlayerList();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Saliste de la sala");
        SceneManager.LoadScene("LobbyScene"); // Ajusta el nombre de tu escena de lobby
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // Si cambió el estado del juego
        if (propertiesThatChanged.ContainsKey("EstadoJuego"))
        {
            string estado = propertiesThatChanged["EstadoJuego"].ToString();
            Debug.Log($"Estado del juego cambió a: {estado}");
        }
    }

    /// <summary>
    /// Muestra una notificación
    /// </summary>
    private void ShowNotification(string message)
    {
        Debug.Log($"[NOTIFICACIÓN] {message}");
        // Implementar sistema de notificaciones en UI
    }
}