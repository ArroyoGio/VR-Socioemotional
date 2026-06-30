using System;
using System.IO;
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
        string tendencia = GameSessionData.Reintento
            ? "Fortaleza: persistencia"
            : "Debilidad: abandono";

        string contenido =
            "=== REPORTE DE SESI\u00D3N - CrisisVR ===\n" +
            $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n" +
            "--- ACTO 1: Autorregulaci\u00F3n emocional ---\n" +
            $"Reintent\u00F3: {(GameSessionData.Reintento ? "S\u00ED" : "No")}\n" +
            $"Tiempo de reintento: {GameSessionData.TiempoReintento:F1}s\n" +
            $"Mirada sostenida: {GameSessionData.PorcentajeMirada:F0}%\n" +
            $"Us\u00F3 laptop: {(GameSessionData.UsoLaptop ? "S\u00ED" : "No")}\n" +
            $"HDD completado: {(GameSessionData.HDDCompletado ? "S\u00ED" : "No")}\n" +
            $"Tendencia: {tendencia}\n";

        string ruta = Application.persistentDataPath + "/reporte.txt";
        File.WriteAllText(ruta, contenido);

        Debug.Log("Reporte guardado en: " + ruta);
    }
}
