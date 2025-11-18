using UnityEngine;
using UnityEngine.AI;

public class TheDestroyerController : MonoBehaviour
{
    [SerializeField]
    NavMeshAgent navMeshAgent;

    [SerializeField]
    Transform testTransform;

    private void Update()
    {
        NavMeshPath path = new NavMeshPath();
        //Testing boss movement
        navMeshAgent.CalculatePath(testTransform.position, path);
        navMeshAgent.SetPath(path);
    }
}
