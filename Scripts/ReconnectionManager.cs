using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Sistema de reconexión automática para jugadores que se desconectan
/// Permite que los jugadores vuelvan a su partida con su estado intacto
/// </summary>
public class ReconnectionManager : MonoBehaviourPunCallbacks
{
    public static ReconnectionManager Instance { get; private set; }

    [Header("Configuración de Reconexión")]
    [SerializeField] private float reconnectDelay = 2f;
    [SerializeField] private int maxReconnectAttempts = 5;
    [SerializeField] private bool autoReconnect = true;

    [Header("UI (Opcional)")]
    [SerializeField] private GameObject reconnectingPanel;

    private string lastRoomName = "";
    private int reconnectAttempts = 0;
    private bool isReconnecting = false;

    // Guardar estado del jugador
    private PlayerGameState savedPlayerState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Verificar si hay una reconexión pendiente al iniciar
        CheckPendingReconnection();
    }

    /// <summary>
    /// Guarda el nombre de la sala actual para poder reconectar
    /// </summary>
    public void SaveCurrentRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            lastRoomName = PhotonNetwork.CurrentRoom.Name;
            PlayerPrefs.SetString("LastRoomName", lastRoomName);
            PlayerPrefs.Save();
            Debug.Log($"Sala guardada para reconexión: {lastRoomName}");
        }
    }

    /// <summary>
    /// Guarda el estado del jugador antes de desconectar
    /// </summary>
    public void SavePlayerState(PlayerGameState state)
    {
        savedPlayerState = state;

        // Guardar también en Photon Custom Properties para persistencia
        if (PhotonNetwork.LocalPlayer != null)
        {
            Hashtable props = new Hashtable
            {
                { "Money", state.money },
                { "Position", state.position },
                { "InJail", state.inJail },
                { "LastUpdate", PhotonNetwork.ServerTimestamp }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }

    /// <summary>
    /// Verifica si hay una reconexión pendiente al iniciar
    /// </summary>
    private void CheckPendingReconnection()
    {
        string savedRoom = PlayerPrefs.GetString("LastRoomName", "");

        if (!string.IsNullOrEmpty(savedRoom) && !PhotonNetwork.InRoom)
        {
            Debug.Log($"Reconexión pendiente detectada: {savedRoom}");

            if (autoReconnect && PhotonNetwork.IsConnectedAndReady)
            {
                AttemptReconnect(savedRoom);
            }
        }
    }

    /// <summary>
    /// Intenta reconectar a la última sala
    /// </summary>
    public void AttemptReconnect(string roomName = null)
    {
        if (isReconnecting)
        {
            Debug.Log("Ya hay un intento de reconexión en curso");
            return;
        }

        string targetRoom = roomName ?? lastRoomName;

        if (string.IsNullOrEmpty(targetRoom))
        {
            Debug.LogWarning("No hay sala para reconectar");
            return;
        }

        isReconnecting = true;
        ShowReconnectingUI(true);

        Debug.Log($"Intentando reconectar a: {targetRoom}");

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            // Primero conectar a Photon
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            // Intentar rejoin directamente
            StartCoroutine(TryRejoinRoom(targetRoom));
        }
    }

    /// <summary>
    /// Intenta volver a unirse a la sala
    /// </summary>
    private IEnumerator TryRejoinRoom(string roomName)
    {
        yield return new WaitForSeconds(reconnectDelay);

        if (reconnectAttempts >= maxReconnectAttempts)
        {
            Debug.LogError("Máximo de intentos de reconexión alcanzado");
            HandleReconnectionFailed();
            yield break;
        }

        reconnectAttempts++;
        Debug.Log($"Intento de reconexión {reconnectAttempts}/{maxReconnectAttempts}");

        // Intentar rejoin si el jugador ya estaba en la sala
        if (PhotonNetwork.ReconnectAndRejoin())
        {
            Debug.Log("Usando ReconnectAndRejoin...");
        }
        else
        {
            // Si no funciona, intentar join normal
            Debug.Log("ReconnectAndRejoin falló, intentando rejoin normal...");
            PhotonNetwork.RejoinRoom(roomName);
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Reconectado al Master Server");

        if (isReconnecting && !string.IsNullOrEmpty(lastRoomName))
        {
            StartCoroutine(TryRejoinRoom(lastRoomName));
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Reconexión exitosa a: {PhotonNetwork.CurrentRoom.Name}");

        isReconnecting = false;
        reconnectAttempts = 0;
        ShowReconnectingUI(false);

        // Restaurar estado del jugador
        RestorePlayerState();

        // Cargar escena de juego si no estamos en ella
        if (SceneManager.GetActiveScene().name != "GameScene")
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Fallo al reconectar: {message}");

        // Intentar de nuevo si no se alcanzó el máximo
        if (reconnectAttempts < maxReconnectAttempts)
        {
            StartCoroutine(TryRejoinRoom(lastRoomName));
        }
        else
        {
            HandleReconnectionFailed();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Desconectado: {cause}");

        // No intentar reconectar si fue intencional
        if (cause == DisconnectCause.DisconnectByClientLogic)
        {
            return;
        }

        // Si estábamos en una partida, intentar reconectar
        if (!string.IsNullOrEmpty(lastRoomName) && autoReconnect)
        {
            StartCoroutine(DelayedReconnect());
        }
    }

    private IEnumerator DelayedReconnect()
    {
        yield return new WaitForSeconds(reconnectDelay);
        AttemptReconnect();
    }

    /// <summary>
    /// Restaura el estado del jugador desde Photon Custom Properties
    /// </summary>
    private void RestorePlayerState()
    {
        if (PhotonNetwork.LocalPlayer == null)
            return;

        var props = PhotonNetwork.LocalPlayer.CustomProperties;

        if (props.ContainsKey("Money"))
        {
            int money = (int)props["Money"];
            int position = props.ContainsKey("Position") ? (int)props["Position"] : 0;
            bool inJail = props.ContainsKey("InJail") && (bool)props["InJail"];

            Debug.Log($"Estado restaurado - Dinero: ${money}, Posición: {position}");

            // Aquí aplicarías estos valores al estado del jugador en el juego
            // Ejemplo: PlayerController.Instance.SetMoney(money);
            // Ejemplo: PlayerController.Instance.SetPosition(position);
            // Ejemplo: PlayerController.Instance.SetJailStatus(inJail);
        }
    }

    /// <summary>
    /// Maneja el fallo total de reconexión
    /// </summary>
    private void HandleReconnectionFailed()
    {
        Debug.LogError("Reconexión fallida completamente");

        isReconnecting = false;
        reconnectAttempts = 0;
        ShowReconnectingUI(false);

        // Limpiar datos
        PlayerPrefs.DeleteKey("LastRoomName");
        lastRoomName = "";

        // Volver al lobby
        if (SceneManager.GetActiveScene().name != "LobbyScene")
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }

    /// <summary>
    /// Muestra u oculta el UI de reconexión
    /// </summary>
    private void ShowReconnectingUI(bool show)
    {
        if (reconnectingPanel != null)
        {
            reconnectingPanel.SetActive(show);
        }
    }

    /// <summary>
    /// Limpia los datos de reconexión (llamar al salir intencionalmente)
    /// </summary>
    public void ClearReconnectionData()
    {
        PlayerPrefs.DeleteKey("LastRoomName");
        lastRoomName = "";
        savedPlayerState = null;
        Debug.Log("Datos de reconexión limpiados");
    }

    private void OnApplicationQuit()
    {
        // Guardar datos para reconexión si estamos en partida
        if (PhotonNetwork.InRoom)
        {
            SaveCurrentRoom();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        // En Web, esto puede ayudar con la reconexión
        if (pause && PhotonNetwork.InRoom)
        {
            SaveCurrentRoom();
        }
    }
}

/// <summary>
/// Clase para almacenar el estado del jugador
/// </summary>
[System.Serializable]
public class PlayerGameState
{
    public int money;
    public int position;
    public bool inJail;
    public int jailTurns;
    public bool[] ownedProperties;

    public PlayerGameState(int money, int position, bool inJail = false)
    {
        this.money = money;
        this.position = position;
        this.inJail = inJail;
        this.jailTurns = 0;
    }
}