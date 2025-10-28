using System.Collections;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField, Tooltip("The delay before the destructiblePrefab parts begin to disappear")]
    float delayBeforeDecay;

    [SerializeField, Tooltip("The duration it takes for destructiblePrefab parts to fully disappear")]
    float decayDuration;

    Rigidbody[] childRigidbodies;

    bool isDecaying;
    float currentDecayTime;

    Vector3 forceDir;
    float forceMagnitude;

    private void Awake()
    {
        childRigidbodies = GetComponentsInChildren<Rigidbody>();
        currentDecayTime = decayDuration;
    }

    private void Start()
    {
        Invoke("BeginDecaying", delayBeforeDecay);
        //wait until end of frame
        foreach (Rigidbody childRB in childRigidbodies)
        {
            //Apply a force in the given direction
            childRB.AddForce(-forceDir * forceMagnitude);
        }
    }

    private void Update()
    {
        if (!isDecaying) { return; }
        if (currentDecayTime <= 0)
        {
            //Remove the destructible object.
            Destroy(gameObject);
            return;
        }

        //Decrease the scale of the child objects
        //Scale = current decay time / decay duration
        foreach (Rigidbody childRb in childRigidbodies)
        {
            childRb.transform.localScale = Vector3.one * (currentDecayTime / decayDuration);
        }

        currentDecayTime = Mathf.Clamp(
            currentDecayTime - Time.deltaTime,
            0,
            decayDuration
            );
    }

    public void Initialize(Vector3 direction, float forceMagnitude)
    {
        forceDir = direction;
        this.forceMagnitude = forceMagnitude;
    }
    void BeginDecaying()
    {
        isDecaying = true;
    }
}
