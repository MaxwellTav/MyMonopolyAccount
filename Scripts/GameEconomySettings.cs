using UnityEngine;
using Photon.Pun;
using System;
using ExitGames.Client.Photon;

/// <summary>
/// Sistema de econom�a escalable para el Monopolio
/// Calcula todos los valores del juego bas�ndose en la configuraci�n de la sala
/// </summary>
public class GameEconomySettings : MonoBehaviour
{
    public static GameEconomySettings Instance { get; private set; }

    [Header("Valores de Referencia del Monopolio Original")]
    private const double ORIGINAL_MEDITERRANEO = 60.0;
    private const double ORIGINAL_SALIDA = 200.0;
    private const double ORIGINAL_IMPUESTO_LUJO = 75.0;
    private const double ORIGINAL_IMPUESTO_RENTA = 200.0;

    [SerializeField] float InitialAmmount = 15000;

    [Header("Configuraci�n Actual")]
    private int denominacionMinima = 1;
    private int valorMediterraneo = 60; // Valor de Avenida Mediterr�neo configurado

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
    }

    private void Start()
    {
        LoadEconomyFromRoom();
    }

    /// <summary>
    /// Carga la configuraci�n de econom�a desde las propiedades de la sala
    /// </summary>
    public void LoadEconomyFromRoom()
    {
        //if (!PhotonNetwork.InRoom)
        //{
        //    Debug.LogWarning("No estamos en una sala, usando valores por defecto");
        //    return;
        //}

        var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;

        if (roomProps.ContainsKey("DenominacionMinima"))
        {
            denominacionMinima = (int)roomProps["DenominacionMinima"];
        }

        if (roomProps.ContainsKey("ValorMediterraneo"))
        {
            valorMediterraneo = (int)roomProps["ValorMediterraneo"];
        }

        Debug.Log($"Econom�a cargada - Denominaci�n: {denominacionMinima}, Mediterr�neo: {valorMediterraneo}");
        LogEconomyValues();
    }

    /// <summary>
    /// Calcula un valor escalado basado en la proporci�n del juego
    /// </summary>
    /// <param name="valorOriginal">Valor del Monopolio original (ej: 200 para Salida)</param>
    /// <returns>Valor escalado seg�n la econom�a actual</returns>
    public int CalculateScaledValue(double valorOriginal)
    {
        // Formula: (valorOriginal * valorMediterraneoActual) / valorMediterraneoOriginal
        double resultado = (valorOriginal * valorMediterraneo) / ORIGINAL_MEDITERRANEO;

        // Redondear al m�ltiplo m�s cercano de la denominaci�n m�nima
        int valorFinal = RoundToDenomination((int)Math.Round(resultado));

        return valorFinal;
    }

    /// <summary>
    /// Redondea un valor al m�ltiplo m�s cercano de la denominaci�n m�nima
    /// </summary>
    private int RoundToDenomination(int value)
    {
        if (denominacionMinima <= 1)
            return value;

        return (int)(Math.Round((double)value / denominacionMinima) * denominacionMinima);
    }

    // ========== VALORES ESPEC�FICOS DEL JUEGO ==========

    /// <summary>
    /// Dinero que recibe el jugador al pasar por SALIDA
    /// </summary>
    public int GetSalaryAmount()
    {
        return CalculateScaledValue(ORIGINAL_SALIDA);
    }

    /// <summary>
    /// Impuesto de lujo
    /// </summary>
    public int GetLuxuryTax()
    {
        return CalculateScaledValue(ORIGINAL_IMPUESTO_LUJO);
    }

    /// <summary>
    /// Impuesto de renta
    /// </summary>
    public int GetIncomeTax()
    {
        return CalculateScaledValue(ORIGINAL_IMPUESTO_RENTA);
    }

    /// <summary>
    /// Calcula el valor de una propiedad seg�n su valor original
    /// </summary>
    public int GetPropertyValue(int originalValue)
    {
        return CalculateScaledValue(originalValue);
    }

    /// <summary>
    /// Calcula el costo de una casa
    /// </summary>
    public int GetHouseCost(int originalCost)
    {
        return CalculateScaledValue(originalCost);
    }

    /// <summary>
    /// Calcula la renta de una propiedad
    /// </summary>
    public int GetRentValue(int originalRent)
    {
        return CalculateScaledValue(originalRent);
    }

    /// <summary>
    /// Obtiene el dinero inicial de cada jugador
    /// Original: 1500
    /// </summary>
    public int GetInitialMoney()
    {
        return CalculateScaledValue(InitialAmmount);
    }

    /// <summary>
    /// Calcula cualquier valor de tarjeta de Casualidad o Arca Comunal
    /// </summary>
    public int CalculateCardValue(int originalCardValue)
    {
        return CalculateScaledValue(originalCardValue);
    }

    /// <summary>
    /// Obtiene la denominaci�n m�nima del juego
    /// </summary>
    public int GetMinDenomination()
    {
        return denominacionMinima;
    }

    /// <summary>
    /// Obtiene el valor configurado de Mediterr�neo
    /// </summary>
    public int GetMediterraneoValue()
    {
        return valorMediterraneo;
    }

    /// <summary>
    /// Calcula el factor de escala actual
    /// </summary>
    public double GetScaleFactor()
    {
        return (double)valorMediterraneo / ORIGINAL_MEDITERRANEO;
    }

    /// <summary>
    /// Muestra en consola todos los valores importantes de la econom�a
    /// </summary>
    public void LogEconomyValues()
    {
        Debug.Log("========== ECONOM�A DEL JUEGO ==========");
        Debug.Log($"Factor de Escala: x{GetScaleFactor():F2}");
        Debug.Log($"Denominaci�n M�nima: ${denominacionMinima}");
        Debug.Log($"Valor Mediterr�neo: ${valorMediterraneo}");
        Debug.Log($"Dinero Inicial: ${GetInitialMoney()}");
        Debug.Log($"Dinero por Salida: ${GetSalaryAmount()}");
        Debug.Log($"Impuesto de Lujo: ${GetLuxuryTax()}");
        Debug.Log($"Impuesto de Renta: ${GetIncomeTax()}");
        Debug.Log("=======================================");
    }

    /// <summary>
    /// Ejemplo de uso para las tarjetas
    /// </summary>
    public string GetCardValueExample(string cardDescription, int originalValue)
    {
        int newValue = CalculateCardValue(originalValue);
        return cardDescription.Replace(originalValue.ToString(), newValue.ToString());
    }
}

/// <summary>
/// Clase auxiliar para definir valores originales del Monopolio
/// </summary>
public static class MonopolyOriginalValues
{
    // Propiedades por color
    public static readonly int[] BROWN = { 60, 60 }; // Mediterr�neo, B�ltico
    public static readonly int[] LIGHT_BLUE = { 100, 100, 120 };
    public static readonly int[] PINK = { 140, 140, 160 };
    public static readonly int[] ORANGE = { 180, 180, 200 };
    public static readonly int[] RED = { 220, 220, 240 };
    public static readonly int[] YELLOW = { 260, 260, 280 };
    public static readonly int[] GREEN = { 300, 300, 320 };
    public static readonly int[] BLUE = { 350, 400 }; // Park Place, Boardwalk

    // Ferrocarriles
    public const int RAILROAD = 200;

    // Servicios p�blicos
    public const int UTILITY = 150;

    // Costos de construcci�n
    public const int HOUSE_BROWN_LIGHTBLUE = 50;
    public const int HOUSE_PINK_ORANGE = 100;
    public const int HOUSE_RED_YELLOW = 150;
    public const int HOUSE_GREEN_BLUE = 200;

    // Valores de tarjetas comunes
    public const int BANK_ERROR = 200;
    public const int DOCTOR_FEE = -50;
    public const int SCHOOL_FEE = -150;
    public const int BEAUTY_CONTEST = 10;
    public const int INHERIT = 100;
    public const int LIFE_INSURANCE = 100;
    public const int HOSPITAL_FEE = -100;
    public const int SALE_STOCK = 50;
}