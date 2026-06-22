using System.Collections.Generic;
using UnityEngine;

// Zona de entrega de la mesa. NO usa ningún Collider ni Trigger — se
// registra en una lista estática propia, y PlayerInteractor la consulta
// directamente por distancia SOLO en el instante en que el jugador
// presiona E. Cero dependencia de física, cero riesgo de tunneling,
// funciona igual caminando, girando rápido, o con teletransporte en VR.
//
// Esta clase NUNCA decide por sí sola entregar algo — solo responde
// "¿puedo recibir esto ahora?" (PuedeEntregar) y, si PlayerInteractor
// confirma, ejecuta la entrega (Entregar). El jugador es quien decide.

public class MesaEntregasTrigger : MonoBehaviour
{
    private static readonly List<MesaEntregasTrigger> zonasActivas = new List<MesaEntregasTrigger>();

    [Header("Referencias")]
    public Act1Manager act1Manager;
    public Transform snapPoint; // dónde se acomoda visualmente el proyecto sobre la mesa

    [Header("Configuración")]
    public float radioDeteccion = 1f; // qué tan cerca debe estar el proyecto del SnapPoint

    private bool primeraEntrega = false;
    private float tiempoInicioVentana;

    void OnEnable() => zonasActivas.Add(this);
    void OnDisable() => zonasActivas.Remove(this);

    // Llamado por PlayerInteractor: ¿esta zona acepta el objeto ahora mismo?
    public bool PuedeEntregar(GameObject objeto)
    {
        if (objeto == null || !objeto.CompareTag("Proyecto")) return false;
        if (snapPoint == null) return false;

        float distancia = Vector3.Distance(objeto.transform.position, snapPoint.position);
        return distancia <= radioDeteccion;
    }

    // Llamado por PlayerInteractor SOLO después de confirmar PuedeEntregar().
    // Coloca el objeto en el SnapPoint y registra el resultado en el Acto 1.
    public void Entregar(GameObject objeto)
    {
        objeto.transform.SetParent(snapPoint);
        objeto.transform.localPosition = Vector3.zero;
        objeto.transform.localRotation = Quaternion.identity;

        if (objeto.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true; // se queda fijo, sin gravedad, hasta que se vuelva a agarrar
        }

        if (!primeraEntrega)
        {
            primeraEntrega = true;
            tiempoInicioVentana = Time.time;
            act1Manager.OnEntregaIniciada();
        }
        else
        {
            float tiempoTranscurrido = Time.time - tiempoInicioVentana;
            act1Manager.OnReintentoCompletado(tiempoTranscurrido);
        }
    }

    // Busca entre todas las zonas activas de la escena si alguna acepta
    // este objeto en este momento. Por eso PlayerInteractor no necesita
    // una referencia directa a ninguna mesa específica.
    public static MesaEntregasTrigger BuscarZonaParaEntregar(GameObject objeto)
    {
        foreach (MesaEntregasTrigger zona in zonasActivas)
        {
            if (zona.PuedeEntregar(objeto)) return zona;
        }
        return null;
    }
}