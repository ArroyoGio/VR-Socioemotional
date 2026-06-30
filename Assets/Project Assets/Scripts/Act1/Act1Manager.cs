using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class Act1Manager : MonoBehaviour
{
    public ProfesorNavMeshMover profesorNavMeshMover;

    [Header("Yarn Spinner")]
    public DialogueRunner dialogueRunner;

    [Header("Referencias")]
    public Transform profesorMirada;
    public Camera playerCamera;
    public GameObject notaObjeto;          // empieza INACTIVO en el Inspector
    public Animator profesorAnimator;      // opcional � para animaci�n de retirarse

    [Header("Clips de audio del profesor")]
    public AudioSource profesorAudioSource;
    public AudioClip clipAproximacion;       // "Buenas. �Ese es tu avance?"
    public AudioClip clipRevisionSilencio;   // sonido ambiente / pausa mientras revisa
    public AudioClip clipBuenaBase;          // "A ver... mmm. Tiene buena base..."
    public AudioClip clipFeedbackDetallado;  // "La parte de ensamblaje..."
    public AudioClip clipCriterio;           // "M�ralo, te dej� algunos comentarios..."
    public AudioClip clipTomateElTiempo;     // "T�mate el tiempo que necesites."
    public AudioClip clipBienSigueAsi;       // Ruta A: "Bien. Eso est� mejor."

    [Header("Compa�ero de fondo (Fase 1E)")]
    public AudioSource companeroAudioSource;
    public AudioClip clipCompanero;
    public float tiempoLineaCompanero = 10f;

    [Header("Post-entrega")]
    public PlayerController playerController;
    public InteractableObject laptopInteractable;

    [Header("Configuraci�n")]
    public float ventanaMirada = 2.5f;
    public float ventanaReintento = 30f;
    public float radioAproximacion = 2f;   // distancia para disparar el di�logo de aproximaci�n

    private bool aproximacionDisparada = false;
    private bool entregaRealizada = false;
    private bool decisionTomada = false;
    private Transform proyectoTransform;   // para medir la distancia de aproximaci�n

    // MesaEntregasTrigger llama esto al detectar que el jugador est� cerca
    // con el proyecto en mano (ANTES de soltar)
    public void SetProyectoTransform(Transform t) => proyectoTransform = t;

    void Update()
    {
        // Detecta proximidad del proyecto para disparar el primer di�logo
        if (!aproximacionDisparada && proyectoTransform != null)
        {
            float dist = Vector3.Distance(proyectoTransform.position, transform.position);
            if (dist <= radioAproximacion)
            {
                aproximacionDisparada = true;
                dialogueRunner.StartDialogue("Acto1_Aproximacion");
            }
        }
    }

    // MesaEntregasTrigger.Entregar() llama esto cuando el proyecto llega al SnapPoint
    public void OnEntregaIniciada()
    {
        if (entregaRealizada) return;

        entregaRealizada = true;
        StartCoroutine(DialogoEntregaConNota());
    }

    IEnumerator DialogoEntregaConNota()
    {
        dialogueRunner.StartDialogue("Acto1_Entrega");

        yield return new WaitForSeconds(6f);

        if (notaObjeto != null)
        {
            notaObjeto.SetActive(true);
            Debug.Log("Nota mostrada despu�s del di�logo inicial.");
        }

        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);

        if (playerController != null)
            playerController.enabled = false;

        yield return new WaitForSeconds(3f);

        dialogueRunner.StartDialogue("Acto1_Reaccion");

        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);

        if (laptopInteractable != null)
            laptopInteractable.canInteract = true;

        if (playerController != null)
            playerController.enabled = true;

        Debug.Log("Iniciando ventana de decisi�n desde c�digo.");
        IniciarVentanaDecision();
    }

    // Llamado por MesaEntregasTrigger cuando el jugador reintenta (Ruta A)
    public void OnReintentoCompletado(float tiempoTranscurrido)
    {
        if (decisionTomada) return;
        decisionTomada = true;

        EventLogger.Instance.RegistrarEvento(
            competencia: "Autorregulaci�n emocional (persistencia)",
            situacion: "Recibi� retroalimentaci�n negativa en su primera entrega",
            comportamiento: "Retom� el proyecto, lo corrigi� y volvi� a entregarlo",
            tiempo: tiempoTranscurrido,
            tendencia: "fortaleza"
        );

        dialogueRunner.StartDialogue("Acto1_Reintento");

        GameSessionData.Reintento = true;
        GameSessionData.TiempoReintento = tiempoTranscurrido;
        ReporteManager.Instance.GuardarReporte();
    }

    // ??? YARN COMMANDS ???????????????????????????????????????????????????????

    // <<reproducir_linea "nombre_clip">>
    // Yarn espera a que el audio termine antes de seguir con la siguiente l�nea
    [YarnCommand("reproducir_linea")]
    public IEnumerator ReproducirLinea(string nombreClip)
    {
        AudioClip clip = ObtenerClipPorNombre(nombreClip);

        if (profesorAudioSource != null && clip != null)
        {
            profesorAudioSource.clip = clip;
            profesorAudioSource.Play();
            yield return new WaitForSeconds(clip.length + 0.2f); // peque�o respiro entre l�neas
        }
        else
        {
            yield return new WaitForSeconds(1.8f); // respaldo mientras no hay audio grabado
        }
    }

    // <<mostrar_nota>>
    // La nota aparece EXACTAMENTE aqu�, no al inicio del di�logo
    [YarnCommand("mostrar_nota")]
    public void MostrarNota()
    {
        if (notaObjeto != null) notaObjeto.SetActive(true);
    }
    // <<medir_mirada>>
    // Mide si el jugador sostiene contacto visual con el profesor
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

        float porcentaje = (tiempoMirando / ventanaMirada) * 100f;
        string tendencia = porcentaje >= 50f ? "fortaleza" : "debilidad";

        EventLogger.Instance.RegistrarEvento(
            competencia: "Autorregulaci�n emocional (control emocional)",
            situacion: "El profesor explica su retroalimentaci�n frente al jugador",
            comportamiento: $"Sostuvo la mirada el {porcentaje:F0}% del tiempo",
            tiempo: ventanaMirada,
            tendencia: tendencia
        );
        GameSessionData.PorcentajeMirada = porcentaje;
    }

    // <<profesor_retirarse>>
    // Activa animaci�n de caminar hacia el escritorio (opcional)
    [YarnCommand("profesor_retirarse")]
    public void ProfesorRetirarse()
    {
        if (profesorAnimator != null)
            profesorAnimator.SetTrigger("Retirarse");
    }

    // <<iniciar_ventana_decision>>
    // Arranca el timer de 30s y el compa�ero de fondo
    [YarnCommand("iniciar_ventana_decision")]
    public void IniciarVentanaDecision()
    {
        StartCoroutine(VentanaDeDecision());
    }

    // ??? M�TODOS INTERNOS ????????????????????????????????????????????????????

    IEnumerator VentanaDeDecision()
    {
        bool lineaReproducida = false;
        float t = 0f;

        while (t < ventanaReintento && !decisionTomada)
        {
            if (!lineaReproducida && t >= tiempoLineaCompanero)
            {
                lineaReproducida = true;
                if (companeroAudioSource != null && clipCompanero != null)
                {
                    companeroAudioSource.clip = clipCompanero;
                    companeroAudioSource.Play();
                }
            }
            t += Time.deltaTime;
            yield return null;
        }

        if (!decisionTomada) RegistrarRutaB(t);
    }

    void RegistrarRutaB(float tiempoTranscurrido)
    {
        decisionTomada = true;

        Debug.Log("RUTA B: El jugador NO volvi� a intentar.");
        Debug.Log("Tiempo transcurrido: " + tiempoTranscurrido + " segundos.");
        Debug.Log("Competencia: Autorregulaci�n emocional (persistencia)");
        Debug.Log("Tendencia: debilidad");

        EventLogger.Instance.RegistrarEvento(
            competencia: "Autorregulaci�n emocional (persistencia)",
            situacion: "Recibi� retroalimentaci�n negativa en su primera entrega",
            comportamiento: "No retom� el proyecto dentro del tiempo disponible",
            tiempo: tiempoTranscurrido,
            tendencia: "debilidad"
        );

        if (profesorNavMeshMover != null)
        {
            profesorNavMeshMover.Retirarse();
        }
        GameSessionData.Reintento = false;
        GameSessionData.TiempoReintento = tiempoTranscurrido;
        ReporteManager.Instance.GuardarReporte();
    }

    bool MiraAlProfesor()
    {
        if (profesorMirada == null || playerCamera == null) return false;
        Vector3 dir = (profesorMirada.position - playerCamera.transform.position).normalized;
        return Vector3.Angle(playerCamera.transform.forward, dir) < 25f;
    }

    AudioClip ObtenerClipPorNombre(string nombre)
    {
        switch (nombre)
        {
            case "aproximacion":         return clipAproximacion;
            case "revision_silencio":    return clipRevisionSilencio;
            case "buena_base":           return clipBuenaBase;
            case "feedback_detallado":   return clipFeedbackDetallado;
            case "criterio":             return clipCriterio;
            case "tomate_tiempo":        return clipTomateElTiempo;
            case "bien_sigue_asi":       return clipBienSigueAsi;
            default:                     return null;
        }
    }
}