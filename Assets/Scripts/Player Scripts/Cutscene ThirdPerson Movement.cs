using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneThirdPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;
    public Animator animator;
    public Transform playerTransform;

    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float rollSpeed = 20f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    bool isRolling = false;

    // Juster denne offset afhængigt af din animations højde
    float animationHeightOffset = 1.0f;

    void Start()
    {
        playerTransform.position = new Vector3(playerTransform.position.x, 40.8f, playerTransform.position.z);
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.Space) && isRunning && !isRolling)
        {
            StartCoroutine(RollForward());
        }

        animator.SetBool("running", isRunning);
        animator.SetBool("rolling", isRolling);

        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        if (!isRolling && direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            moveDir.y = 0f;
            controller.Move(moveDir.normalized * targetSpeed * Time.deltaTime);
        }
    }

    IEnumerator RollForward()
    {
        isRolling = true;

        // Tag den aktuelle retning, spilleren kigger
        float targetAngle = transform.eulerAngles.y;
        // Beregn bevægelsesvektoren baseret på den retning
        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

        // Indstil spilleren til at bevæge sig kun fremad
        moveDir.y = 0f;

        // Juster spillerens y-position baseret på animationshøjden
        Vector3 startPosition = playerTransform.position;
        Vector3 targetPosition = startPosition + Vector3.up * animationHeightOffset;

        // Kontroller om den nye position er gyldig
        if (Physics.Raycast(targetPosition, Vector3.down, out RaycastHit hit, animationHeightOffset * 2))
        {
            targetPosition = hit.point + Vector3.up * animationHeightOffset;
        }

        // Rull fremad i en kort tid
        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            playerTransform.position = Vector3.Lerp(startPosition, targetPosition, t / 0.5f);
            controller.Move(moveDir.normalized * rollSpeed * Time.deltaTime);
            yield return null;
        }

        isRolling = false;
    }
}