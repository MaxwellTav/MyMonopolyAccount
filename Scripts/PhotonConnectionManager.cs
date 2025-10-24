using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Gestiona la conexión a Photon con configuración optimizada para Web
/// </summary>
public class PhotonConnectionManager : MonoBehaviourPunCallbacks
{
    public static PhotonConnectionManager Instance { get; private set; }

    [Header("Configuración de Conexión")]
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private byte maxPlayersPerRoom = 15;

    private bool isConnecting = false;

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

        ConfigurePhotonSettings();
    }

    private void Start()
    {
        ConnectToPhoton();
    }

    /// <summary>
    /// Configura Photon para óptimo rendimiento en Web
    /// </summary>
    private void ConfigurePhotonSettings()
    {
        // Configuración optimizada para Web y conexiones lentas
        PhotonNetwork.SendRate = 20; // Envíos por segundo (reducido para Web)
        PhotonNetwork.SerializationRate = 10; // Sincronización por segundo

        // Configuración de desconexión - IMPORTANTE para evitar timeouts
        PhotonNetwork.KeepAliveInBackground = 999999; // Casi infinito
        PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = 0.0001f;

        // Configuración de timeouts extendidos
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 999999;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.SentCountAllowance = 99999;

        // Configuración de juego
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.AutomaticallySyncScene = true; // Sincronizar escenas automáticamente
    }

    /// <summary>
    /// Conecta a Photon Cloud
    /// </summary>
    public void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Ya conectado a Photon");
            return;
        }

        if (isConnecting)
        {
            Debug.Log("Ya estamos conectando...");
            return;
        }

        isConnecting = true;
        Debug.Log("Conectando a Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado al servidor Master de Photon");
        isConnecting = false;

        // Unirse automáticamente al lobby
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Unido al Lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Desconectado de Photon: {cause}");
        isConnecting = false;

        // Intentar reconectar automáticamente
        StartCoroutine(TryReconnect());
    }

    private IEnumerator TryReconnect()
    {
        yield return new WaitForSeconds(2f);

        if (!PhotonNetwork.IsConnected && !isConnecting)
        {
            Debug.Log("Intentando reconectar...");
            ConnectToPhoton();
        }
    }

    public override void OnConnected()
    {
        Debug.Log("Conexión establecida con éxito");
    }

    public byte GetMaxPlayersPerRoom()
    {
        return maxPlayersPerRoom;
    }
}