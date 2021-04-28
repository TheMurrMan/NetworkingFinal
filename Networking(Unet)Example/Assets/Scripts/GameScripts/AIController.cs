using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AIController : MonoBehaviour
{
    [SerializeField] int enemySpeed;
    [SerializeField] int enemyHealth;
    [SerializeField] int attackRange;
    [SerializeField] int enemyDamage = 10;
    [SerializeField] float attackTimer = 0;
    [SerializeField] float resetAttackTimer = 0.5f;

    public int myId; 
    ServerClient closestPlayer;
    public AIState state;
    private Slider healthBar;

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

        healthBar = GetComponentInChildren<Slider>();
        healthBar.maxValue = enemyHealth;
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = enemyHealth;
        if (attackTimer <= 0)
            attackTimer = resetAttackTimer;

        if(closestPlayer == null)
		{
            GetClosestPlayer();
		}

        switch(state)
		{
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
    }

    void WalkingState()
    {
        Debug.Log("Walking State");
        if (Vector3.Distance(transform.position, closestPlayer.position) > 2)
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
        if (Vector3.Distance(transform.position, closestPlayer.position) <= attackRange)
        {

            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    Debug.Log("Attack");
                    attackTimer = 0f;
                    Server server = FindObjectOfType<Server>();

                    server.OnPlayerTakeDamage(closestPlayer);
                }
            }
        }

        else
        {
            state = AIState.Walking;
            Debug.Log("Continue Walking");
        }
    }

    void HandleMovement()
	{
        Vector3 targetPos = closestPlayer.position;
        Vector3 currentPos = transform.position;

        if (Vector3.Distance(currentPos, targetPos) >= 0.15f)
        {
            Vector3 moveDir = (targetPos - currentPos).normalized;

            transform.position += moveDir * (enemySpeed * Time.deltaTime);

            state = AIState.Walking;
        }
    }

    public void TakeDamage(int damage, int bulletID)
	{
        enemyHealth -= damage;
        FindObjectOfType<Server>().OnTakeDamage(this, bulletID);
    }

    private void GetClosestPlayer()
    {
        closestPlayer = null;
        float distanceToClosestPlayer = Mathf.Infinity;

        List<ServerClient> clients = FindObjectOfType<Server>().clients;
        foreach (ServerClient client in clients)
        {
            float distanceToPlayer = (client.position - transform.position).sqrMagnitude;
            if (distanceToPlayer < distanceToClosestPlayer)
            {
                distanceToClosestPlayer = distanceToPlayer;
                closestPlayer = client;
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

    public ServerClient ReturnClosestPlayer()
	{
        return closestPlayer;
	}
}
