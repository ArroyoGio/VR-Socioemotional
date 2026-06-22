using UnityEngine;
using Yarn.Unity;

// Va en tu empty GameObject que tiene el BoxCollider (Is Trigger = true).
// Detecta cuando el jugador entra cargando el proyecto y dispara
// el nodo "Acto1_Aproximacion" de Yarn Spinner.
// Es independiente de Act1Manager — no necesita referencia a él.

public class ZonaAproximacionMesa : MonoBehaviour
{
    [Header("Referencias")]
    public DialogueRunner dialogueRunner;
    public PlayerInteractor playerInteractor;

    private bool disparado = false;

    void OnTriggerEnter(Collider other)
    {
        if (disparado) return;

        // Solo reacciona si quien entra es el jugador
        if (!other.CompareTag("Player")) return;

        // Solo dispara si el jugador está cargando el proyecto
        GameObject held = playerInteractor.GetHeldObject();
        if (held == null || !held.CompareTag("Proyecto")) return;

        disparado = true;
        dialogueRunner.StartDialogue("Acto1_Aproximacion");
    }
}