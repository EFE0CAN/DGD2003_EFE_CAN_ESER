using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerFollower : MonoBehaviour
{
    [Header("Hedef (bos birak = MainCam bulur)")]
    public Transform playerTarget;

    [Header("Takip")]
    public float stopDistance = 2f;
    public float repathInterval = 0.2f;

    private NavMeshAgent agent;
    private float timer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (playerTarget == null)
        {
            GameObject mainCam = GameObject.Find("MainCam");
            if (mainCam != null)
                playerTarget = mainCam.transform;
        }
    }

    void Start()
    {
        agent.stoppingDistance = stopDistance;
    }

    void Update()
    {
        if (playerTarget == null) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = repathInterval;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }
        else
        {
            Vector3 dir = playerTarget.position - transform.position;
            dir.y = 0f;
            if (dir.magnitude > stopDistance)
                transform.position += dir.normalized * agent.speed * repathInterval;
        }
    }
}