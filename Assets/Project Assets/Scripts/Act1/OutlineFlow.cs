using UnityEngine;

public class OutlineFlow : MonoBehaviour
{
    [Header("Referencias")]
    public Outline mesaAlumno;
    public Outline mesaProfesor;
    public Transform jugador;

    [Header("Configuracion")]
    public float radioProximidad = 2f;

    private bool outlineMesaAlumnoActivo = true;
    private bool mesaProfesorDesactivada = false;

    void Start()
    {
        if (mesaAlumno != null)
        {
            mesaAlumno.OutlineColor = Color.yellow;
            mesaAlumno.OutlineMode = Outline.Mode.OutlineVisible;
            mesaAlumno.OutlineWidth = 10f;
        }

        if (mesaProfesor != null)
        {
            mesaProfesor.OutlineColor = Color.yellow;
            mesaProfesor.OutlineMode = Outline.Mode.OutlineVisible;
            mesaProfesor.OutlineWidth = 0f;
        }
    }

    void Update()
    {
        if (!outlineMesaAlumnoActivo || jugador == null || mesaAlumno == null)
            return;

        float dist = Vector3.Distance(jugador.position, mesaAlumno.transform.position);
        if (dist <= radioProximidad)
        {
            mesaAlumno.OutlineWidth = 0f;
            outlineMesaAlumnoActivo = false;

            if (mesaProfesor != null && !mesaProfesorDesactivada)
                mesaProfesor.OutlineWidth = 10f;
        }
    }

    public void DesactivarOutlineMesaProfesor()
    {
        if (mesaProfesor != null)
            mesaProfesor.OutlineWidth = 0f;

        mesaProfesorDesactivada = true;
    }
}
