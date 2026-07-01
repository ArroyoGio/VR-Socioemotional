using System;
using System.IO;
using System.Text;
using UnityEngine;

public class ReporteManager : MonoBehaviour
{
    public static ReporteManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void GuardarReporte()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("=== REPORTE DE SESI\u00D3N ===");
        sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine();

        // ─ Eventos ─
        var eventos = EventLogger.Instance.ObtenerResultados();
        for (int i = 0; i < eventos.Count; i++)
        {
            var e = eventos[i];
            sb.AppendLine($"Evento #{i + 1}");
            sb.AppendLine($"  Competencia: {e.competencia}");
            sb.AppendLine($"  Situaci\u00F3n: {e.situacion}");
            sb.AppendLine($"  Comportamiento: {e.comportamiento}");

            if (e.tiempoReaccion > 0f)
                sb.AppendLine($"  Tiempo de reacci\u00F3n: {e.tiempoReaccion:F1}s");

            sb.AppendLine($"  Tendencia: {e.tendencia}");
            sb.AppendLine();
        }

        // ─ Resumen ─
        string tendencia = GameSessionData.Reintento
            ? "Fortaleza: persistencia"
            : "Debilidad: abandono";

        sb.AppendLine("--- RESUMEN ---");
        sb.AppendLine($"Reintent\u00F3: {(GameSessionData.Reintento ? "S\u00ED" : "No")}");
        sb.AppendLine($"Tiempo de reintento: {GameSessionData.TiempoReintento:F1}s");
        sb.AppendLine($"Mirada sostenida: {GameSessionData.PorcentajeMirada:F0}%");
        sb.AppendLine($"Us\u00F3 laptop: {(GameSessionData.UsoLaptop ? "S\u00ED" : "No")}");
        sb.AppendLine($"HDD completado: {(GameSessionData.HDDCompletado ? "S\u00ED" : "No")}");
        sb.AppendLine($"Tendencia general: {tendencia}");

        string ruta = Application.persistentDataPath + "/reporte.txt";
        File.WriteAllText(ruta, sb.ToString(), Encoding.UTF8);

        Debug.Log("Reporte guardado en: " + ruta);
    }
}
