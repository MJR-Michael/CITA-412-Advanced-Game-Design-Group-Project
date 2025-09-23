using UnityEngine;
using System.Collections;

public class Dodgeroll : MonoBehaviour
{
    [SerializeField] float dodgerollCooldown = 1f;
    [SerializeField] float dodgerollDistance = 20f;
    [SerializeField] float dodgerollSpeed = 5f;
    [SerializeField] float dodgerollForce;

    private bool canDodgeRoll = true;
    private bool isDodgeRolling = false;

    public Rigidbody rb;

    void Update()
    {
        if (InputManager.Instance.PlayerAbility1() == true) //&& canDodgeRoll)
        {
            Debug.Log("Ability Used");
            StartCoroutine(dodgerollAction());
        }
    }

    private IEnumerator dodgerollAction()
    {
        canDodgeRoll = false;
        isDodgeRolling = true;

        rb.linearVelocity = Vector3.zero;
        // Apply force in the forward direction
        rb.AddForce(transform.forward * dodgerollForce, ForceMode.VelocityChange);

        // Optional: limit dash duration if needed
        yield return new WaitForSeconds(1f);

        isDodgeRolling = false;

        yield return new WaitForSeconds(dodgerollCooldown);
        canDodgeRoll = true;
    }
}


// float dashDuration = dashDistance / dashSpeed;

//         // Save original drag (optional)
//         float originalDrag = rb.drag;

//         // Remove drag so dash isn't slowed
//         rb.drag = 0f;

//         // Set velocity directly for consistent dash
//         rb.velocity = transform.forward * dashSpeed;

//         // Wait for the dash duration
//         yield return new WaitForSeconds(dashDuration);

//         // Stop dash but keep any momentum you want
//         rb.velocity = Vector3.zero;

//         // Restore drag
//         rb.drag = originalDrag;