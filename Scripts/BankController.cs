using UnityEngine;
using Lean.Gui;

/// <summary>
/// Controla los botones del panel del banco
/// </summary>
public class BankController : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private LeanButton casualidadButton;
    [SerializeField] private LeanButton arcaComunalButton;

    [Header("MessageBox")]
    [SerializeField] private LeanWindow messageBox;

    private void Start()
    {
        if (casualidadButton != null)
            casualidadButton.OnClick.AddListener(OnCasualidadClicked);

        if (arcaComunalButton != null)
            arcaComunalButton.OnClick.AddListener(OnArcaComunalClicked);
    }

    private void OnCasualidadClicked()
    {
        string card = CardDrawer.Instance.DrawChanceCard();
        ShowCard("Casualidad", card);
    }

    private void OnArcaComunalClicked()
    {
        string card = CardDrawer.Instance.DrawCommunityCard();
        ShowCard("Arca Comunal", card);
    }

    private void ShowCard(string title, string message)
    {
        if (messageBox != null)
        {
            MessageBoxController msgBox = messageBox.GetComponent<MessageBoxController>();
            if (msgBox != null)
            {
                msgBox.ShowMessage(title, message);
                messageBox.TurnOn();
            }
        }
    }
}