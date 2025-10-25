using ExitGames.Client.Photon;
using Lean.Gui;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Gestiona transacciones entre jugadores con comisión del 0.7%
/// Permite que el banco pueda tener dinero negativo
/// </summary>
public class TransactionManager : MonoBehaviourPun
{
    public static TransactionManager Instance { get; private set; }
    private const float COMMISSION_RATE = 0.007f; // 0.7%

    [Header("UI Referencias")]
    [SerializeField] private GameObject transactionPanel;
    [SerializeField] private TMP_InputField amountInput;
    [SerializeField] private TMP_Text targetPlayerText;
    [SerializeField] private TMP_Text commissionText;

    [Header("Botones del Teclado")]
    [SerializeField] private LeanButton[] numberButtons; // 0-9
    [SerializeField] private LeanButton dotButton; // .
    [SerializeField] private LeanButton clearButton;
    [SerializeField] private LeanButton button200;
    [SerializeField] private LeanButton button500;
    [SerializeField] private LeanButton button1000;
    [SerializeField] private LeanButton button5000;
    [SerializeField] private LeanButton confirmButton;

    [Space(15)]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> clips;

    // Estado
    private Player targetPlayer;
    private bool isLotteryTarget = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SetupKeyboardButtons();

        if (transactionPanel != null)
        {
            transactionPanel.SetActive(false);
        }

        if (amountInput != null)
        {
            amountInput.onValueChanged.AddListener(OnAmountChanged);
        }
    }

    #region CÁLCULOS DE COMISIÓN

    public int CalculateTransfer(int amount)
    {
        int commission = Mathf.RoundToInt(amount * COMMISSION_RATE);
        int finalAmount = amount - commission;
        Debug.Log($"[TransactionManager] Transferencia: {amount} - Comisión: {commission} = {finalAmount}");
        return finalAmount;
    }

    public int CalculateCommission(int amount)
    {
        return Mathf.RoundToInt(amount * COMMISSION_RATE);
    }

    public int GetFinalAmount(int amount)
    {
        return amount - CalculateCommission(amount);
    }

    public double CalculateTransferDouble(double amount)
    {
        double commission = amount * COMMISSION_RATE;
        double finalAmount = amount - commission;
        Debug.Log($"[TransactionManager] Transferencia: ${amount:N2} - Comisión: ${commission:N2} = ${finalAmount:N2}");
        return finalAmount;
    }

    public double CalculateCommissionDouble(double amount)
    {
        return amount * COMMISSION_RATE;
    }

    #endregion

    #region CONFIGURACIÓN DE BOTONES

    private void SetupKeyboardButtons()
    {
        if (numberButtons != null)
        {
            for (int i = 0; i < numberButtons.Length; i++)
            {
                int num = i;
                if (numberButtons[i] != null)
                {
                    numberButtons[i].OnClick.AddListener(() => OnNumberButtonClicked(num.ToString()));
                }
            }
        }

        if (dotButton != null)
        {
            dotButton.OnClick.AddListener(() => OnNumberButtonClicked("."));
        }

        if (clearButton != null)
        {
            clearButton.OnClick.AddListener(OnClearButtonClicked);
        }

        if (button200 != null)
        {
            button200.OnClick.AddListener(() => OnQuickAmountButtonClicked("200"));
        }

        if (button500 != null)
        {
            button500.OnClick.AddListener(() => OnQuickAmountButtonClicked("500"));
        }

        if (button1000 != null)
        {
            button1000.OnClick.AddListener(() => OnQuickAmountButtonClicked("1000"));
        }

        if (button5000 != null)
        {
            button5000.OnClick.AddListener(() => OnQuickAmountButtonClicked("5000"));
        }

        if (confirmButton != null)
        {
            confirmButton.OnClick.AddListener(OnConfirmButtonClicked);
        }
    }

    #endregion

    #region ABRIR/CERRAR PANEL

    public void OpenForPlayer(Player player)
    {
        targetPlayer = player;
        isLotteryTarget = false;

        if (targetPlayerText != null)
        {
            targetPlayerText.text = $"Transferir a\n{player.NickName}";
        }

        OpenPanel();
    }

    public void OpenForLottery()
    {
        targetPlayer = null;
        isLotteryTarget = true;

        if (targetPlayerText != null)
        {
            targetPlayerText.text = "Transferir a\nLotería / Impuestos";
        }

        OpenPanel();
    }

    private void OpenPanel()
    {
        if (transactionPanel != null)
        {
            transactionPanel.SetActive(true);
        }

        if (amountInput != null)
        {
            amountInput.text = "";
        }

        if (commissionText != null)
        {
            commissionText.text = "";
        }

        Debug.Log($"[TransactionManager] Panel abierto. Target: {(isLotteryTarget ? "Lotería" : targetPlayer?.NickName)}");
    }

    public void ClosePanel()
    {
        if (transactionPanel != null)
        {
            transactionPanel.SetActive(false);
        }

        targetPlayer = null;
        isLotteryTarget = false;
    }

    #endregion

    #region TECLADO NUMÉRICO

    private void OnNumberButtonClicked(string digit)
    {
        if (amountInput == null)
            return;

        if (digit == "." && amountInput.text.Contains("."))
            return;

        amountInput.text += digit;
    }

    private void OnQuickAmountButtonClicked(string amount)
    {
        if (amountInput == null)
            return;

        amountInput.text = amount;
    }

    private void OnClearButtonClicked()
    {
        if (amountInput == null)
            return;

        amountInput.text = "";
    }

    private void OnAmountChanged(string value)
    {
        if (commissionText == null)
            return;

        if (string.IsNullOrEmpty(value))
        {
            commissionText.text = "";
            return;
        }

        if (double.TryParse(value, out double amount))
        {
            double commission = CalculateCommissionDouble(amount);
            double finalAmount = amount - commission;
            commissionText.text = $"Comisión: ${commission:N2}\nRecibirá: ${finalAmount:N2}";
        }
    }

    #endregion

    #region CONFIRMAR TRANSFERENCIA

    private void OnConfirmButtonClicked()
    {
        if (amountInput == null || string.IsNullOrEmpty(amountInput.text))
        {
            Debug.LogWarning("[TransactionManager] No hay cantidad ingresada");
            return;
        }

        if (!double.TryParse(amountInput.text, out double amount))
        {
            Debug.LogError("[TransactionManager] Cantidad inválida");
            return;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("[TransactionManager] La cantidad debe ser mayor a 0");
            return;
        }

        // ✅ VALIDACIÓN CONDICIONAL: Solo el banco puede quedar en negativo
        double myMoney = GetMyMoney();
        bool iAmBank = IsPlayerBank(PhotonNetwork.LocalPlayer);

        if (!iAmBank && amount > myMoney)
        {
            Debug.LogWarning($"[TransactionManager] Fondos insuficientes. Tienes: ${myMoney:N2}, Intentas: ${amount:N2}");
            ShowInsufficientFundsMessage(); // Opcional: mostrar mensaje en UI
            return;
        }

        // Si soy el banco, puedo quedarme en negativo
        if (iAmBank && myMoney < amount)
        {
            Debug.Log($"[TransactionManager] BANCO haciendo transferencia que resulta en negativo. Balance: ${myMoney:N2}, Transferencia: ${amount:N2}");
        }

        // Calcular comisión y monto final
        double commission = CalculateCommissionDouble(amount);
        double finalAmount = amount - commission;

        // Realizar transferencia
        if (isLotteryTarget)
        {
            TransferToLottery(amount, finalAmount, commission);
        }
        else if (targetPlayer != null)
        {
            TransferToPlayer(targetPlayer, amount, finalAmount, commission);
        }

        // Sonido de transferencia
        if (audioSource != null && clips != null && clips.Count > 0)
        {
            audioSource.clip = clips[0];
            audioSource.Play();
        }

        ClosePanel();
    }

    /// <summary>
    /// Verifica si un jugador es el banco
    /// </summary>
    private bool IsPlayerBank(Player player)
    {
        if (BankManager.Instance != null)
        {
            return BankManager.Instance.IsBankPlayer(player);
        }

        // Fallback: si no hay BankManager, nadie es banco
        return false;
    }

    /// <summary>
    /// Muestra mensaje de fondos insuficientes (opcional)
    /// </summary>
    private void ShowInsufficientFundsMessage()
    {
        // Puedes implementar un panel de mensaje aquí
        // Por ejemplo, mostrar un texto temporal en pantalla
        Debug.Log("[TransactionManager] Mostrar mensaje: Fondos Insuficientes");
    }

    private void TransferToPlayer(Player target, double totalAmount, double finalAmount, double commission)
    {
        double myMoney = GetMyMoney();
        double newMyMoney = myMoney - totalAmount;
        SetMyMoney(newMyMoney);

        double targetMoney = GetPlayerMoney(target);
        double newTargetMoney = targetMoney + finalAmount;
        SetPlayerMoney(target, newTargetMoney);

        AddCommissionToLottery(commission);

        Debug.Log($"[TransactionManager] Transferido ${totalAmount:N2} a {target.NickName}. Recibe: ${finalAmount:N2}, Comisión: ${commission:N2}");
    }

    private void TransferToLottery(double totalAmount, double finalAmount, double commission)
    {
        double myMoney = GetMyMoney();
        double newMyMoney = myMoney - totalAmount;
        SetMyMoney(newMyMoney);

        AddToLottery(totalAmount);

        Debug.Log($"[TransactionManager] Transferido ${totalAmount:N2} a Lotería/Impuestos");
    }

    private void AddCommissionToLottery(double commission)
    {
        GameSceneManager gameManager = FindObjectOfType<GameSceneManager>();
        if (gameManager != null && PhotonNetwork.IsMasterClient)
        {
            gameManager.AddToLottery(commission);
        }
    }

    private void AddToLottery(double amount)
    {
        GameSceneManager gameManager = FindObjectOfType<GameSceneManager>();
        if (gameManager != null && PhotonNetwork.IsMasterClient)
        {
            gameManager.AddToLottery(amount);
        }
    }

    #endregion

    #region HELPERS DE DINERO

    private double GetMyMoney()
    {
        return GetPlayerMoney(PhotonNetwork.LocalPlayer);
    }

    private double GetPlayerMoney(Player player)
    {
        if (player.CustomProperties.ContainsKey("Money"))
        {
            return (double)player.CustomProperties["Money"];
        }
        return 0;
    }

    private void SetMyMoney(double amount)
    {
        SetPlayerMoney(PhotonNetwork.LocalPlayer, amount);
    }

    private void SetPlayerMoney(Player player, double amount)
    {
        Hashtable props = new Hashtable
        {
            { "Money", amount }
        };
        player.SetCustomProperties(props);
    }

    #endregion

    private void OnDestroy()
    {
        if (numberButtons != null)
        {
            foreach (var btn in numberButtons)
            {
                if (btn != null)
                    btn.OnClick.RemoveAllListeners();
            }
        }

        if (dotButton != null)
            dotButton.OnClick.RemoveAllListeners();

        if (clearButton != null)
            clearButton.OnClick.RemoveAllListeners();

        if (button200 != null)
            button200.OnClick.RemoveAllListeners();

        if (button500 != null)
            button500.OnClick.RemoveAllListeners();

        if (button1000 != null)
            button1000.OnClick.RemoveAllListeners();

        if (button5000 != null)
            button5000.OnClick.RemoveAllListeners();

        if (confirmButton != null)
            confirmButton.OnClick.RemoveAllListeners();

        if (amountInput != null)
            amountInput.onValueChanged.RemoveAllListeners();
    }
}