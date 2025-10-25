using UnityEngine;
using Photon.Pun;

/// <summary>
/// Gestiona transacciones entre jugadores con comisi�n del 0.7%
/// </summary>
public class TransactionManager : MonoBehaviourPun
{
    public static TransactionManager Instance { get; private set; }

    private const float COMMISSION_RATE = 0.007f; // 0.7%

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Transfiere dinero con comisi�n
    /// </summary>
    public int CalculateTransfer(int amount)
    {
        int commission = Mathf.RoundToInt(amount * COMMISSION_RATE);
        int finalAmount = amount - commission;

        Debug.Log($"Transferencia: {amount} - Comisi�n: {commission} = {finalAmount}");
        return finalAmount;
    }

    /// <summary>
    /// Calcula solo la comisi�n
    /// </summary>
    public int CalculateCommission(int amount)
    {
        return Mathf.RoundToInt(amount * COMMISSION_RATE);
    }

    /// <summary>
    /// Obtiene el monto final despu�s de comisi�n
    /// </summary>
    public int GetFinalAmount(int amount)
    {
        return amount - CalculateCommission(amount);
    }
}