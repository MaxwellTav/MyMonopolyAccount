using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

/// <summary>
/// Gestiona el nombre del jugador, preguntándolo solo la primera vez
/// </summary>
public class PlayerNameManager : MonoBehaviour
{
    [Header("UI Referencias")]
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject lobbyPanel;

    [Header("Configuración")]
    [SerializeField] private string defaultName = "Jugador";
    [SerializeField] private int minNameLength = 3;
    [SerializeField] private int maxNameLength = 15;

    private const string PLAYER_NAME_KEY = "MonopolyPlayerName";

    private void Start()
    {
        // Configurar botón
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmName);
        }

        // Verificar si ya tiene nombre guardado
        CheckPlayerName();
    }

    /// <summary>
    /// Verifica si el jugador ya tiene un nombre guardado
    /// </summary>
    private void CheckPlayerName()
    {
        string savedName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "");

        if (string.IsNullOrEmpty(savedName))
        {
            // Primera vez - Mostrar panel de nombre
            ShowNameInput();
        }
        else
        {
            // Ya tiene nombre - Ir directo al lobby
            SetPlayerName(savedName);
            ShowLobby();
        }
    }

    /// <summary>
    /// Muestra el panel de entrada de nombre
    /// </summary>
    private void ShowNameInput()
    {
        if (nameInputPanel != null)
            nameInputPanel.SetActive(true);

        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        // Focus en el input
        if (nameInputField != null)
        {
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    /// <summary>
    /// Muestra el lobby principal
    /// </summary>
    private void ShowLobby()
    {
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
    }

    /// <summary>
    /// Maneja la confirmación del nombre
    /// </summary>
    public void OnConfirmName()
    {
        string playerName = nameInputField != null ? nameInputField.text.Trim() : "";

        // Validar nombre
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("El nombre no puede estar vacío");
            ShowError("Por favor ingresa un nombre");
            return;
        }

        if (playerName.Length < minNameLength)
        {
            Debug.LogWarning($"El nombre debe tener al menos {minNameLength} caracteres");
            ShowError($"Nombre muy corto (mínimo {minNameLength} caracteres)");
            return;
        }

        if (playerName.Length > maxNameLength)
        {
            playerName = playerName.Substring(0, maxNameLength);
        }

        // Guardar y configurar nombre
        SavePlayerName(playerName);
        SetPlayerName(playerName);
        ShowLobby();
    }

    /// <summary>
    /// Guarda el nombre del jugador en PlayerPrefs
    /// </summary>
    private void SavePlayerName(string playerName)
    {
        PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
        PlayerPrefs.Save();
        Debug.Log($"Nombre guardado: {playerName}");
    }

    /// <summary>
    /// Configura el nombre del jugador en Photon
    /// </summary>
    private void SetPlayerName(string playerName)
    {
        PhotonNetwork.NickName = playerName;
        Debug.Log($"Nombre de Photon configurado: {PhotonNetwork.NickName}");
    }

    /// <summary>
    /// Muestra un mensaje de error
    /// </summary>
    private void ShowError(string message)
    {
        // Aquí puedes implementar un sistema de notificaciones
        Debug.LogWarning(message);
        // Ejemplo: errorText.text = message;
    }

    /// <summary>
    /// Obtiene el nombre del jugador actual
    /// </summary>
    public string GetPlayerName()
    {
        return PhotonNetwork.NickName;
    }

    /// <summary>
    /// Permite cambiar el nombre manualmente (opcional)
    /// </summary>
    public void ChangePlayerName()
    {
        ShowNameInput();
    }

    /// <summary>
    /// Limpia el nombre guardado (para testing)
    /// </summary>
    public void ClearSavedName()
    {
        PlayerPrefs.DeleteKey(PLAYER_NAME_KEY);
        PlayerPrefs.Save();
        Debug.Log("Nombre eliminado");
    }
}