using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

/// <summary>
/// Sistema de tarjetas de Casualidad y Arca Comunal
/// con econom�a escalable seg�n la configuraci�n del juego
/// </summary>
public class CardSystem : MonoBehaviour
{
    public static CardSystem Instance { get; private set; }

    [Header("Configuraci�n")]
    [SerializeField] private bool shuffleOnStart = true;

    private List<Card> chanceCards = new List<Card>();
    private List<Card> communityChestCards = new List<Card>();

    private Queue<Card> chanceCardsDeck = new Queue<Card>();
    private Queue<Card> communityChestDeck = new Queue<Card>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeCards();

        if (shuffleOnStart)
        {
            ShuffleAllDecks();
        }
    }

    /// <summary>
    /// Inicializa todas las tarjetas del juego
    /// </summary>
    private void InitializeCards()
    {
        InitializeChanceCards();
        InitializeCommunityChestCards();

        Debug.Log($"Tarjetas inicializadas - Casualidad: {chanceCards.Count}, Arca Comunal: {communityChestCards.Count}");
    }

    /// <summary>
    /// Inicializa las tarjetas de Casualidad (Chance)
    /// </summary>
    private void InitializeChanceCards()
    {
        chanceCards.Clear();

        // TARJETAS DE MOVIMIENTO
        chanceCards.Add(new Card(
            "Avanza hasta la Salida",
            CardType.Movement,
            CardDeck.Chance,
            () => MoveToPosition(0)
        ));

        chanceCards.Add(new Card(
            "Avanza hasta Illinois Avenue",
            CardType.Movement,
            CardDeck.Chance,
            () => MoveToPosition(24)
        ));

        chanceCards.Add(new Card(
            "Avanza hasta St. Charles Place",
            CardType.Movement,
            CardDeck.Chance,
            () => MoveToPosition(11)
        ));

        chanceCards.Add(new Card(
            "Avanza al ferrocarril m�s cercano",
            CardType.Movement,
            CardDeck.Chance,
            () => MoveToNearestRailroad()
        ));

        chanceCards.Add(new Card(
            "Avanza al servicio p�blico m�s cercano",
            CardType.Movement,
            CardDeck.Chance,
            () => MoveToNearestUtility()
        ));

        chanceCards.Add(new Card(
            "Retrocede 3 espacios",
            CardType.Movement,
            CardDeck.Chance,
            () => MoveBackward(3)
        ));

        chanceCards.Add(new Card(
            "Ve a la c�rcel directamente",
            CardType.Jail,
            CardDeck.Chance,
            () => GoToJail()
        ));

        // TARJETAS DE DINERO
        chanceCards.Add(new Card(
            $"El banco te paga un dividendo de ${GameEconomySettings.Instance.CalculateCardValue(50)}",
            CardType.Money,
            CardDeck.Chance,
            () => GiveMoney(50)
        ));

        chanceCards.Add(new Card(
            $"Tu pr�stamo y construcci�n madura. Recibes ${GameEconomySettings.Instance.CalculateCardValue(150)}",
            CardType.Money,
            CardDeck.Chance,
            () => GiveMoney(150)
        ));

        chanceCards.Add(new Card(
            $"Multa por exceso de velocidad: ${GameEconomySettings.Instance.CalculateCardValue(15)}",
            CardType.Money,
            CardDeck.Chance,
            () => PayMoney(15)
        ));

        chanceCards.Add(new Card(
            $"Impuesto a los pobres: Paga ${GameEconomySettings.Instance.CalculateCardValue(15)}",
            CardType.Money,
            CardDeck.Chance,
            () => PayMoney(15)
        ));

        // TARJETAS DE REPARACIONES
        chanceCards.Add(new Card(
            $"Haz reparaciones generales en todas tus propiedades: Paga ${GameEconomySettings.Instance.CalculateCardValue(25)} por cada casa y ${GameEconomySettings.Instance.CalculateCardValue(100)} por cada hotel",
            CardType.Repair,
            CardDeck.Chance,
            () => PayRepairs(25, 100)
        ));

        // TARJETA DE SALIR DE LA C�RCEL
        chanceCards.Add(new Card(
            "Sales libre de la c�rcel (esta tarjeta puede guardarse hasta que sea necesaria)",
            CardType.GetOutOfJail,
            CardDeck.Chance,
            () => GiveGetOutOfJailCard()
        ));
    }

    /// <summary>
    /// Inicializa las tarjetas de Arca Comunal (Community Chest)
    /// </summary>
    private void InitializeCommunityChestCards()
    {
        communityChestCards.Clear();

        // TARJETAS DE DINERO
        communityChestCards.Add(new Card(
            $"Avanza hasta la Salida. Cobra ${GameEconomySettings.Instance.GetSalaryAmount()}",
            CardType.Movement,
            CardDeck.CommunityChest,
            () => MoveToPosition(0)
        ));

        communityChestCards.Add(new Card(
            $"Error del banco a tu favor. Cobra ${GameEconomySettings.Instance.CalculateCardValue(200)}",
            CardType.Money,
            CardDeck.CommunityChest,
            () => GiveMoney(200)
        ));

        communityChestCards.Add(new Card(
            $"Honorarios del doctor. Paga ${GameEconomySettings.Instance.CalculateCardValue(50)}",
            CardType.Money,
            CardDeck.CommunityChest,
            () => PayMoney(50)
        ));

        communityChestCards.Add(new Card(
            $"Vendes acciones. Recibes ${GameEconomySettings.Instance.CalculateCardValue(50)}",
            CardType.Money,
            CardDeck.CommunityChest,
            () => GiveMoney(50)
        ));

        communityChestCards.Add(new Card(
            $"Ganas el segundo premio en un concurso de belleza. Cobra ${GameEconomySettings.Instance.CalculateCardValue(10)}",
            CardType.Money,
            CardDeck.CommunityChest,
            () => GiveMoney(10)
        ));

        communityChestCards.Add(new Card(
            $"Heredas ${GameEconomySettings.Instance.CalculateCardValue(100)}",
            CardType.Money,
            CardDeck.CommunityChest,
            () => GiveMoney(100)
        ));

        communityChestCards.Add(new Card(
            $"Tu seguro de vida madura. Cobra ${GameEconomySettings.Instance.CalculateCardValue(100)}",
            CardType.Money,
            CardDeck.CommunityChest,
            () => GiveMoney(100)
        ));

        communityChestCards.Add(new Card(
            $"Gastos hospitalarios. Paga ${GameEconomySettings.Instance.CalculateCardValue(100)}",
            CardType.Money,
            CardDeck.CommunityChest,
            () => PayMoney(100)
        ));

        communityChestCards.Add(new Card(
            $"Gastos escolares. Paga ${GameEconomySettings.Instance.CalculateCardValue(50)}",
            CardType.Money,
            CardDeck.CommunityChest,
            () => PayMoney(50)
        ));

        communityChestCards.Add(new Card(
            $"Recibes una consultor�a. Cobra ${GameEconomySettings.Instance.CalculateCardValue(25)}",
            CardType.Money,
            CardDeck.CommunityChest,
            () => GiveMoney(25)
        ));

        // TARJETAS DE IMPUESTOS/CUMPLEA�OS
        communityChestCards.Add(new Card(
            $"Es tu cumplea�os. Cada jugador te da ${GameEconomySettings.Instance.CalculateCardValue(10)}",
            CardType.CollectFromAll,
            CardDeck.CommunityChest,
            () => CollectFromAllPlayers(10)
        ));

        // TARJETAS ESPECIALES
        communityChestCards.Add(new Card(
            "Ve a la c�rcel directamente. No pases por la Salida. No cobres",
            CardType.Jail,
            CardDeck.CommunityChest,
            () => GoToJail()
        ));

        communityChestCards.Add(new Card(
            "Sales libre de la c�rcel (esta tarjeta puede guardarse hasta que sea necesaria)",
            CardType.GetOutOfJail,
            CardDeck.CommunityChest,
            () => GiveGetOutOfJailCard()
        ));

        // TARJETAS DE REPARACIONES
        communityChestCards.Add(new Card(
            $"Debes pagar por reparaciones en la calle: ${GameEconomySettings.Instance.CalculateCardValue(40)} por casa, ${GameEconomySettings.Instance.CalculateCardValue(115)} por hotel",
            CardType.Repair,
            CardDeck.CommunityChest,
            () => PayRepairs(40, 115)
        ));
    }

    /// <summary>
    /// Baraja todos los mazos
    /// </summary>
    public void ShuffleAllDecks()
    {
        ShuffleDeck(CardDeck.Chance);
        ShuffleDeck(CardDeck.CommunityChest);
    }

    /// <summary>
    /// Baraja un mazo espec�fico
    /// </summary>
    public void ShuffleDeck(CardDeck deckType)
    {
        List<Card> sourceCards = deckType == CardDeck.Chance ? chanceCards : communityChestCards;
        Queue<Card> targetDeck = deckType == CardDeck.Chance ? chanceCardsDeck : communityChestDeck;

        targetDeck.Clear();

        // Barajar usando Fisher-Yates
        List<Card> shuffled = sourceCards.OrderBy(x => Random.Range(0f, 1f)).ToList();

        foreach (var card in shuffled)
        {
            targetDeck.Enqueue(card);
        }

        Debug.Log($"Mazo {deckType} barajado: {targetDeck.Count} tarjetas");
    }

    /// <summary>
    /// Saca una tarjeta del mazo especificado
    /// </summary>
    public Card DrawCard(CardDeck deckType)
    {
        Queue<Card> deck = deckType == CardDeck.Chance ? chanceCardsDeck : communityChestDeck;

        if (deck.Count == 0)
        {
            Debug.Log($"Mazo {deckType} vac�o, barajando de nuevo...");
            ShuffleDeck(deckType);
        }

        Card drawnCard = deck.Dequeue();

        // Si no es una carta de "Salir de la c�rcel", regresarla al mazo
        if (drawnCard.Type != CardType.GetOutOfJail)
        {
            deck.Enqueue(drawnCard);
        }

        Debug.Log($"Carta sacada de {deckType}: {drawnCard.Description}");
        return drawnCard;
    }

    // ========== ACCIONES DE LAS TARJETAS ==========
    // Estas son funciones de ejemplo que debes adaptar a tu l�gica de juego

    private void MoveToPosition(int position)
    {
        Debug.Log($"[ACCI�N] Mover a posici�n {position}");
        // Implementar: Mover al jugador a la posici�n especificada
        // PlayerController.Instance.MoveToPosition(position);
    }

    private void MoveToNearestRailroad()
    {
        Debug.Log("[ACCI�N] Mover al ferrocarril m�s cercano");
        // Implementar: Mover al jugador al ferrocarril m�s cercano
    }

    private void MoveToNearestUtility()
    {
        Debug.Log("[ACCI�N] Mover al servicio p�blico m�s cercano");
        // Implementar: Mover al jugador al servicio m�s cercano
    }

    private void MoveBackward(int spaces)
    {
        Debug.Log($"[ACCI�N] Retroceder {spaces} espacios");
        // Implementar: Mover al jugador hacia atr�s
    }

    private void GoToJail()
    {
        Debug.Log("[ACCI�N] Ir a la c�rcel");
        // Implementar: Enviar al jugador a la c�rcel
        // PlayerController.Instance.GoToJail();
    }

    private void GiveMoney(int originalAmount)
    {
        int amount = GameEconomySettings.Instance.CalculateCardValue(originalAmount);
        Debug.Log($"[ACCI�N] Dar ${amount} al jugador");
        // Implementar: Dar dinero al jugador
        // PlayerController.Instance.AddMoney(amount);
    }

    private void PayMoney(int originalAmount)
    {
        int amount = GameEconomySettings.Instance.CalculateCardValue(originalAmount);
        Debug.Log($"[ACCI�N] Cobrar ${amount} al jugador");
        // Implementar: Cobrar dinero al jugador
        // PlayerController.Instance.RemoveMoney(amount);
    }

    private void PayRepairs(int originalHouseCost, int originalHotelCost)
    {
        int houseCost = GameEconomySettings.Instance.CalculateCardValue(originalHouseCost);
        int hotelCost = GameEconomySettings.Instance.CalculateCardValue(originalHotelCost);

        Debug.Log($"[ACCI�N] Pagar reparaciones: ${houseCost}/casa, ${hotelCost}/hotel");
        // Implementar: Calcular y cobrar seg�n las propiedades del jugador
        // int totalHouses = PlayerController.Instance.GetTotalHouses();
        // int totalHotels = PlayerController.Instance.GetTotalHotels();
        // int totalCost = (totalHouses * houseCost) + (totalHotels * hotelCost);
        // PlayerController.Instance.RemoveMoney(totalCost);
    }

    private void CollectFromAllPlayers(int originalAmount)
    {
        int amount = GameEconomySettings.Instance.CalculateCardValue(originalAmount);
        Debug.Log($"[ACCI�N] Cobrar ${amount} de cada jugador");
        // Implementar: Cobrar dinero de todos los dem�s jugadores
        // foreach (Player player in PhotonNetwork.PlayerListOthers)
        // {
        //     // Transferir dinero de cada jugador al actual
        // }
    }

    private void GiveGetOutOfJailCard()
    {
        Debug.Log("[ACCI�N] Dar tarjeta de Salir de la C�rcel");
        // Implementar: Dar al jugador una tarjeta de salir de la c�rcel
        // PlayerController.Instance.AddGetOutOfJailCard();
    }
}

/// <summary>
/// Tipo de mazo
/// </summary>
public enum CardDeck
{
    Chance,
    CommunityChest
}

/// <summary>
/// Tipo de tarjeta
/// </summary>
public enum CardType
{
    Money,
    Movement,
    Jail,
    GetOutOfJail,
    Repair,
    CollectFromAll
}

/// <summary>
/// Clase que representa una tarjeta
/// </summary>
[System.Serializable]
public class Card
{
    public string Description { get; private set; }
    public CardType Type { get; private set; }
    public CardDeck Deck { get; private set; }
    private System.Action action;

    public Card(string description, CardType type, CardDeck deck, System.Action action)
    {
        Description = description;
        Type = type;
        Deck = deck;
        this.action = action;
    }

    public void Execute()
    {
        action?.Invoke();
    }
}