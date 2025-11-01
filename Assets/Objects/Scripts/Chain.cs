using Unity.VisualScripting;
using UnityEngine;

public class Chain : MonoBehaviour
{
    [SerializeField, Tooltip("The maximum distance from the player before the chain decides to not calculate its physics")]
    float maxDistanceBeforeRbSetKinematic = 50f;

    Rigidbody[] rigidbodies;
    bool isSetToKinematic = false;


    private void Awake()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
    }
    void Update()
    {
        float currentPlayerDistanceFromChain = Vector3.Distance(Camera.main.transform.position, transform.position);

        //Debug.Log(currentPlayerDistanceFromChain);

        if (currentPlayerDistanceFromChain > maxDistanceBeforeRbSetKinematic)
        {
            //If the chain is not already kinematic, make it kinematic
            if (isSetToKinematic) return;

            isSetToKinematic = true;

            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = isSetToKinematic;
            }
        }
        //Otherwise, re-enable rigidbody physics if the chain is currently kinematic
        else if (isSetToKinematic)
        {


            isSetToKinematic = false;
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = isSetToKinematic;
            }
        }

        //if (isSetToKinematic)
        //{
        //    Debug.Log("is kinematic");
        //}
        //else
        //{
        //    Debug.Log("is not kinematic");
        //}
    }
}
