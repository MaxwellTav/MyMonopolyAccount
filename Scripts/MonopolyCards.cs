using System.Collections.Generic;

/// <summary>
/// Contiene todas las tarjetas originales del Monopolio
/// Los valores se calcularán automáticamente con GameEconomySettings
/// </summary>
public static class MonopolyCards
{
    // CASUALIDAD - 16 tarjetas
    public static List<string> ChanceCards = new List<string>
    {
        "Avanza hasta la Salida. Cobra {SALARY}",
        "Avanza hasta Illinois Avenue",
        "Avanza hasta St. Charles Place. Si pasas por la Salida, cobra {SALARY}",
        "Avanza al ferrocarril más cercano. Si está sin dueño, puedes comprarlo. Si tiene dueño, paga el doble de la renta",
        "Avanza al ferrocarril más cercano. Si está sin dueño, puedes comprarlo. Si tiene dueño, paga el doble de la renta",
        "Avanza al servicio público más cercano. Si está sin dueño, puedes comprarlo. Si tiene dueño, tira los dados y paga 10 veces la cantidad",
        "El banco te paga un dividendo de {50}",
        "Tarjeta de Salir de la Cárcel GRATIS. Esta tarjeta puede guardarse hasta que sea necesaria o venderse",
        "Retrocede 3 espacios",
        "Ve directamente a la Cárcel. No pases por la Salida. No cobres {SALARY}",
        "Haz reparaciones generales en todas tus propiedades. Por cada casa paga {25}. Por cada hotel paga {100}",
        "Multa por exceso de velocidad: {15}",
        "Avanza hasta Reading Railroad",
        "Avanza hasta Boardwalk",
        "Has sido elegido presidente de la junta directiva. Paga a cada jugador {50}",
        "Tu préstamo y construcción madura. Recibes {150}"
    };

    // ARCA COMUNAL - 16 tarjetas
    public static List<string> CommunityChestCards = new List<string>
    {
        "Avanza hasta la Salida. Cobra {SALARY}",
        "Error del banco a tu favor. Cobra {200}",
        "Honorarios del doctor. Paga {50}",
        "Vendes acciones. Recibes {50}",
        "Tarjeta de Salir de la Cárcel GRATIS. Esta tarjeta puede guardarse hasta que sea necesaria o venderse",
        "Ve directamente a la Cárcel. No pases por la Salida. No cobres {SALARY}",
        "Ganas el segundo premio en un concurso de belleza. Cobra {10}",
        "Heredas {100}",
        "Tu seguro de vida madura. Cobra {100}",
        "Gastos hospitalarios. Paga {50}",
        "Gastos escolares. Paga {50}",
        "Recibes una consultoría. Cobra {25}",
        "Es tu cumpleaños. Cada jugador te da {10}",
        "Impuesto a la renta. Paga {200}",
        "Debes pagar por reparaciones en la calle: {40} por casa, {115} por hotel",
        "Has ganado el segundo lugar en un concurso de belleza. Cobra {10}"
    };

    /// <summary>
    /// Reemplaza los valores en las tarjetas con los valores escalados
    /// </summary>
    public static string ProcessCard(string cardText)
    {
        if (GameEconomySettings.Instance == null)
            return cardText;

        // Reemplazar {SALARY} con el salario
        cardText = cardText.Replace("{SALARY}", FormatMoney(GameEconomySettings.Instance.GetSalaryAmount()));

        // Reemplazar valores específicos
        cardText = ReplaceValue(cardText, "{200}", 200);
        cardText = ReplaceValue(cardText, "{150}", 150);
        cardText = ReplaceValue(cardText, "{100}", 100);
        cardText = ReplaceValue(cardText, "{115}", 115);
        cardText = ReplaceValue(cardText, "{50}", 50);
        cardText = ReplaceValue(cardText, "{40}", 40);
        cardText = ReplaceValue(cardText, "{25}", 25);
        cardText = ReplaceValue(cardText, "{15}", 15);
        cardText = ReplaceValue(cardText, "{10}", 10);

        return cardText;
    }

    private static string ReplaceValue(string text, string placeholder, int originalValue)
    {
        if (!text.Contains(placeholder))
            return text;

        int scaledValue = GameEconomySettings.Instance.CalculateCardValue(originalValue);
        return text.Replace(placeholder, FormatMoney(scaledValue));
    }

    /// <summary>
    /// Formatea dinero como $0,000.00
    /// </summary>
    public static string FormatMoney(int amount)
    {
        int dollars = amount / 100;
        int cents = amount % 100;
        return $"${dollars:N0}<size=20>.{cents:00}</size>";
    }
}