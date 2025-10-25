using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// Gestiona la escena del juego: muestra BankPanel o PlayerPanel según el rol
/// </summary>
public class GameSceneManager : MonoBehaviourPun
{
    [Header("Panels")]
    [SerializeField] private GameObject bankPanel;
    [SerializeField] private GameObject playerPanel;

    private void Start()
    {
        SetupPanels();
    }

    private void SetupPanels()
    {
        bool isMaster = PhotonNetwork.IsMasterClient;

        if (bankPanel != null)
            bankPanel.SetActive(isMaster);

        if (playerPanel != null)
            playerPanel.SetActive(!isMaster);

        Debug.Log($"Panel activo: {(isMaster ? "Bank" : "Player")}");
    }

    //public override void OnMasterClientSwitched(Player newMasterClient)
    //{
    //    // Si cambia el host, reconfigurar panels
    //    SetupPanels();
    //}
}