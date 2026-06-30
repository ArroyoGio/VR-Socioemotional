using UnityEngine;
using UnityEngine.Events;

// Pon este componente en CUALQUIER objeto interactuable: laptop, hoja de
// apuntes, el proyecto, la nota de retroalimentaci�n, etc.
// Conecta "On Interact" desde el Inspector a lo que deba pasar
// (mostrar un panel, activar una animaci�n, recogerlo, etc.)
// No necesitas escribir c�digo nuevo por cada objeto.

public class InteractableObject : MonoBehaviour
{
    [Header("Configuraci�n")]
    public string promptText = "Presiona E para interactuar";
    public bool canInteract = true;
    public bool interactableOnce = false;
    public bool isCargable = false; // si es true, se recoge autom�ticamente al interactuar
    // (usamos este bool en vez de un segundo Tag, porque Unity solo permite un Tag por objeto)

    [Header("Eventos")]
    public UnityEvent onInteract;

    private bool alreadyUsed = false;

    public void Interact()
    {
        if (!canInteract) return;
        if (interactableOnce && alreadyUsed) return;
        alreadyUsed = true;
        onInteract?.Invoke();
    }
}