using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolAndChase : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] points;
    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float targetRadius = 0.1f;
    [SerializeField] private float visionRange = 10;
    [SerializeField] private float visionConeAngle = 30;
    public int Respawn;

    private int indexOfTarget;
    private Vector3 targetPoint;
    private State state = State.PatrolState;
    private CharacterController controller;

    
    //Animator animator;
    UnityEngine.AI.NavMeshAgent agent;

    void Start()
    {
        //animator = this.GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        controller = GetComponent<CharacterController>();
        indexOfTarget = -1;
        NextTarget();
        LookAtTarget();
        agent.SetDestination(targetPoint);
    }

    float GetDistanceToPlayer()
    {
        return
            (player.position - transform.position)
            .magnitude;
    }
    
        float GetAngleToPlayer()
    {
        Vector3 directionToPlayer =
            (player.position - transform.position)
            .normalized;
        return Vector3.Angle(transform.forward, directionToPlayer);
    }

        bool SightLineObstructed()
    {
        Vector3 vectorToPlayer = player.transform.position - transform.position;
        Ray ray = new Ray(
            transform.position,
            vectorToPlayer);
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo, vectorToPlayer.magnitude))
        {
            GameObject obj = hitInfo.collider.gameObject;
            return obj != player.gameObject;
        }
        return false;
    }

    bool HasCaughtPlayer() {
        return GetDistanceToPlayer() < 30;
    }

    bool CanSeePlayer()
    {
        if (GetDistanceToPlayer() < visionRange)
        {
            if (GetAngleToPlayer() < visionConeAngle)
            {
                if (!SightLineObstructed())
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Update()
    {
        //Debug.Log("Distance: " + GetDistanceToPlayer());
        if(state == State.PatrolState)
        {
            Patrol();
        }
        if(state == State.ChaseState)
        {
            Chase();
        }
    }

    void NextTarget()
    {
        indexOfTarget = (indexOfTarget + 1) % points.Length;
        targetPoint = points[indexOfTarget].position;
        targetPoint.y = transform.position.y;
    }

    void LookAtTarget()
    {
        Vector3 lookAt = targetPoint;
        lookAt.y = transform.position.y;

        Vector3 lookDir = (lookAt - transform.position).normalized;
        transform.forward = lookDir;
    }

    void Patrol()
    {
        if (CanSeePlayer())
        {
            state = State.ChaseState;
            return;
        }

        if ((targetPoint-transform.position).magnitude < agent.radius)
        {
            NextTarget();
            LookAtTarget();
        }
        agent.SetDestination(targetPoint);
        //animator.SetBool("running", false);

    }
    void Chase()
    {
        if(!CanSeePlayer())
        {
            state = State.PatrolState;
            NextTarget();
            LookAtTarget();
            return;
        }
        LookAtTarget();
        agent.SetDestination(targetPoint);
        targetPoint = player.transform.position;
        Vector3 velocity = targetPoint - transform.position;
        velocity.y = 0;
        velocity.Normalize();
        velocity *= moveSpeed * Time.deltaTime;
        controller.Move(velocity);
        //animator.SetBool("running", true);

    }

    enum State
    {
        PatrolState,
        ChaseState,
    }
}
