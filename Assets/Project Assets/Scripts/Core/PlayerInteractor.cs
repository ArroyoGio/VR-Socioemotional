using UnityEngine;

// Va en el GameObject del jugador. Detecta hacia qu� objeto interactuable
// est� mirando (raycast desde la c�mara) y es la �NICA autoridad sobre
// qu� hace la tecla E: recoger, soltar, o entregar.
//
// IMPORTANTE: este script NO conoce a MesaEntregasTrigger por una
// referencia directa � le pregunta a la lista est�tica de zonas activas
// si alguna acepta el objeto que carga. As� PlayerInteractor sigue siendo
// gen�rico y reutilizable para cualquier zona de entrega futura (Acto 2,
// Acto 3, etc.) sin necesitar m�s wiring en el Inspector.

public class PlayerInteractor : MonoBehaviour
{
    [Header("Referencias")]
    public Camera playerCamera;
    public Transform holdPoint; // punto frente a la c�mara donde "flota" el objeto cargado

    [Header("Configuraci�n")]
    public float interactRange = 3f;
    public LayerMask interactableLayer;

    private InteractableObject currentTarget;
    private GameObject heldObject;

    void Update()
    {
        DetectInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject != null)
            {
                HandleEntregaOSuelta();
            }
            else if (currentTarget != null)
            {
                currentTarget.Interact();

                if (currentTarget.isCargable)
                {
                    PickUp(currentTarget.gameObject);
                }
            }
        }
    }

    void HandleEntregaOSuelta()
    {
        // Pregunta a TODAS las zonas de entrega activas si alguna acepta
        // el objeto en este instante. Esto se eval�a solo aqu�, una vez,
        // nunca en un Update() continuo � por eso no hay entrega autom�tica.
        MesaEntregasTrigger zona = MesaEntregasTrigger.BuscarZonaParaEntregar(heldObject);

        if (zona != null)
        {
            GameObject objetoEntregado = heldObject;
            heldObject = null; // la zona toma posesi�n del objeto
            zona.Entregar(objetoEntregado);
        }
        else
        {
            Drop();
        }
    }

    void DetectInteractable()
    {
        currentTarget = null;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            InteractableObject obj = hit.collider.GetComponent<InteractableObject>();
            if (obj != null && obj.canInteract)
                currentTarget = obj;
        }
    }

    public void PickUp(GameObject obj)
    {
        heldObject = obj;
        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        if (obj.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
    }

    public void Drop()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);
        if (heldObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }
        heldObject = null;
    }

    public GameObject GetHeldObject() => heldObject;
    public string GetCurrentPromptText() => currentTarget != null ? currentTarget.promptText : null;
}