using System.Collections;
using UnityEngine;
using Yarn.Unity;
using TMPro;

public class Act1Manager : MonoBehaviour
{
    public ProfesorNavMeshMover profesorNavMeshMover;

    [Header("Yarn Spinner")]
    public DialogueRunner dialogueRunner;

    [Header("Referencias")]
    public Transform profesorMirada;
    public Camera playerCamera;
    public GameObject notaObjeto;
    public Animator profesorAnimator;

    [Header("Clips de audio del profesor")]
    public AudioSource profesorAudioSource;
    public AudioClip clipAproximacion;
    public AudioClip clipRevisionSilencio;
    public AudioClip clipBuenaBase;
    public AudioClip clipFeedbackDetallado;
    public AudioClip clipCriterio;
    public AudioClip clipTomateElTiempo;
    public AudioClip clipBienSigueAsi;

    [Header("Compañero de fondo (Fase 1E)")]
    public float tiempoLineaCompanero = 10f;
    public string nodoCompaniero = "Acto1_Companiero";

    [Header("Post-entrega")]
    public PlayerController playerController;
    public InteractableObject laptopInteractable;

    [Header("HDD")]
    public Transform hddModel;
    public GameObject proyectoObjeto;
    public SnapPointHDDTrigger snapPointHDD;

    [Header("Laptop UI")]
    public GameObject laptopPanel;
    public TMP_Text workingText;

    [Header("Configuración")]
    public float ventanaMirada = 2.5f;
    public float ventanaReintento = 45f;
    public float radioAproximacion = 2f;

    private bool aproximacionDisparada = false;
    private bool entregaRealizada = false;
    private bool decisionTomada = false;
    private Transform proyectoTransform;
    private bool yaUsoLaptop = false;
    private bool hddCompletado = false;
    private GameObject proyectoEnHDD;

    void Start()
    {
        if (workingText == null)
        {
            var wt = GameObject.Find("WorkingText");
            if (wt != null) workingText = wt.GetComponent<TMP_Text>();
        }

        if (laptopPanel == null)
        {
            laptopPanel = GameObject.Find("LaptopPanel");
        }

        if (laptopInteractable != null)
        {
            var showPanel = laptopInteractable.GetComponent<ShowInfoPanel>();
            if (showPanel != null) Destroy(showPanel);

            laptopInteractable.onInteract.AddListener(OnLaptopInteraction);
        }
    }

    public void SetProyectoTransform(Transform t) => proyectoTransform = t;

    void Update()
    {
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
            Debug.Log("Nota mostrada después del diálogo inicial.");
        }

        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);

        if (playerController != null)
            playerController.enabled = false;

        if (hddModel != null && playerCamera != null)
            yield return RotarCamaraHaciaHDD();

        float restante = 3f - 0.8f;
        if (restante > 0)
            yield return new WaitForSeconds(restante);

        dialogueRunner.StartDialogue("Acto1_Reaccion");

        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);

        if (laptopInteractable != null)
            laptopInteractable.canInteract = true;

        if (snapPointHDD != null)
        {
            snapPointHDD.gameObject.SetActive(true);
            snapPointHDD.activo = true;
        }

        if (playerController != null)
            playerController.enabled = true;

        Debug.Log("Iniciando ventana de decisión desde código.");
        IniciarVentanaDecision();
    }

    private IEnumerator RotarCamaraHaciaHDD()
    {
        Vector3 dir = (hddModel.position - playerCamera.transform.position).normalized;
        Quaternion targetBodyRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        Quaternion startBodyRot = playerController.transform.rotation;

        Vector3 targetLookDir = (hddModel.position - playerCamera.transform.position).normalized;
        Quaternion targetCamRot = Quaternion.LookRotation(targetLookDir);
        Quaternion startCamRot = playerCamera.transform.rotation;

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            playerController.transform.rotation = Quaternion.Slerp(startBodyRot, targetBodyRot, t);
            playerCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetCamRot, t);

            yield return null;
        }

        playerController.transform.rotation = targetBodyRot;
        playerCamera.transform.rotation = targetCamRot;
    }

    public void OnLaptopInteraction()
    {
        if (yaUsoLaptop) return;
        yaUsoLaptop = true;

        StartCoroutine(FlujoLaptop());
    }

    private IEnumerator FlujoLaptop()
    {
        if (playerController != null)
            playerController.enabled = false;

        if (laptopPanel != null)
            laptopPanel.SetActive(true);

        if (workingText != null)
            yield return AnimarWorkingText();

        if (laptopPanel != null)
            laptopPanel.SetActive(false);

        dialogueRunner.StartDialogue("Acto1_Laptop_Mejora");

        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);

        GameSessionData.UsoLaptop = true;

        EventLogger.Instance.RegistrarEvento(
            competencia: "Autorregulación emocional (persistencia)",
            situacion: "Recibió retroalimentación negativa en su primera entrega",
            comportamiento: "Usó la laptop para mejorar el proyecto digitalmente",
            tiempo: 0f,
            tendencia: "fortaleza"
        );

        if (playerController != null)
            playerController.enabled = true;
    }

    private IEnumerator AnimarWorkingText()
    {
        string baseText = "Working";
        float stepDuration = 0.3f;
        int cycles = 3;

        for (int c = 0; c < cycles; c++)
        {
            workingText.text = baseText;               yield return new WaitForSeconds(stepDuration);
            workingText.text = baseText + ".";          yield return new WaitForSeconds(stepDuration);
            workingText.text = baseText + "..";         yield return new WaitForSeconds(stepDuration);
            workingText.text = baseText + "...";        yield return new WaitForSeconds(stepDuration);
        }

        workingText.text = baseText + "...";
    }

    public void OnProyectoEnHDD(GameObject proyecto)
    {
        if (hddCompletado) return;
        hddCompletado = true;
        GameSessionData.HDDCompletado = true;
        proyectoEnHDD = proyecto;

        if (dialogueRunner.IsDialogueRunning)
            dialogueRunner.Stop();

        StartCoroutine(FlujoHDD());
    }

    private IEnumerator FlujoHDD()
    {
        if (playerController != null)
            playerController.enabled = false;

        dialogueRunner.StartDialogue("Acto1_HDD_Mejora");

        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);

        if (proyectoEnHDD != null)
        {
            var interactable = proyectoEnHDD.GetComponent<InteractableObject>();
            if (interactable != null)
                interactable.canInteract = true;

            proyectoEnHDD.transform.SetParent(null);
        }

        if (snapPointHDD != null)
        {
            snapPointHDD.activo = false;
            var col = snapPointHDD.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        if (playerController != null)
            playerController.enabled = true;
    }

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

        dialogueRunner.StartDialogue("Acto1_Reintento");

        GameSessionData.Reintento = true;
        GameSessionData.TiempoReintento = tiempoTranscurrido;
        ReporteManager.Instance.GuardarReporte();

        StartCoroutine(EsperarDialogoYFade());
    }

    IEnumerator EsperarDialogoYFade()
    {
        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);
        yield return FadeOut();
        EventLogger.Instance.ExportarJSON();
        if (playerController != null)
            playerController.enabled = false;
    }

    [YarnCommand("reproducir_linea")]
    public IEnumerator ReproducirLinea(string nombreClip)
    {
        AudioClip clip = ObtenerClipPorNombre(nombreClip);

        if (profesorAudioSource != null && clip != null)
        {
            profesorAudioSource.clip = clip;
            profesorAudioSource.Play();
            yield return new WaitForSeconds(clip.length + 0.2f);
        }
        else
        {
            yield return new WaitForSeconds(1.8f);
        }
    }

    [YarnCommand("mostrar_nota")]
    public void MostrarNota()
    {
        if (notaObjeto != null) notaObjeto.SetActive(true);
    }

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
            competencia: "Autorregulación emocional (control emocional)",
            situacion: "El profesor explica su retroalimentación frente al jugador",
            comportamiento: $"Sostuvo la mirada el {porcentaje:F0}% del tiempo",
            tiempo: ventanaMirada,
            tendencia: tendencia
        );
        GameSessionData.PorcentajeMirada = porcentaje;
    }

    [YarnCommand("profesor_retirarse")]
    public void ProfesorRetirarse()
    {
        if (profesorAnimator != null)
            profesorAnimator.SetTrigger("Retirarse");
    }

    [YarnCommand("iniciar_ventana_decision")]
    public void IniciarVentanaDecision()
    {
        StartCoroutine(VentanaDeDecision());
    }

    IEnumerator VentanaDeDecision()
    {
        bool lineaReproducida = false;
        float t = 0f;

        while (t < ventanaReintento && !decisionTomada)
        {
            if (!lineaReproducida && t >= tiempoLineaCompanero && !hddCompletado)
            {
                lineaReproducida = true;
                dialogueRunner.StartDialogue(nodoCompaniero);
                float tc = 0f;
                while (tc < 2f && dialogueRunner.IsDialogueRunning)
                {
                    tc += Time.deltaTime;
                    yield return null;
                }
                if (dialogueRunner.IsDialogueRunning)
                    dialogueRunner.Stop();
            }
            t += Time.deltaTime;
            yield return null;
        }

        if (!decisionTomada) yield return RegistrarRutaB(t);
    }

    IEnumerator RegistrarRutaB(float tiempoTranscurrido)
    {
        decisionTomada = true;

        Debug.Log("RUTA B: El jugador NO volvió a intentar.");
        Debug.Log("Tiempo transcurrido: " + tiempoTranscurrido + " segundos.");

        EventLogger.Instance.RegistrarEvento(
            competencia: "Autorregulación emocional (persistencia)",
            situacion: "Recibió retroalimentación negativa en su primera entrega",
            comportamiento: "No retomó el proyecto dentro del tiempo disponible",
            tiempo: tiempoTranscurrido,
            tendencia: "debilidad"
        );

        GameSessionData.Reintento = false;
        GameSessionData.TiempoReintento = tiempoTranscurrido;
        ReporteManager.Instance.GuardarReporte();

        if (profesorAnimator != null)
            profesorAnimator.applyRootMotion = false;

        if (profesorNavMeshMover != null)
            profesorNavMeshMover.Retirarse();

        yield return new WaitForSeconds(2f);

        if (profesorNavMeshMover != null && profesorNavMeshMover.agent != null)
        {
            float esperaMax = 10f;
            float t = 0f;
            while (t < esperaMax && profesorNavMeshMover.agent.pathPending)
            {
                t += Time.deltaTime;
                yield return null;
            }
            while (t < esperaMax && profesorNavMeshMover.agent.remainingDistance > 0.5f)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        Transform doorCube = null;
        var puertaAula = GameObject.Find("PuertaAula");
        if (puertaAula != null)
        {
            var child = puertaAula.transform.Find("a door (2)");
            if (child != null)
            {
                var cube = child.Find("Cube_10");
                if (cube != null) doorCube = cube;
            }
        }

        if (doorCube != null)
        {
            Quaternion from = doorCube.localRotation;
            Quaternion to = Quaternion.Euler(0, 0, 0);
            float duracion = 1.2f;
            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                doorCube.localRotation = Quaternion.Slerp(from, to, t / duracion);
                yield return null;
            }
            doorCube.localRotation = to;
        }

        yield return FadeOut();

        EventLogger.Instance.ExportarJSON();

        if (playerController != null)
            playerController.enabled = false;
    }

    IEnumerator FadeOut()
    {
        GameObject fadeObj = GameObject.Find("Fader/Fade");
        if (fadeObj == null) yield break;

        var img = fadeObj.GetComponent<UnityEngine.UI.Image>();
        if (img == null) yield break;

        float duration = 1.5f;
        float elapsed = 0f;
        Color c = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            img.color = c;
            yield return null;
        }

        c.a = 1f;
        img.color = c;
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
