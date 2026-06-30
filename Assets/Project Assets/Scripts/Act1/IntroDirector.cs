using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class IntroDirector : MonoBehaviour
{
    [Header("Asignar manualmente en el Inspector")]
    public DialogueRunner dialogueRunner;
    public PlayerController playerController;
    public CharacterController characterController;
    public GameObject waitWall;
    public Transform waitPoint;
    public Transform puerta;
    public Transform puertaTargetLook;

    private bool introCompleta = false;

    void Start()
    {
        if (dialogueRunner == null)
            dialogueRunner = GameObject.Find("Dialogue System").GetComponent<DialogueRunner>();

        if (playerController == null || characterController == null)
        {
            var player = GameObject.Find("Player");
            if (playerController == null) playerController = player.GetComponent<PlayerController>();
            if (characterController == null) characterController = player.GetComponent<CharacterController>();
        }

        if (waitWall == null) waitWall = GameObject.Find("WaitWall");
        if (waitPoint == null) { var wp = GameObject.Find("waitpoint"); if (wp != null) waitPoint = wp.transform; }
        if (puerta == null) { var c = GameObject.Find("Cube_10"); if (c != null) puerta = c.transform; }
        if (puertaTargetLook == null) { var p = GameObject.Find("PuertaAula"); if (p != null) puertaTargetLook = p.transform; }

        Debug.Log("[IntroDirector] dialogueRunner=" + (dialogueRunner != null) +
            " player=" + (playerController != null) +
            " charCtrl=" + (characterController != null) +
            " waitWall=" + (waitWall != null) +
            " waitPoint=" + (waitPoint != null) +
            " puerta=" + (puerta != null ? puerta.name + " (" + puerta.parent.name + ")" : "NULL") +
            " targetLook=" + (puertaTargetLook != null ? puertaTargetLook.name : "NULL"));

        if (puerta != null)
        {
            puerta.localRotation = Quaternion.Euler(0, 0, 0);
        }

        dialogueRunner.AddCommandHandler("abrir_puerta", AnimacionApertura);
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
    }

    private IEnumerator AnimacionApertura()
    {
        characterController.enabled = false;
        playerController.enabled = false;

        Vector3 startPos = playerController.transform.position;
        Vector3 targetPos = new Vector3(
            waitPoint.position.x,
            startPos.y,
            waitPoint.position.z
        );

        Vector3 targetDir = new Vector3(
            puertaTargetLook.position.x - targetPos.x,
            0,
            puertaTargetLook.position.z - targetPos.z
        ).normalized;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);

        float moveDist = Vector3.Distance(
            new Vector3(startPos.x, 0, startPos.z),
            new Vector3(targetPos.x, 0, targetPos.z)
        );
        float moveDuration = moveDist / playerController.walkSpeed;
        float elapsed = 0;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            playerController.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        playerController.transform.position = targetPos;

        Quaternion startRot = playerController.transform.rotation;
        Quaternion startCamRot = playerController.playerCamera.transform.localRotation;
        float rotDuration = 0.8f;
        float rotElapsed = 0;

        while (rotElapsed < rotDuration)
        {
            rotElapsed += Time.deltaTime;
            float t = rotElapsed / rotDuration;
            playerController.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            playerController.playerCamera.transform.localRotation = Quaternion.Slerp(startCamRot, Quaternion.identity, t);
            yield return null;
        }

        playerController.transform.rotation = targetRot;
        playerController.playerCamera.transform.localRotation = Quaternion.identity;

        if (puerta != null)
        {
            Quaternion doorFrom = Quaternion.Euler(0, 0, 0);
            Quaternion doorTo = Quaternion.Euler(0, 90, 0);
            float doorDuration = 1.2f;
            float doorElapsed = 0;

            while (doorElapsed < doorDuration)
            {
                doorElapsed += Time.deltaTime;
                puerta.localRotation = Quaternion.Lerp(doorFrom, doorTo, doorElapsed / doorDuration);
                yield return null;
            }

            puerta.localRotation = doorTo;
        }
        else
        {
            Debug.LogError("[IntroDirector] puerta is NULL!");
        }
    }

    private void OnDialogueComplete()
    {
        if (introCompleta) return;
        introCompleta = true;

        characterController.enabled = true;
        playerController.enabled = true;
        waitWall.SetActive(false);
    }
}
