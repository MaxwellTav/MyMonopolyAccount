using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using ExitGames.Client.Photon;
using Lean.Gui;

/// <summary>
/// Representa un item de sala en la lista del lobby
/// Compatible con LeanButton
/// </summary>
public class RoomItem : MonoBehaviour
{
    [Header("UI Referencias")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text statusText;

    // IMPORTANTE: LeanButton en lugar de Button
    private LeanButton leanButton;

    private RoomInfo roomInfo;
    private LobbyManager lobbyManager;

    private void Awake()
    {
        // Obtener el LeanButton del GameObject principal
        leanButton = GetComponent<LeanButton>();

        if (leanButton == null)
        {
            Debug.LogError("[RoomItem] No se encontró LeanButton en el GameObject");
        }

        // Auto-buscar textos si no están asignados
        if (roomNameText == null)
        {
            roomNameText = transform.Find("RoomNameText")?.GetComponent<TMP_Text>();
        }

        if (playerCountText == null)
        {
            playerCountText = transform.Find("PlayerCountText")?.GetComponent<TMP_Text>();
        }

        if (statusText == null)
        {
            statusText = transform.Find("StatusText")?.GetComponent<TMP_Text>();
        }
    }

    /// <summary>
    /// Configura el item con la información de la sala
    /// </summary>
    public void Setup(RoomInfo info, LobbyManager lobby)
    {
        roomInfo = info;
        lobbyManager = lobby;

        UpdateUI();

        // Configurar LeanButton
        if (leanButton != null)
        {
            // Limpiar listeners previos
            leanButton.OnClick.RemoveAllListeners();

            // Asignar el evento OnClick
            leanButton.OnClick.AddListener(OnJoinButtonClicked);

            Debug.Log($"[RoomItem] Botón configurado para sala: {info.Name}");
        }
        else
        {
            Debug.LogError("[RoomItem] LeanButton es null, no se puede configurar el evento");
        }
    }

    /// <summary>
    /// Actualiza la UI del item
    /// </summary>
    private void UpdateUI()
    {
        if (roomInfo == null)
            return;

        // Nombre de la sala
        if (roomNameText != null)
        {
            roomNameText.text = roomInfo.Name;
        }

        // Conteo de jugadores: "1/15"
        if (playerCountText != null)
        {
            playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        }

        // Estado de la sala
        if (statusText != null)
        {
            string estado = GetRoomStatus();
            statusText.text = estado;

            // Cambiar color según el estado
            if (estado == "CERRADO" || estado == "EN JUEGO")
            {
                statusText.color = Color.red;
            }
            else if (estado == "LLENO")
            {
                statusText.color = new Color(1f, 0.5f, 0f); // Naranja
            }
            else
            {
                statusText.color = Color.green;
            }
        }

        // Habilitar/deshabilitar LeanButton según disponibilidad
        if (leanButton != null)
        {
            bool canJoin = roomInfo.IsOpen &&
                          roomInfo.PlayerCount < roomInfo.MaxPlayers;

            leanButton.interactable = canJoin;
        }
    }

    /// <summary>
    /// Obtiene el estado de la sala
    /// </summary>
    private string GetRoomStatus()
    {
        if (!roomInfo.IsOpen)
            return "CERRADO";

        if (roomInfo.PlayerCount >= roomInfo.MaxPlayers)
            return "LLENO";

        // Verificar si el juego ya comenzó
        if (roomInfo.CustomProperties != null &&
            roomInfo.CustomProperties.ContainsKey("EstadoJuego"))
        {
            string estadoJuego = roomInfo.CustomProperties["EstadoJuego"].ToString();
            if (estadoJuego == "EnJuego")
                return "EN JUEGO";
            else if (estadoJuego == "Finalizado")
                return "FINALIZADO";
        }

        return "ABIERTO";
    }

    /// <summary>
    /// Maneja el click en el botón de unirse
    /// </summary>
    private void OnJoinButtonClicked()
    {
        if (roomInfo == null)
        {
            Debug.LogError("[RoomItem] RoomInfo es null");
            return;
        }

        if (lobbyManager == null)
        {
            Debug.LogError("[RoomItem] LobbyManager es null");
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("[RoomItem] No conectado a Photon");
            return;
        }

        Debug.Log($"[RoomItem] Intentando unirse a sala: {roomInfo.Name}");
        lobbyManager.JoinRoom(roomInfo.Name);
    }

    /// <summary>
    /// Formato alternativo para mostrar la información
    /// Formato: "Sala1 | 1/15 | ABIERTO"
    /// </summary>
    public void SetupAlternativeFormat()
    {
        if (roomInfo == null)
            return;

        string displayText = $"{roomInfo.Name} | {roomInfo.PlayerCount}/{roomInfo.MaxPlayers} | {GetRoomStatus()}";

        if (roomNameText != null)
        {
            roomNameText.text = displayText;
        }
    }

    /// <summary>
    /// Actualiza la información de la sala (útil para refresh)
    /// </summary>
    public void RefreshRoomInfo(RoomInfo info)
    {
        roomInfo = info;
        UpdateUI();
    }

    private void OnDestroy()
    {
        // Limpiar listeners al destruir para evitar memory leaks
        if (leanButton != null)
        {
            leanButton.OnClick.RemoveAllListeners();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Método de debugging para el editor
    /// </summary>
    private void OnValidate()
    {
        if (leanButton == null)
        {
            leanButton = GetComponent<LeanButton>();
        }
    }
#endif
}