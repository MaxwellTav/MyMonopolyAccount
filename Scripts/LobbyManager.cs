using ExitGames.Client.Photon;
using Lean.Gui;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gestiona el lobby principal: crear salas, listar salas disponibles
/// </summary>
public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI - Crear Sala")]
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private LeanButton createRoomButton;
    [SerializeField] private LeanButton refreshButton;

    [Header("UI - Lista de Salas")]
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomItemPrefab;

    [Header("Configuración")]
    [SerializeField] private string defaultRoomPrefix = "Sala";
    [SerializeField] private int maxPlayersPerRoom = 15;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private Dictionary<string, GameObject> roomListItems = new Dictionary<string, GameObject>();

    private void Start()
    {
        // Configurar botones
        if (createRoomButton != null)
            createRoomButton.OnClick.AddListener(OnCreateRoomButtonClicked);

        if (refreshButton != null)
            refreshButton.OnClick.AddListener(OnRefreshButtonClicked);

        // Esperar a estar conectado al lobby
        if (PhotonNetwork.InLobby)
        {
            OnJoinedLobby();
        }
    }

    /// <summary>
    /// Crear una nueva sala
    /// </summary>
    public void OnCreateRoomButtonClicked()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("No conectado a Photon");
            ShowNotification("No conectado al servidor. Intentando reconectar...");
            return;
        }

        string roomName = roomNameInput != null ? roomNameInput.text.Trim() : "";

        // Si no hay nombre, generar uno automático
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"{defaultRoomPrefix}{Random.Range(1000, 9999)}";
        }

        // Configurar opciones de la sala
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true,

            // Propiedades personalizadas de la sala para economía
            CustomRoomProperties = new Hashtable
            {
                { "DenominacionMinima", 1 }, // Valor por defecto
                { "ValorMediterraneo", 60 }, // Valor por defecto del original
                { "EstadoJuego", "Esperando" } // Esperando, EnJuego, Finalizado
            },

            // Propiedades visibles en el lobby
            CustomRoomPropertiesForLobby = new string[]
            {
                "DenominacionMinima",
                "ValorMediterraneo",
                "EstadoJuego"
            },

            // Limpiar jugadores cuando se desconectan (después de un tiempo)
            PlayerTtl = 999999, // Tiempo que se mantiene el slot del jugador (casi infinito)
            EmptyRoomTtl = 0, // Sala se destruye inmediatamente si está vacía

            // Permitir que los jugadores puedan volver a unirse
            PublishUserId = true
        };

        Debug.Log($"Creando sala: {roomName}");
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"Sala creada exitosamente: {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Error al crear sala: {message}");

        // Si el nombre ya existe, intentar con otro
        if (returnCode == ErrorCode.GameIdAlreadyExists)
        {
            string newRoomName = $"{defaultRoomPrefix}{Random.Range(1000, 9999)}";
            ShowNotification($"Ese nombre ya existe. Creando: {newRoomName}");

            if (roomNameInput != null)
                roomNameInput.text = newRoomName;

            OnCreateRoomButtonClicked();
        }
        else
        {
            ShowNotification($"Error al crear sala: {message}");
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Unido a sala: {PhotonNetwork.CurrentRoom.Name}");

        SceneManager.LoadScene("RoomScene");
    }

    /// <summary>
    /// Unirse a una sala existente
    /// </summary>
    public void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("No conectado a Photon");
            ShowNotification("No conectado al servidor");
            return;
        }

        Debug.Log($"Intentando unirse a: {roomName}");
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Error al unirse a sala: {message}");
        ShowNotification($"No se pudo unir a la sala: {message}");
    }

    /// <summary>
    /// Actualizar lista de salas
    /// </summary>
    public void OnRefreshButtonClicked()
    {
        Debug.Log("Actualizando lista de salas...");

        // Photon actualiza automáticamente, pero podemos forzar
        if (PhotonNetwork.InLobby)
        {
            // La lista se actualizará automáticamente con OnRoomListUpdate
            ShowNotification("Lista actualizada");
        }
        else
        {
            ShowNotification("Reconectando al lobby...");
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("En el lobby, listo para ver salas");
        cachedRoomList.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"Actualización de lista de salas: {roomList.Count} cambios");

        // Actualizar cache de salas
        foreach (RoomInfo roomInfo in roomList)
        {
            // Eliminar salas cerradas o que ya no existen
            if (!roomInfo.IsOpen || !roomInfo.IsVisible || roomInfo.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(roomInfo.Name))
                {
                    cachedRoomList.Remove(roomInfo.Name);
                }
            }
            else
            {
                // Actualizar o agregar sala
                cachedRoomList[roomInfo.Name] = roomInfo;
            }
        }

        // Actualizar UI
        UpdateRoomListUI();
    }

    /// <summary>
    /// Actualiza la UI con la lista de salas
    /// </summary>
    private void UpdateRoomListUI()
    {
        // Limpiar items anteriores
        foreach (var item in roomListItems.Values)
        {
            if (item != null)
                Destroy(item);
        }
        roomListItems.Clear();

        // Crear items para cada sala
        foreach (var roomInfo in cachedRoomList.Values)
        {
            if (roomItemPrefab != null && roomListContainer != null)
            {
                GameObject roomItem = Instantiate(roomItemPrefab, roomListContainer);
                RoomItem roomItemScript = roomItem.GetComponent<RoomItem>();

                if (roomItemScript != null)
                {
                    roomItemScript.Setup(roomInfo, this);
                }

                roomListItems[roomInfo.Name] = roomItem;
            }
        }

        Debug.Log($"Lista actualizada: {cachedRoomList.Count} salas disponibles");
    }

    /// <summary>
    /// Muestra una notificación al jugador
    /// </summary>
    private void ShowNotification(string message)
    {
        Debug.Log($"[NOTIFICACIÓN] {message}");
        // Aquí puedes implementar un sistema de UI para notificaciones
        // Ejemplo: notificationText.text = message;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Desconectado: {cause}");
        cachedRoomList.Clear();
        UpdateRoomListUI();
    }
}