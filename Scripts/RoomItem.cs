using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;

/// <summary>
/// Representa un item de sala en la lista del lobby
/// </summary>
public class RoomItem : MonoBehaviour
{
    [Header("UI Referencias")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button joinButton;

    private RoomInfo roomInfo;
    private LobbyManager lobbyManager;

    /// <summary>
    /// Configura el item con la información de la sala
    /// </summary>
    public void Setup(RoomInfo info, LobbyManager lobby)
    {
        roomInfo = info;
        lobbyManager = lobby;

        UpdateUI();

        // Configurar botón
        if (joinButton != null)
        {
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(OnJoinButtonClicked);
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

        // Habilitar/deshabilitar botón según disponibilidad
        if (joinButton != null)
        {
            bool canJoin = roomInfo.IsOpen &&
                          roomInfo.PlayerCount < roomInfo.MaxPlayers;
            joinButton.interactable = canJoin;
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
        if (lobbyManager != null && roomInfo != null)
        {
            lobbyManager.JoinRoom(roomInfo.Name);
        }
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
}