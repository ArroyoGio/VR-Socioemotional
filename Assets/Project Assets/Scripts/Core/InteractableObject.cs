using UnityEngine;
using UnityEngine.Events;

// Pon este componente en CUALQUIER objeto interactuable: laptop, hoja de
// apuntes, el proyecto, la nota de retroalimentación, etc.
// Conecta "On Interact" desde el Inspector a lo que deba pasar
// (mostrar un panel, activar una animación, recogerlo, etc.)
// No necesitas escribir código nuevo por cada objeto.

public class InteractableObject : MonoBehaviour
{
    [Header("Configuración")]
    public string promptText = "Presiona E para interactuar";
    public bool interactableOnce = false;
    public bool isCargable = false; // si es true, se recoge automáticamente al interactuar
    // (usamos este bool en vez de un segundo Tag, porque Unity solo permite un Tag por objeto)

    [Header("Eventos")]
    public UnityEvent onInteract;

    private bool alreadyUsed = false;

    public void Interact()
    {
        if (interactableOnce && alreadyUsed) return;
        alreadyUsed = true;
        onInteract?.Invoke();
    }
}