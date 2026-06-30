using UnityEngine;

public class SnapPointHDDTrigger : MonoBehaviour
{
    public Act1Manager act1Manager;
    public PlayerInteractor playerInteractor;
    public Transform snapPoint;
    public float distanciaMinimaJugador = 2f;
    public bool activo = false;

    private bool yaColocado = false;

    void OnTriggerEnter(Collider other)
    {
        if (!activo) return;
        if (yaColocado) return;
        if (!other.CompareTag("Proyecto")) return;

        GameObject proyecto = other.gameObject;

        if (playerInteractor == null || playerInteractor.GetHeldObject() != proyecto)
            return;

        if (Vector3.Distance(playerInteractor.transform.position, transform.position) > distanciaMinimaJugador)
            return;

        yaColocado = true;

        playerInteractor.Drop();

        if (snapPoint != null)
        {
            proyecto.transform.SetParent(snapPoint);
            proyecto.transform.localPosition = Vector3.zero;
            proyecto.transform.localRotation = Quaternion.identity;
        }

        if (proyecto.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        act1Manager.OnProyectoEnHDD(proyecto);
    }
}
