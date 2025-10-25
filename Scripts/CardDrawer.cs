using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Saca cartas al azar de Casualidad y Arca Comunal
/// </summary>
public class CardDrawer : MonoBehaviour
{
    public static CardDrawer Instance { get; private set; }

    private List<string> chanceDeck = new List<string>();
    private List<string> communityDeck = new List<string>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeDecks();
    }

    private void InitializeDecks()
    {
        chanceDeck = new List<string>(MonopolyCards.ChanceCards);
        communityDeck = new List<string>(MonopolyCards.CommunityChestCards);

        ShuffleDecks();
    }

    private void ShuffleDecks()
    {
        chanceDeck = chanceDeck.OrderBy(x => Random.value).ToList();
        communityDeck = communityDeck.OrderBy(x => Random.value).ToList();

        Debug.Log("Mazos barajados");
    }

    /// <summary>
    /// Saca una carta de Casualidad
    /// </summary>
    public string DrawChanceCard()
    {
        if (chanceDeck.Count == 0)
        {
            chanceDeck = new List<string>(MonopolyCards.ChanceCards);
            chanceDeck = chanceDeck.OrderBy(x => Random.value).ToList();
        }

        string card = chanceDeck[0];
        chanceDeck.RemoveAt(0);

        return MonopolyCards.ProcessCard(card);
    }

    /// <summary>
    /// Saca una carta de Arca Comunal
    /// </summary>
    public string DrawCommunityCard()
    {
        if (communityDeck.Count == 0)
        {
            communityDeck = new List<string>(MonopolyCards.CommunityChestCards);
            communityDeck = communityDeck.OrderBy(x => Random.value).ToList();
        }

        string card = communityDeck[0];
        communityDeck.RemoveAt(0);

        return MonopolyCards.ProcessCard(card);
    }
}