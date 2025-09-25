using UnityEngine;
using System.Collections;
using StarterAssets;

public class DodgeRoll : MonoBehaviour
{
    private FirstPersonController fpsController;
    public Player player;

    public float rollSpeed = 15f;
    public float rollDistance = 5f;
    public float rollCooldown = 1f;

    private bool canRoll = true;
    private bool isRolling = false;     //functionally useless unless we have if statements that occur during the dodgeroll, or if we're playing animations.

    void Start()
    {
        fpsController = GetComponent<FirstPersonController>();
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        if (InputManager.Instance.PlayerAbility3() && canRoll)
        {
            Vector2 moveInput = InputManager.Instance.GetPlayerMovement();
            Vector3 localInput = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 direction = transform.TransformDirection(localInput).normalized;

            if (direction.sqrMagnitude < 0.001f)
            {
                // No WASD → roll forward
                direction = transform.forward;
            }

            StartCoroutine(Roll(direction));
        }
    }

    private IEnumerator Roll(Vector3 direction)
    {
        canRoll = false;
        isRolling = true;
        player.isInvulnerable = true;

        float rollTime = rollDistance / rollSpeed;
        float startTime = Time.time;

        // Disable normal movement during roll
        fpsController.CanMove = false;

        //Actual Dodgeroll movement
        while (Time.time < startTime + rollTime)
        {
            Vector3 velocity = direction * rollSpeed + new Vector3(0f, fpsController.GetVerticalVelocity(), 0f);

            fpsController.GetController().Move(velocity * Time.deltaTime);

            yield return null;
        }

        //Return Controll
        fpsController.CanMove = true;
        player.isInvulnerable = false;
        isRolling = false;

        //Start Cooldown
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }
}