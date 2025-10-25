using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// Gestiona el rol del banco en el juego
/// El banco puede tener dinero negativo
/// </summary>
public class BankManager : MonoBehaviourPunCallbacks
{
    public static BankManager Instance { get; private set; }

    private const string BANK_PLAYER_KEY = "IsBank";

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
        }
    }

    #region ASIGNACIÓN DEL BANCO

    /// <summary>
    /// Asigna el rol de banco a un jugador específico
    /// Solo el MasterClient puede hacer esto
    /// </summary>
    public void SetBankPlayer(Player player)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[BankManager] Solo el MasterClient puede asignar el banco");
            return;
        }

        // Primero, remover el rol de banco de todos los jugadores
        RemoveAllBankRoles();

        // Asignar el rol de banco al jugador seleccionado
        Hashtable props = new Hashtable
        {
            { BANK_PLAYER_KEY, true }
        };
        player.SetCustomProperties(props);

        Debug.Log($"[BankManager] {player.NickName} es ahora el BANCO");
    }

    /// <summary>
    /// Asigna automáticamente el rol de banco al primer jugador (por defecto el host)
    /// </summary>
    public void SetFirstPlayerAsBank()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Player[] players = PhotonNetwork.PlayerList;
        if (players.Length > 0)
        {
            SetBankPlayer(players[0]);
        }
    }

    /// <summary>
    /// Remueve el rol de banco de todos los jugadores
    /// </summary>
    private void RemoveAllBankRoles()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey(BANK_PLAYER_KEY))
            {
                Hashtable props = new Hashtable
                {
                    { BANK_PLAYER_KEY, false }
                };
                player.SetCustomProperties(props);
            }
        }
    }

    #endregion

    #region VERIFICACIÓN DE BANCO

    /// <summary>
    /// Verifica si un jugador es el banco
    /// </summary>
    public bool IsBankPlayer(Player player)
    {
        if (player == null)
            return false;

        if (player.CustomProperties.ContainsKey(BANK_PLAYER_KEY))
        {
            return (bool)player.CustomProperties[BANK_PLAYER_KEY];
        }

        return false;
    }

    /// <summary>
    /// Verifica si el jugador local es el banco
    /// </summary>
    public bool IsLocalPlayerBank()
    {
        return IsBankPlayer(PhotonNetwork.LocalPlayer);
    }

    /// <summary>
    /// Obtiene el jugador que es el banco (si existe)
    /// </summary>
    public Player GetBankPlayer()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (IsBankPlayer(player))
            {
                return player;
            }
        }

        return null;
    }

    #endregion

    #region CALLBACKS DE PHOTON

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Si no hay banco asignado y soy el master, asignar el banco
        if (PhotonNetwork.IsMasterClient && GetBankPlayer() == null)
        {
            SetFirstPlayerAsBank();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Si el banco se fue, asignar un nuevo banco
        if (IsBankPlayer(otherPlayer) && PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"[BankManager] El banco ({otherPlayer.NickName}) se fue. Asignando nuevo banco...");
            SetFirstPlayerAsBank();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Si el master cambia y no hay banco, asignar uno
        if (PhotonNetwork.IsMasterClient && GetBankPlayer() == null)
        {
            SetFirstPlayerAsBank();
        }
    }

    #endregion

    #region MÉTODOS AUXILIARES PARA UI

    /// <summary>
    /// Obtiene el nombre del jugador banco (para mostrar en UI)
    /// </summary>
    public string GetBankPlayerName()
    {
        Player bankPlayer = GetBankPlayer();
        return bankPlayer != null ? bankPlayer.NickName : "Sin Banco";
    }

    /// <summary>
    /// Obtiene el dinero del banco
    /// </summary>
    public double GetBankMoney()
    {
        Player bankPlayer = GetBankPlayer();
        if (bankPlayer != null && bankPlayer.CustomProperties.ContainsKey("Money"))
        {
            return (double)bankPlayer.CustomProperties["Money"];
        }
        return 0;
    }

    #endregion
}