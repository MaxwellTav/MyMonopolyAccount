using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Controla el MessageBox (LeanWindow) para mostrar las tarjetas
/// </summary>
public class MessageBoxController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text messageText;

    /// <summary>
    /// Muestra un mensaje en el MessageBox
    /// </summary>
    public void ShowMessage(string title, string message)
    {
        if (titleText != null)
            titleText.text = title;

        if (messageText != null)
            messageText.text = message;

        Debug.Log($"[{title}] {message}");
    }
}