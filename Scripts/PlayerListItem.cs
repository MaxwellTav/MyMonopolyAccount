using UnityEngine;
using TMPro;
using Photon.Realtime;
using Lean.Gui;

/// <summary>
/// Representa un jugador en la lista del panel de transferencias
/// Usa UN SOLO texto para nombre y dinero
/// </summary>
public class PlayerListItem : MonoBehaviour
{
    [Header("UI Referencias")]
    [SerializeField] private TMP_Text playerText;

    private LeanButton button;
    private Player player;
    private bool showMoney;
    private bool isLottery = false;

    private void Awake()
    {
        button = GetComponent<LeanButton>();

        if (button != null)
        {
            button.OnClick.AddListener(OnClicked);
        }
    }

    /// <summary>
    /// Configura el item con los datos del jugador
    /// </summary>
    public void Setup(string playerName, double money, bool showMoneyValue, Player playerRef)
    {
        player = playerRef;
        showMoney = showMoneyValue;
        isLottery = (player == null); // Si no hay player, es la lotería

        if (playerText == null)
        {
            Debug.LogError("[PlayerListItem] PlayerText no asignado");
            return;
        }

        // Formatear texto según si se muestra el dinero
        if (showMoneyValue)
        {
            // Formato: Nombre\n$0,000<size=10>.00</size>
            string formattedMoney = FormatMoney(money);
            playerText.text = $"{playerName}\n{formattedMoney}";
        }
        else
        {
            // Solo nombre
            playerText.text = playerName;
        }
    }

    /// <summary>
    /// Formatea el dinero con decimales pequeños
    /// Formato: $0,000<size=10>.00</size>
    /// </summary>
    private string FormatMoney(double amount)
    {
        // Separar parte entera y decimal
        int integerPart = (int)amount;
        int decimalPart = (int)((amount - integerPart) * 100);

        // Formatear: $15,000<size=10>.00</size>
        return $"${integerPart:N0}<size=10>.{decimalPart:00}</size>";
    }

    /// <summary>
    /// Se ejecuta al hacer click en el jugador
    /// Abre el TransactionPanel
    /// </summary>
    private void OnClicked()
    {
        TransactionManager transactionManager = FindObjectOfType<TransactionManager>();

        if (transactionManager == null)
        {
            Debug.LogError("[PlayerListItem] No se encontró TransactionManager en la escena");
            return;
        }

        if (isLottery)
        {
            Debug.Log($"[PlayerListItem] Click en Lotería / Impuestos");
            transactionManager.OpenForLottery();
        }
        else if (player != null)
        {
            Debug.Log($"[PlayerListItem] Click en jugador: {player.NickName}");
            transactionManager.OpenForPlayer(player);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.OnClick.RemoveAllListeners();
        }
    }
}