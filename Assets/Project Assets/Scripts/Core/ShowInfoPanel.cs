using UnityEngine;

// Conecta esto al "On Interact" de un InteractableObject (laptop, hoja
// de apuntes, nota de retroalimentación) para mostrar un panel de UI
// con el texto correspondiente. Se oculta solo después de unos segundos.

public class ShowInfoPanel : MonoBehaviour
{
    public GameObject panel;
    public float autoHideAfter = 4f;

    public void Show()
    {
        if (panel == null) return;
        panel.SetActive(true);
        CancelInvoke();
        if (autoHideAfter > 0)
        {
            Invoke(nameof(Hide), autoHideAfter);
        }
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}