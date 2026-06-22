using UnityEngine;

public class CerrarPuertaSimple : MonoBehaviour
{
    public Transform puerta;

    bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado) return;

        if (!other.CompareTag("Player")) return;

        activado = true;

        puerta.rotation = Quaternion.Euler(0, 0, 0);
    }
}