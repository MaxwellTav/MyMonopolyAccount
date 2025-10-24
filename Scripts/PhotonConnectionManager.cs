using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Gestiona la conexi�n a Photon con configuraci�n optimizada para Web
/// </summary>
public class PhotonConnectionManager : MonoBehaviourPunCallbacks
{
    public static PhotonConnectionManager Instance { get; private set; }

    [Header("Configuraci�n de Conexi�n")]
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
    /// Configura Photon para �ptimo rendimiento en Web
    /// </summary>
    private void ConfigurePhotonSettings()
    {
        // Configuraci�n optimizada para Web y conexiones lentas
        PhotonNetwork.SendRate = 20; // Env�os por segundo (reducido para Web)
        PhotonNetwork.SerializationRate = 10; // Sincronizaci�n por segundo

        // Configuraci�n de desconexi�n - IMPORTANTE para evitar timeouts
        PhotonNetwork.KeepAliveInBackground = 999999; // Casi infinito
        PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = 0.0001f;

        // Configuraci�n de timeouts extendidos
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 999999;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.SentCountAllowance = 99999;

        // Configuraci�n de juego
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.AutomaticallySyncScene = true; // Sincronizar escenas autom�ticamente
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

        // Unirse autom�ticamente al lobby
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

        // Intentar reconectar autom�ticamente
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
        Debug.Log("Conexi�n establecida con �xito");
    }

    public byte GetMaxPlayersPerRoom()
    {
        return maxPlayersPerRoom;
    }
}