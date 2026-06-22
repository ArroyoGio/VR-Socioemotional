using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Un solo objeto de este tipo vive durante toda la sesión (DontDestroyOnLoad).
// Cada Acto llama a RegistrarEvento() cuando el jugador toma una decisión.
// Al final, ExportarJSON() guarda todo en un archivo que el psicólogo
// puede revisar fuera de la VR.

[System.Serializable]
public class ResultadoEvento
{
    public string competencia;
    public string situacion;
    public string comportamiento;
    public float tiempoReaccion;
    public string tendencia; // "fortaleza" o "debilidad", según el criterio de la psicopedagoga

    public ResultadoEvento(string competencia, string situacion, string comportamiento, float tiempo, string tendencia)
    {
        this.competencia = competencia;
        this.situacion = situacion;
        this.comportamiento = comportamiento;
        this.tiempoReaccion = tiempo;
        this.tendencia = tendencia;
    }
}

[System.Serializable]
public class ReporteSesion
{
    public string estudianteId = "ID-PRUEBA";
    public List<ResultadoEvento> resultados = new List<ResultadoEvento>();
}

public class EventLogger : MonoBehaviour
{
    public static EventLogger Instance;

    private ReporteSesion reporte = new ReporteSesion();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegistrarEvento(string competencia, string situacion, string comportamiento, float tiempo, string tendencia)
    {
        ResultadoEvento r = new ResultadoEvento(competencia, situacion, comportamiento, tiempo, tendencia);
        reporte.resultados.Add(r);

        Debug.Log($"[EventLogger] {competencia} ? {comportamiento} ({tendencia}) — {tiempo:F1}s");
    }

    public List<ResultadoEvento> ObtenerResultados() => reporte.resultados;

    public void ExportarJSON()
    {
        string json = JsonUtility.ToJson(reporte, true);
        string path = Path.Combine(Application.persistentDataPath, "reporte_sesion.json");
        File.WriteAllText(path, json);
        Debug.Log($"[EventLogger] Reporte exportado a: {path}");
    }
}