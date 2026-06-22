using UnityEngine;
using Yarn.Unity;

public class TriggerIntroPuerta : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public MonoBehaviour playerController;
    public float duracionIntro = 25f;

    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado) return;
        if (!other.CompareTag("Player")) return;

        activado = true;

        playerController.enabled = false;
        dialogueRunner.StartDialogue("Acto1_Intro");

        Invoke(nameof(DesbloquearMovimiento), duracionIntro);
    }

    private void DesbloquearMovimiento()
    {
        playerController.enabled = true;
    }
}