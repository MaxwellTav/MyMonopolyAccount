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
        #region Deprecated
        "Avanza hasta la Salida. Cobra $4,000.",
        "Avanza hasta Avenida Illinois.",
        "Avanza hasta Paseo Tablado. Sin pasas por la Salida.",
        "Avanza al ferrocarril más cercano. Si está sin dueño, puedes comprarlo. Si tiene dueño, paga el doble de la renta",
        "Avanza al servicio público más cercano. Si está sin dueño, puedes comprarlo. Si tiene dueño, tira los dados y paga 10 veces la cantidad",
        "El banco te paga un dividendo de $5,000",
        "Retrocede 3 espacios",
        "Retrocede 5 espacios",
        "Retrocede 7 espacios",
        "Ve directamente a la Cárcel. No pases por la Salida. No cobres.",
        "Haz reparaciones generales en todas tus propiedades. Por cada casa paga 250. Por cada hotel paga {100}",
        "Avanza hasta Reading",
        "Has sido elegido presidente de la junta directiva. Paga a cada jugador 500",
        #endregion

        "Avance hasta el ferrocarril más cercano, y si tiene dueño\nPAGUE EL DOBLE del aquiler al dueño, si no tiene dueño puede comprarlo.",
        "Hubo un error con la compra de una parcela. El banco ha decidido que usted ha heredado la propiedad plaza Santiago, sin importar que otro jugador la tenga y seré heredada con todas sus casas u hoteles.",
        "Tírelos dados nuevamente y lo que salga en ese tiro Tendrá que pagarlo a todos los jugadores multiplicado por 250",
        "Usted se despertó Generoso, tiene que obligatoriamente. ¿Regalar 5000 pesos al jugador de la izquierda",
        "Pase directamente por avenida New York.",
        "Pase directamente por ferrocarril B & O?",
        "Retrocede hasta la propiedad más cercana y pague el triple al dueño.",
        "Hay rumoros de que usted está en una red de narcotráfico con César El abusador tiene 5 tunos sin jugar, sin cobrar GO, pague 1,000 pesos a todos los jugadores y va directamente a la cárcel",
        "Error en el banco. Este le ha dado de más pague todos los jugadores un impuesto de 1000 pesos",
        "Has intentado sobonar a los policías y esto no cogen cotorra usted va directamente a la cárcel",
        "Tiene los dados nuevamente. Y si cae algún doble, irá directamente a la cárcel si. Cae la suma de 7. Todos los jugadores tendrán que pagarles 200",
        "Usted se depertó de generoso tiene que, obligatoriamente, Regalar la propiedad más cara de su inventario con todad sus casas y hoteles.",
        "Hubo un error con la compra de una parcela. El banco ha decidido que usted ha heredado la propiedad Paseo Tablado, sin importar que otro jugador lo tenga y será heredado con sus casas u hoteles.",
        "Fin de semana. El banco cobra el impuesto 500 cada uno de los jugadores.",
        "El banco pone 5000 pesos más como acumulado en la quiniela y Pale",
        "Pague con la propiedad más barata al jugador de la derecha.",
        "Tómese un descanso de tanta casualidades.",
        "Pase directamente por la carcel de visita.",
        "Se le ha detectado fraude en su última transacción. Se ha abierto el caso Medusa en su contra tiene 5 tunos sin jugar y sin derecho a tirar dados.",
        "Usted se ha ganado la lotería.",
        "El jugador de la derecha, en la vida real, le debe una yaroa pequeña.",
        "Usted le ha robado con éxito al banco 5000 pesos. Ha sido descubierto años de pue va a la carcel.",
        "Se le ha visto robando una lavadora por la 27 de febrero y el jugador a su derecha. Le ha chibateado. Baña directamente a la cárcel.",
        "Pase directamente por el paseo tablado.",
        "Hora de la subasta.",
        "Pase directamente por el ferrocarril persilvania.",
        "El banco está de buenas y le. Pagará 5000 pesos a cada uno de los jugadores.",
        "El punto de droga ha sido descubierto. Vaya directamente a la cárcel.",
        "Pase directamente por plaza San Carlos.",
        "Tiene la posibilidad de quitar o poner La primera banca rota de un jugador, puede negociar con esta decisión. NO TE PUEDES QUEDAR CON LA BANCA ROTA.",
        "Usted ha sido elegido como presidente de la República Dominicana, cobre 2600 peso, y cada uno de los jugadores.",
        "Pase  directamente y cobre 2000 al banco.",
        "Avance hasta su ferocarri más cercano y si tiene dueño, pague el doble del alquiler. Si no tiene dueño, puede comprarlo. Avance, hasta avenida Ilinois."
    };

    // ARCA COMUNAL - 16 tarjetas
    public static List<string> CommunityChestCards = new List<string>
    {
        #region Deprecated
        "Avanza hasta la Salida. Cobra $4,000",
        "Error del banco a tu favor. Cobra $2000",
        "Honorarios del doctor. Paga $500",
        "Vendes acciones. Recibes $500",
        "Tarjeta de Salir de la Cárcel GRATIS. Esta tarjeta puede guardarse hasta que sea necesaria o venderse",
        "Ve directamente a la Cárcel. No pases por la Salida. No cobres.",
        "Ganas el segundo premio en un concurso de belleza. Cobra $100",
        "Heredas $1000",
        "Tu seguro de vida madura. Cobra $1000",
        "Gastos hospitalarios. Paga $500",
        "Gastos escolares. Paga $500",
        "Recibes una consultoría. Cobra $250",
        "Es tu cumpleaños. Cada jugador te da 1,234.56",
        "Impuesto a la renta. Paga $200",
        "Debes pagar por reparaciones en la calle: $400 por casa, $1150 por hotel",
        "Has ganado el segundo lugar en un concurso de belleza. Cobra $100",
        #endregion

        "Hay que pagar una contribución para las escuelas de 1500 peso.",
        "El COVID-19 ha acabado con su economía actual. Se le ha declarado su primera banca rota. Si ya está en su primera banca rota, el banco le pagará un dividendo de 5000 y se le quitará su banca rota actual.",
        "Usted se ha caído de 1 quinta planta y se ha roto todos los huesos del cuerpo y ha quedado vivo. Pague 5010 pesos al banco.",
        "El jugador de la izquierda, tendrá que regalarle la propiedad que este jugador decida. No será dada con la casa utele. Esta último se las lleve el banco.",
        "Restrocede este ferrocarril más cercano y páguele 300 veces a la cantidad de sus dados más el. Alquiler.",
        "Usted ha ganado el segundo premio del certamen. De belleza, cobra domil al banco.",
        "Usted está de suelte. Se ha ganado la lotería",
        "Usted ha ganado el segundo premio en el certamen de belleza cobra. 2000 al banco, pero has sobornado a los demás jugadores para ganar. Pague 700 a cada uno de los jugadores, excepto al banco.",
        "Usted ha conseguido un trato con todos los jugadores. Pague 1000 a todos los jugadores y se le permitirá poner un hotel más en una sola propiedad que usted elija. Ojo, esto no es una decisión de los demásUsted ha conseguido un trato con todos los jugadores. Pague 1000 a todos los jugadores y se le permitirá poner un hotel más en una sola propiedad que usted elija. Ojo, esto no es una decisión de los demás. Sino suya.",
        "El error del banco recibirá un impuesto de 2000 pesos.",
        "Se cumplió el plazo de los ahorros para la Navidad. Pase por GO y cobre 2000 al banco.",
        "Ha ocurrido un error con el banco tírelo. Dado y todos los jugadores tendrán que pagar la cantidad de lo dado por un múltiplo de 500.",
        "Usted ha robado con éxito a todos los jugadores un dividendo de 1500 peso, y no se pudo resolver el caso.",
        "Avance hasta un ferrocarril más cercano. Si tiene dueño, págale el dueño el doble. Si no lo tiene, no podrá comprarlo. Retroceda 5 pasos y págale el doble. Si no tiene, no podrá comprarlo.",
        "Día de reparaciones del mes en sus empresas. Pague 500 por cada casa y 1500 por cada hotel.",
        "Usted ha ganado el primer premio en el certamen de belleza cobre 5000 al banco.",
        "El banco le regalo un hotel gratis. Si no tiene propiedad en dónde ponerle, el banco le regalará la propiedad más barata que no haya sido comprada aún, para que pueda fundar su microempresa.",
        "Está de suerte. Se ha ganado la lotería"
       

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