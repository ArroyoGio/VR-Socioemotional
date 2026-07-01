using System.Collections.Generic;
using UnityEngine;

public class MesaEntregasTrigger : MonoBehaviour
{
    private static readonly List<MesaEntregasTrigger> zonasActivas = new List<MesaEntregasTrigger>();

    [Header("Referencias")]
    public Act1Manager act1Manager;
    public PlayerInteractor playerInteractor;
    public Transform snapPoint;

    [Header("Configuraci�n")]
    public float radioEntrega = 0.6f;       // distancia para snap autom�tico al presionar E
    public float radioAproximacion = 2.5f;  // distancia para disparar el primer di�logo

    private bool primeraEntrega = false;
    private bool aproximacionAvisada = false;
    private float tiempoInicioVentana;

    void OnEnable() => zonasActivas.Add(this);
    void OnDisable() => zonasActivas.Remove(this);

    void Update()
    {
        if (playerInteractor == null || act1Manager == null) return;

        GameObject proyecto = playerInteractor.GetHeldObject();
        if (proyecto == null || !proyecto.CompareTag("Proyecto")) return;

        // Avisa al Act1Manager la posici�n del proyecto para que mida proximidad
        if (!aproximacionAvisada)
        {
            act1Manager.SetProyectoTransform(proyecto.transform);
        }
    }

    public bool PuedeEntregar(GameObject objeto)
    {
        if (objeto == null || !objeto.CompareTag("Proyecto")) return false;
        if (snapPoint == null) return false;

        float distancia = Vector3.Distance(objeto.transform.position, snapPoint.position);
        return distancia <= radioEntrega;
    }

    public void Entregar(GameObject objeto)
    {
        if (!primeraEntrega)
        {
            primeraEntrega = true;
            tiempoInicioVentana = Time.time;

            objeto.transform.SetParent(snapPoint);
            objeto.transform.localPosition = Vector3.zero;
            objeto.transform.localRotation = Quaternion.identity;

            if (objeto.TryGetComponent(out Rigidbody rb))
                rb.isKinematic = true;

            act1Manager.OnEntregaIniciada();
        }
        else
        {
            objeto.transform.SetParent(snapPoint);
            objeto.transform.localPosition = Vector3.zero;
            objeto.transform.localRotation = Quaternion.identity;

            if (objeto.TryGetComponent(out Rigidbody rb))
                rb.isKinematic = true;

            // Solo cuenta como reintento si pas� por el HDD
            if (!act1Manager.PuedeReintentar())
                return;

            float tiempoTranscurrido = Time.time - tiempoInicioVentana;
            act1Manager.OnReintentoCompletado(tiempoTranscurrido);
        }
    }

    public static MesaEntregasTrigger BuscarZonaParaEntregar(GameObject objeto)
    {
        foreach (MesaEntregasTrigger zona in zonasActivas)
        {
            if (zona.PuedeEntregar(objeto)) return zona;
        }
        return null;
    }
}