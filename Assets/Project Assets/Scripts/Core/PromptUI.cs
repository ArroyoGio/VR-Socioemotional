using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Muestra/oculta el texto de interacción en pantalla según lo que el
// jugador esté mirando. Va en el Canvas, junto al panel del prompt.

public class PromptUI : MonoBehaviour
{
    public PlayerInteractor playerInteractor;
    public GameObject promptPanel; // el objeto que se muestra/oculta (puede ser solo el Text)
    public TMP_Text promptText;     // UI > Text (Legacy). Si usas TextMeshPro, cambia el tipo a TMP_Text.

    void Update()
    {
        string texto = playerInteractor.GetCurrentPromptText();

        if (!string.IsNullOrEmpty(texto))
        {
            promptPanel.SetActive(true);
            promptText.text = texto;
        }
        else
        {
            promptPanel.SetActive(false);
        }
    }
}