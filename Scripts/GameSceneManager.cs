using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Gestiona la escena del juego completa:
/// - Muestra BankPanel o PlayerPanel según el rol
/// - Panel de transferencias con jugadores
/// - Mi Dinero, Lotería, Timers
/// </summary>
public class GameSceneManager : MonoBehaviourPunCallbacks
{
    [Header("Panels Principales")]
    [SerializeField] private GameObject bankPanel;
    [SerializeField] private GameObject playerPanel;

    [Header("UI - Panel de Transferencias")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerListItemPrefab;

    [Header("UI - Dinero y Lotería")]
    [SerializeField] private TMP_Text myMoneyText;
    [SerializeField] private TMP_Text lotteryText;

    [Header("UI - Timers (Solo Banco)")]
    [SerializeField] private TMP_Text auctionTaxTimerText;

    [Header("Configuración de Timers (Solo Banco)")]
    [Tooltip("Tiempo en minutos para la subasta")]
    [SerializeField] private float auctionIntervalMinutes = 5f;
    [Tooltip("Tiempo en minutos para los impuestos")]
    [SerializeField] private float taxIntervalMinutes = 10f;

    [Header("Configuración Custom")]
    public List<string> CasualityList = new List<string>();
    public List<string> CommunalList = new List<string>();

    // Estado privado
    private Dictionary<string, GameObject> playerListItems = new Dictionary<string, GameObject>();
    private double lotteryPool = 0;
    private float auctionTimeRemaining;
    private float taxTimeRemaining;
    private bool isBankPlayer = false;

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("[GameSceneManager] No estamos en una sala");
            return;
        }

        // Verificar si este jugador es el banco (por nombre)
        isBankPlayer = PhotonNetwork.IsMasterClient;

        // Configurar paneles según rol
        SetupPanels();

        // Inicializar timers
        auctionTimeRemaining = auctionIntervalMinutes * 60f;
        taxTimeRemaining = taxIntervalMinutes * 60f;

        // Configurar visibilidad de timers (solo banco)
        if (auctionTaxTimerText != null)
        {
            auctionTaxTimerText.gameObject.SetActive(isBankPlayer);
        }

        // Asignar el banco automáticamente al primer jugador
        if (PhotonNetwork.IsMasterClient && BankManager.Instance != null)
        {
            BankManager.Instance.SetFirstPlayerAsBank();
        }

        // Cargar lotería desde sala
        LoadLotteryFromRoom();

        //Inicializar el sistema del dinero.
        InitializePlayerMoney();

        // Actualizar UI
        UpdatePlayerList();
        UpdateMyMoney();
        UpdateLottery();

        Debug.Log($"[GameSceneManager] Inicializado. Es Banco: {isBankPlayer}, Es Master: {PhotonNetwork.IsMasterClient}");
    }

    private void Update()
    {
        // Solo el banco actualiza timers (y debe ser MasterClient para evitar duplicados)
        if (isBankPlayer && PhotonNetwork.IsMasterClient)
        {
            UpdateTimers();
        }
    }

    #region PANELES

    /// <summary>
    /// Configura qué panel mostrar según el rol
    /// </summary>
    private void SetupPanels()
    {
        // El banco siempre usa bankPanel
        // Los demás usan playerPanel
        if (bankPanel != null)
            bankPanel.SetActive(isBankPlayer);

        if (playerPanel != null)
            playerPanel.SetActive(!isBankPlayer);

        Debug.Log($"[GameSceneManager] Panel activo: {(isBankPlayer ? "Bank" : "Player")}");
    }

    #endregion

    #region PANEL DE TRANSFERENCIAS

    /// <summary>
    /// Actualiza la lista de jugadores en el panel de transferencias
    /// </summary>
    private void UpdatePlayerList()
    {
        if (playerListContainer == null)
        {
            Debug.LogWarning("[GameSceneManager] PlayerListContainer no asignado");
            return;
        }

        // Limpiar lista actual
        foreach (var item in playerListItems.Values)
        {
            if (item != null)
                Destroy(item);
        }
        playerListItems.Clear();

        // Agregar todos los jugadores
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            CreatePlayerListItem(player);
        }

        // Agregar lotería como "jugador" especial
        CreateLotteryListItem();

        Debug.Log($"[GameSceneManager] Lista de transferencias actualizada. Total: {PhotonNetwork.PlayerList.Length + 1}");
    }

    /// <summary>
    /// Crea un item en la lista para un jugador
    /// </summary>
    private void CreatePlayerListItem(Player player)
    {
        if (playerListItemPrefab == null || playerListContainer == null)
        {
            Debug.LogError("[GameSceneManager] PlayerListItemPrefab o Container es null");
            return;
        }

        GameObject itemObj = Instantiate(playerListItemPrefab, playerListContainer);
        PlayerListItem item = itemObj.GetComponent<PlayerListItem>();

        if (item != null)
        {
            // Obtener dinero del jugador
            double playerMoney = GetPlayerMoney(player);

            // Solo el banco puede ver el dinero de otros jugadores
            bool showMoney = isBankPlayer || player.IsLocal;

            item.Setup(player.NickName, playerMoney, showMoney, player);
        }

        playerListItems[player.UserId] = itemObj;
    }

    private void CreateLotteryListItem()
    {
        if (playerListItemPrefab == null || playerListContainer == null)
            return;

        GameObject itemObj = Instantiate(playerListItemPrefab, playerListContainer);
        PlayerListItem item = itemObj.GetComponent<PlayerListItem>();

        if (item != null)
        {
            item.Setup("Lotería / Impuestos", lotteryPool, true, null);
        }

        playerListItems["LOTTERY"] = itemObj;
    }

    /// <summary>
    /// Obtiene el dinero de un jugador desde CustomProperties
    /// </summary>
    private double GetPlayerMoney(Player player)
    {
        if (player.CustomProperties.ContainsKey("Money"))
        {
            return (double)player.CustomProperties["Money"];
        }
        return 0;
    }

    #endregion

    #region MI DINERO

    /// <summary>
    /// Actualiza el texto de "Mi Dinero"
    /// </summary>
    private void UpdateMyMoney()
    {
        if (myMoneyText == null)
            return;

        double myMoney = GetPlayerMoney(PhotonNetwork.LocalPlayer);
        myMoneyText.text = $"Mi Dinero\n${myMoney:N0}";
    }

    /// <summary>
    /// Inicializa el dinero de todos los jugadores según la inflación
    /// </summary>
    private void InitializePlayerMoney()
    {
        // Solo el MasterClient inicializa el dinero
        if (!PhotonNetwork.IsMasterClient)
            return;

        // Obtener configuración de economía de la sala
        int denominacionMinima = 1;
        int valorMediterraneo = 60; // Valor original por defecto

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("DenominacionMinima"))
        {
            denominacionMinima = (int)PhotonNetwork.CurrentRoom.CustomProperties["DenominacionMinima"];
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("ValorMediterraneo"))
        {
            valorMediterraneo = (int)PhotonNetwork.CurrentRoom.CustomProperties["ValorMediterraneo"];
        }

        // Calcular inflación
        // En el monopolio original: propiedad más baja = $60, dinero inicial = $1,500
        // Fórmula: dineroInicial = (1500 * valorMediterraneo) / 60
        double multiplicador = (double)valorMediterraneo / 60.0;
        double dineroInicial = 1500.0 * multiplicador;

        // Redondear a la denominación mínima
        dineroInicial = System.Math.Round(dineroInicial / denominacionMinima) * denominacionMinima;

        Debug.Log($"[GameSceneManager] Inicializando dinero. Mediterráneo: ${valorMediterraneo}, Multiplicador: {multiplicador}x, Dinero Inicial: ${dineroInicial:N0}");

        // Asignar dinero a todos los jugadores
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Hashtable props = new Hashtable
        {
            { "Money", dineroInicial }
        };
            player.SetCustomProperties(props);
        }
    }
    #endregion

    #region LOTERÍA

    /// <summary>
    /// Carga el pozo de lotería desde las propiedades de la sala
    /// </summary>
    private void LoadLotteryFromRoom()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("LotteryPool"))
        {
            lotteryPool = (double)PhotonNetwork.CurrentRoom.CustomProperties["LotteryPool"];
        }
        else
        {
            lotteryPool = 0;
        }
    }

    /// <summary>
    /// Actualiza el texto de lotería
    /// </summary>
    private void UpdateLottery()
    {
        if (lotteryText == null)
            return;

        lotteryText.text = $"Lotería\n${lotteryPool:N0}";
    }

    /// <summary>
    /// Agrega dinero al pozo de lotería (sincronizado)
    /// Solo el MasterClient puede modificarlo
    /// </summary>
    public void AddToLottery(double amount)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[GameSceneManager] Solo el MasterClient puede modificar la lotería");
            return;
        }

        lotteryPool += amount;

        // Sincronizar con todos los jugadores
        Hashtable props = new Hashtable
        {
            { "LotteryPool", lotteryPool }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        UpdateLottery();
        UpdatePlayerList();

        Debug.Log($"[GameSceneManager] ${amount:N0} agregado a lotería. Total: ${lotteryPool:N0}");
    }

    /// <summary>
    /// Retira todo el dinero de la lotería
    /// Solo el MasterClient puede hacerlo
    /// </summary>
    public double WithdrawLottery()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[GameSceneManager] Solo el MasterClient puede retirar de lotería");
            return 0;
        }

        double amount = lotteryPool;
        lotteryPool = 0;

        // Sincronizar
        Hashtable props = new Hashtable
        {
            { "LotteryPool", lotteryPool }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        UpdateLottery();
        UpdatePlayerList();

        Debug.Log($"[GameSceneManager] Lotería retirada: ${amount:N0}");
        return amount;
    }

    #endregion

    #region TIMERS (SUBASTA E IMPUESTOS)

    /// <summary>
    /// Actualiza los timers de subasta e impuestos
    /// Solo se ejecuta en el banco
    /// </summary>
    private void UpdateTimers()
    {
        // Timer de subasta
        auctionTimeRemaining -= Time.deltaTime;
        if (auctionTimeRemaining <= 0)
        {
            OnAuctionTriggered();
            auctionTimeRemaining = auctionIntervalMinutes * 60f; // Reiniciar
        }

        // Timer de impuestos
        taxTimeRemaining -= Time.deltaTime;
        if (taxTimeRemaining <= 0)
        {
            OnTaxTriggered();
            taxTimeRemaining = taxIntervalMinutes * 60f; // Reiniciar
        }

        // Actualizar texto
        if (auctionTaxTimerText != null)
        {
            string auctionTime = FormatTime(auctionTimeRemaining);
            string taxTime = FormatTime(taxTimeRemaining);
            auctionTaxTimerText.text = $"Subasta: {auctionTime}\nImpuestos: {taxTime}";
        }
    }

    /// <summary>
    /// Formatea segundos a MM:SS
    /// </summary>
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }

    /// <summary>
    /// Se ejecuta cuando el timer de subasta llega a 0
    /// </summary>
    private void OnAuctionTriggered()
    {
        Debug.Log("[GameSceneManager] ¡Tiempo de SUBASTA!");
        // TODO: Implementar lógica de subasta
    }

    /// <summary>
    /// Se ejecuta cuando el timer de impuestos llega a 0
    /// </summary>
    private void OnTaxTriggered()
    {
        Debug.Log("[GameSceneManager] ¡Tiempo de IMPUESTOS!");
        // TODO: Cobrar impuestos y agregarlos a lotería
    }

    #endregion

    #region CALLBACKS DE PHOTON

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"[GameSceneManager] Nuevo MasterClient: {newMasterClient.NickName}");

        // Reconfigurar paneles (por si el banco se desconecta)
        SetupPanels();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[GameSceneManager] Jugador entró: {newPlayer.NickName}");
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[GameSceneManager] Jugador salió: {otherPlayer.NickName}");
        UpdatePlayerList();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // Si cambió el dinero de algún jugador
        if (changedProps.ContainsKey("Money"))
        {
            UpdatePlayerList();

            // Si es el jugador local, actualizar "Mi Dinero"
            if (targetPlayer.IsLocal)
            {
                UpdateMyMoney();
            }
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // Si cambió el pozo de lotería
        if (propertiesThatChanged.ContainsKey("LotteryPool"))
        {
            lotteryPool = (double)propertiesThatChanged["LotteryPool"];
            UpdateLottery();
            UpdatePlayerList(); // Actualizar item de lotería en la lista
        }
    }

    #endregion

    #region MÉTODOS PÚBLICOS

    /// <summary>
    /// Fuerza actualización manual de todas las listas
    /// </summary>
    public void ForceRefreshAll()
    {
        UpdatePlayerList();
        UpdateMyMoney();
        UpdateLottery();
    }

    /// <summary>
    /// Obtiene si el jugador actual es el banco
    /// </summary>
    public bool IsBank()
    {
        return isBankPlayer;
    }

    #endregion
}