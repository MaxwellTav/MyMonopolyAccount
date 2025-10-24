using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;
using System.Collections;
using Lean.Gui;

/// <summary>
/// Gestiona la escena de inicialización (InitScene)
/// Pide el nombre solo la primera vez y luego va al Lobby
/// </summary>
public class InitSceneManager : MonoBehaviour
{
    [Header("UI Referencias")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private TMP_InputField nickNameInput;
    [SerializeField] private LeanButton okButton;

    [Header("Configuración")]
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private int minNameLength = 3;
    [SerializeField] private int maxNameLength = 15;

    [Header("UI Feedback (Opcional)")]
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private GameObject loadingPanel;

    private const string PLAYER_NAME_KEY = "MonopolyPlayerName";
    private bool isProcessing = false;

    private void Start()
    {
        // Configurar botón
        if (okButton != null)
        {
            okButton.OnClick.AddListener(OnOkButtonClicked);
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

        if (!string.IsNullOrEmpty(savedName))
        {
            // Ya tiene nombre - Configurar y ir directo al lobby
            Debug.Log($"Nombre guardado encontrado: {savedName}");
            SetPlayerName(savedName);

            // Ocultar panel de nombre
            if (mainMenuCanvas != null)
                mainMenuCanvas.SetActive(false);

            // Esperar a estar conectado a Photon antes de cambiar de escena
            StartCoroutine(WaitForConnectionAndLoadLobby());
        }
        else
        {
            // Primera vez - Mostrar panel de nombre
            Debug.Log("Primera vez - Pidiendo nombre");
            ShowNameInput();
        }
    }

    /// <summary>
    /// Muestra el panel de entrada de nombre
    /// </summary>
    private void ShowNameInput()
    {
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(true);

        // Focus en el input
        if (nickNameInput != null)
        {
            nickNameInput.Select();
            nickNameInput.ActivateInputField();
        }

        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Maneja el click en el botón OK
    /// </summary>
    public void OnOkButtonClicked()
    {
        if (isProcessing)
        {
            Debug.Log("Ya estamos procesando...");
            return;
        }

        string playerName = nickNameInput != null ? nickNameInput.text.Trim() : "";

        // Validar nombre
        if (string.IsNullOrEmpty(playerName))
        {
            ShowError("Por favor ingresa un nombre");
            return;
        }

        if (playerName.Length < minNameLength)
        {
            ShowError($"El nombre debe tener al menos {minNameLength} caracteres");
            return;
        }

        if (playerName.Length > maxNameLength)
        {
            playerName = playerName.Substring(0, maxNameLength);
        }

        // Guardar y configurar nombre
        isProcessing = true;
        SavePlayerName(playerName);
        SetPlayerName(playerName);

        // Mostrar feedback
        if (errorText != null)
        {
            errorText.color = Color.green;
            errorText.text = "¡Nombre guardado! Conectando...";
            errorText.gameObject.SetActive(true);
        }

        // Ocultar input
        if (nickNameInput != null)
            nickNameInput.interactable = false;

        if (okButton != null)
            okButton.interactable = false;

        // Esperar a estar conectado antes de ir al lobby
        StartCoroutine(WaitForConnectionAndLoadLobby());
    }

    /// <summary>
    /// Espera a estar conectado a Photon y luego carga el lobby
    /// </summary>
    private IEnumerator WaitForConnectionAndLoadLobby()
    {
        ShowLoading(true);

        Debug.Log("Esperando conexión a Photon...");

        // Esperar hasta que Photon esté conectado y en el lobby
        float timeout = 10f;
        float elapsed = 0f;

        while (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;

            if (elapsed >= timeout)
            {
                Debug.LogError("Timeout esperando conexión a Photon");
                ShowError("Error de conexión. Reintentando...");

                // Reintentar conexión
                if (!PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.ConnectUsingSettings();
                }

                elapsed = 0f;
            }

            // Debug info
            if ((int)elapsed % 2 == 0)
            {
                Debug.Log($"Estado: IsConnected={PhotonNetwork.IsConnected}, " +
                         $"InLobby={PhotonNetwork.InLobby}, " +
                         $"NetworkClientState={PhotonNetwork.NetworkClientState}");
            }
        }

        Debug.Log("¡Conectado! Cargando lobby...");

        // Pequeño delay para suavidad
        yield return new WaitForSeconds(0.5f);

        // Cargar escena del lobby
        LoadLobbyScene();
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
    /// Carga la escena del lobby
    /// </summary>
    private void LoadLobbyScene()
    {
        Debug.Log($"Cargando escena: {lobbySceneName}");
        SceneManager.LoadScene(lobbySceneName);
    }

    /// <summary>
    /// Muestra un mensaje de error
    /// </summary>
    private void ShowError(string message)
    {
        Debug.LogWarning(message);

        if (errorText != null)
        {
            errorText.color = Color.red;
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Muestra u oculta el panel de carga
    /// </summary>
    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }
    }

    /// <summary>
    /// Limpia el nombre guardado (para testing)
    /// </summary>
    [ContextMenu("Limpiar Nombre Guardado")]
    public void ClearSavedName()
    {
        PlayerPrefs.DeleteKey(PLAYER_NAME_KEY);
        PlayerPrefs.Save();
        Debug.Log("Nombre eliminado - La próxima vez pedirá el nombre de nuevo");
    }
}