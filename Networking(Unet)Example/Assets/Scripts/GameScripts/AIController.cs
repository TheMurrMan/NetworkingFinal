using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AIController : MonoBehaviour
{
    [SerializeField] int enemySpeed;
    [SerializeField] int enemyHealth;
    [SerializeField] int enemyDamage = 10;

    [SerializeField] PlayerController closestPlayer;
    public AIState state;
    public enum AIState
    {
        Idle,
        Walking,
        Attacking
	}

	// Start is called before the first frame update
	void Start()
    {
        state = AIState.Walking;
        GetClosestPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if(closestPlayer == null)
		{
            GetClosestPlayer();
		}

        switch(state)
		{
            case AIState.Idle:
            {
              IdleState();
            }
            break;

            case AIState.Walking:
            {
               WalkingState();
            }
            break;
            
            case AIState.Attacking:
			{
               AttackinState();
		    }
            break;

            default:
                break;
		}

        if(enemyHealth <=0)
		{
            Destroy(this.gameObject);
		}
    }

    void IdleState()
	{
        Debug.Log("Idle");
	}
    void WalkingState()
    {
        Debug.Log("Walking State");
        if (Vector3.Distance(transform.position, closestPlayer.transform.position) > 1)
		{
            HandleMovement();
		}

        else
		{
            Debug.Log("Within attacking range");
            state = AIState.Attacking;
		}

    }

    void AttackinState()
    {
        Debug.Log("Attacking State");
        if (Vector3.Distance(transform.position, closestPlayer.transform.position) < 10)
        {
            Debug.Log("Attack");
            closestPlayer.GetComponent<PlayerController>().TakeDamage(enemyDamage);
        }

        else
		{
            state = AIState.Walking;
            Debug.Log("Continue Walking");
		}
    }

    void HandleMovement()
	{
        Vector3 targetPos = closestPlayer.transform.position;
        Vector3 currentPos = transform.position;

        if (Vector3.Distance(currentPos, targetPos) >= 0.15f)
        {
            Vector3 moveDir = (targetPos - currentPos).normalized;

            transform.position += moveDir * (enemySpeed * Time.deltaTime);

            state = AIState.Walking;
        }
    }

    public void TakeDamage(int damage)
	{
        enemyHealth -= damage;
	}

    private void GetClosestPlayer()
    {
        closestPlayer = null;
        float distanceToClosestPlayer = Mathf.Infinity;
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            float distanceToPlayer = (player.transform.position - transform.position).sqrMagnitude;
            if (distanceToPlayer < distanceToClosestPlayer)
            {
                distanceToClosestPlayer = distanceToPlayer;
                closestPlayer = player;
            }
        }
    }

    public int GetSpeed()
	{
        return enemySpeed;
	}

    public int GetHealth()
    {
        return enemyHealth;
    }

    public AIState GetState()
    {
        return state;
    }

    public PlayerController ReturnClosestPlayer()
	{
        return closestPlayer;
	}
}
