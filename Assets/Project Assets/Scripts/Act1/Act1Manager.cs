using System.Collections;
using UnityEngine;
using Yarn.Unity;

// Orquesta el Acto 1. El TEXTO y el FLUJO de diálogo ahora los maneja
// Yarn Spinner (nodo "Acto1_Entrega" en Acto1.yarn). Este script expone
// los YarnCommands que Yarn necesita para reproducir audio y para medir
// la mirada del jugador, y sigue siendo dueño de la lógica que no es
// diálogo: la ventana de reintento y el registro en EventLogger.

public class Act1Manager : MonoBehaviour
{
    [Header("Yarn Spinner")]
    public DialogueRunner dialogueRunner;

    [Header("Referencias")]
    public Transform profesorMirada;   // punto vacío a la altura de la cabeza del NPC profesor
    public Camera playerCamera;
    public GameObject notaObjeto;      // la nota de retroalimentación sobre la mesa (empieza inactiva)

    [Header("Audio del profesor (referenciados por nombre desde el .yarn)")]
    public AudioSource profesorAudioSource;
    public AudioClip clipSaludo;              // "Buenas. Déjalo acá."
    public AudioClip clipRevision;            // "A ver... mmm."
    public AudioClip clipBuenaBase;           // "Tiene buena base, pero le falta trabajo todavía."
    public AudioClip clipFeedbackDetallado;   // "Esto no está mal... la parte de ensamblaje no cierra bien."
    public AudioClip clipCriterio;            // "Te lo dejo a tu criterio..."
    public AudioClip clipTomateElTiempo;      // "Tómate el tiempo que necesites."
    public AudioClip clipBienSigueAsi;        // Ruta A: "Bien. Eso está mejor."

    [Header("Audio del compañero de fondo (Fase 1E)")]
    public AudioSource companeroAudioSource;
    public AudioClip lineaCompanero;
    public float tiempoLineaCompanero = 10f;

    [Header("Configuración de medición")]
    public float ventanaMirada = 2.5f;
    public float ventanaReintento = 30f;

    private bool entregaRealizada = false;
    private bool decisionTomada = false;

    // Llamado por MesaEntregasTrigger.Entregar() — sin cambios respecto a antes.
    public void OnEntregaIniciada()
    {
        if (entregaRealizada) return;
        entregaRealizada = true;

        if (notaObjeto != null) notaObjeto.SetActive(true);

        dialogueRunner.StartDialogue("Acto1_Entrega");
    }

    // Comando de Yarn: <<reproducir_linea "nombre_clip">>
    // Al ser un IEnumerator, Yarn Spinner ESPERA a que termine antes de
    // mostrar la siguiente línea — así el texto queda sincronizado con el audio.
    [YarnCommand("reproducir_linea")]
    public IEnumerator ReproducirLinea(string nombreClip)
    {
        AudioClip clip = ObtenerClipPorNombre(nombreClip);

        if (profesorAudioSource != null && clip != null)
        {
            profesorAudioSource.clip = clip;
            profesorAudioSource.Play();
            yield return new WaitForSeconds(clip.length);
        }
        else
        {
            yield return new WaitForSeconds(1.5f); // respaldo si todavía no grabaste ese clip
        }
    }

    AudioClip ObtenerClipPorNombre(string nombre)
    {
        switch (nombre)
        {
            case "saludo": return clipSaludo;
            case "revision": return clipRevision;
            case "buena_base": return clipBuenaBase;
            case "feedback_detallado": return clipFeedbackDetallado;
            case "criterio": return clipCriterio;
            case "tomate_tiempo": return clipTomateElTiempo;
            case "bien_sigue_asi": return clipBienSigueAsi;
            default: return null;
        }
    }

    // Comando de Yarn: <<medir_mirada>>
    // Reemplaza al "-> [esperar pausa para raycast]" que escribió GPT (esa
    // sintaxis no existe en Yarn; "->" es para opciones de diálogo, no pausas).
    [YarnCommand("medir_mirada")]
    public IEnumerator MedirMirada()
    {
        float tiempoMirando = 0f;
        float t = 0f;

        while (t < ventanaMirada)
        {
            if (MiraAlProfesor()) tiempoMirando += Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

        float porcentajeMirada = (tiempoMirando / ventanaMirada) * 100f;
        string tendencia = porcentajeMirada >= 50f ? "fortaleza" : "debilidad";

        EventLogger.Instance.RegistrarEvento(
            competencia: "Autorregulación emocional (control emocional)",
            situacion: "El profesor explica su retroalimentación frente al jugador",
            comportamiento: $"Sostuvo la mirada el {porcentajeMirada:F0}% del tiempo",
            tiempo: ventanaMirada,
            tendencia: tendencia
        );
    }

    bool MiraAlProfesor()
    {
        if (profesorMirada == null || playerCamera == null) return false;

        Vector3 direccionAlNPC = (profesorMirada.position - playerCamera.transform.position).normalized;
        float angulo = Vector3.Angle(playerCamera.transform.forward, direccionAlNPC);

        return angulo < 25f;
    }

    // Comando de Yarn: <<iniciar_ventana_decision>>
    // Se llama al final del nodo, cuando el profesor termina de hablar.
    [YarnCommand("iniciar_ventana_decision")]
    public void IniciarVentanaDecision()
    {
        StartCoroutine(VentanaDeDecision());
    }

    IEnumerator VentanaDeDecision()
    {
        bool lineaCompaneroReproducida = false;
        float t = 0f;

        while (t < ventanaReintento && !decisionTomada)
        {
            if (!lineaCompaneroReproducida && t >= tiempoLineaCompanero)
            {
                lineaCompaneroReproducida = true;
                if (companeroAudioSource != null && lineaCompanero != null)
                {
                    companeroAudioSource.clip = lineaCompanero;
                    companeroAudioSource.Play();
                }
            }
            t += Time.deltaTime;
            yield return null;
        }

        if (!decisionTomada)
        {
            RegistrarRutaB(t);
        }
    }

    // Llamado por MesaEntregasTrigger.Entregar() cuando el proyecto vuelve a la mesa (Ruta A)
    public void OnReintentoCompletado(float tiempoTranscurrido)
    {
        if (decisionTomada) return;
        decisionTomada = true;

        EventLogger.Instance.RegistrarEvento(
            competencia: "Autorregulación emocional (persistencia)",
            situacion: "Recibió retroalimentación negativa en su primera entrega",
            comportamiento: "Retomó el proyecto, lo corrigió y volvió a entregarlo",
            tiempo: tiempoTranscurrido,
            tendencia: "fortaleza"
        );

        StartCoroutine(ReproducirLinea("bien_sigue_asi"));
    }

    void RegistrarRutaB(float tiempoTranscurrido)
    {
        decisionTomada = true;

        EventLogger.Instance.RegistrarEvento(
            competencia: "Autorregulación emocional (persistencia)",
            situacion: "Recibió retroalimentación negativa en su primera entrega",
            comportamiento: "No retomó el proyecto dentro del tiempo disponible",
            tiempo: tiempoTranscurrido,
            tendencia: "debilidad"
        );
    }
}