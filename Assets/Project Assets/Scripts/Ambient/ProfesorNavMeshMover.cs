using UnityEngine;
using UnityEngine.AI;

public class ProfesorNavMeshMover : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
    public Transform puntoDestino;

    public void Retirarse()
    {
        if (agent == null || puntoDestino == null) return;

        if (animator != null)
        {
            animator.SetTrigger("Retirarse");
        }

        agent.SetDestination(puntoDestino.position);
    }
}