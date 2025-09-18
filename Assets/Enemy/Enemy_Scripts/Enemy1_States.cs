using UnityEngine;
using System.Collections;

public class Enemy1_States : MonoBehaviour
{

    [SerializeField] public float maxHealth = 100f;
    private float currentHealth;
    private float distToGround;
    private float deathTransitionTimer = 20f;
    private float groundTransitionTimer = 1f;
    public GameObject phase2Enemy1;

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            ChangePhase();
        }
    }

    private void ChangePhase()
    {
        this.GetComponent<Rigidbody>().useGravity = true;
        distToGround = GetComponent<Collider>().bounds.extents.y;
        StartCoroutine(WaitForGround());
    }

    private IEnumerator WaitForGround()
    {
        float elapsedTime = 0f;
        while (!IsGrounded())
        {
            while (elapsedTime < deathTransitionTimer)
            {
                elapsedTime += Time.deltaTime;
            }
            yield return null;
        }

        // Once the enemy touches the ground, we wait a second then we spawn the next phase of the enemy
        StartCoroutine(GroundedAction());
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }

    private IEnumerator GroundedAction()
    {
        if (IsGrounded())
        {
            float elapsedTime = 0f;
            while (elapsedTime < groundTransitionTimer)
            {
                elapsedTime += Time.deltaTime;
            }
            yield return null;

            // this.gameObject.GetComponent<Renderer>().enabled = false; // makes the enemy invis so two models arent showing at once.
            Instantiate(phase2Enemy1);
            Destroy(gameObject);
        }
    }
    private void DevTestDie()
    {
        float elapsedTime = 0f;

        for (int i = 0; i < 4; i++)
        {
            while (elapsedTime < 1)
            {
                elapsedTime += Time.deltaTime;
                TakeDamage(25);
                Debug.Log("Current health is " + currentHealth);
            }
        }
    }
}
