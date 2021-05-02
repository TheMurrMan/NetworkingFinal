using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AIManager : MonoBehaviour
{
    
    [System.Serializable]
    public class Wave
	{
        public GameObject enemy;
        public int count;
	}

    public enum SpawnState
    {
        Spawning,
        Waiting,
        Counting
    }

    public Wave[] waves;
    private int nextWave = 0;
    public float timeBetweenWaves = 5f;

    int managerId;
    public float waveCountdown;
    private float searchCountdown = 1f;
    private SpawnState state = SpawnState.Counting;
    public List<GameObject> aiList;

	// Start is called before the first frame update
	void Start()
    {
        aiList = new List<GameObject>();
        waveCountdown = timeBetweenWaves;
    }

    // Update is called once per frame
    void Update()
    {
        RemoveDeadAIFromList();
        if (state == SpawnState.Waiting)
		{
            // Check if enemies are alive
            if(!EnemyIsAlive())
			{
                // Begin new round
                WaveCompleted();
			}

            else
			{
                return;
			}
		}

        if (waveCountdown <= 0)
        { 
            if(state != SpawnState.Spawning)
			{
                // Start spawning
                SpawnWave(waves[nextWave]);
            }
        }

        else
		{
            waveCountdown -= Time.deltaTime;
		}

        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnWave(waves[nextWave]);
        }
    }

    void WaveCompleted()
	{
        Debug.Log("Wave Completed");

        state = SpawnState.Counting;
        waveCountdown = timeBetweenWaves;

        if(nextWave + 1 > waves.Length - 1)
		{
            nextWave = 0;
            Debug.Log("All waves complete");
            // WIN State
            FindObjectOfType<Server>().OnWin();
        }

        nextWave++;
    }
    bool EnemyIsAlive()
	{
        searchCountdown -= Time.deltaTime;

        if(searchCountdown <= 0)
		{
            searchCountdown = 1f;
            if (GameObject.FindGameObjectWithTag("AI") == null)
            {
                return false;
            }
        }

        return true;
	}
    void SpawnWave(Wave _wave)
	{
        Debug.Log("Spawning Wave");
        state = SpawnState.Spawning;
        
        for (int i = 0; i < _wave.count; ++i)
        {
            SpawnEnemy(_wave.enemy);
        }

        state = SpawnState.Waiting;
    }

    void SpawnEnemy(GameObject enemy)
	{
        Vector3 newPos;
        GameObject newAI = Instantiate(enemy);
        newPos = new Vector3(Random.Range(-10, 10), 0.25f, Random.Range(-10, 10));
        newAI.transform.position = newPos;
        newAI.GetComponent<AIController>().myId = managerId;
        aiList.Add(newAI);
        managerId++;

        FindObjectOfType<Server>().SpawnEnemy(newAI);
    }

    void RemoveDeadAIFromList()
	{
        foreach(GameObject ai in aiList)
		{
            if(ai.GetComponent<AIController>().GetHealth() <= 0)
			{
                FindObjectOfType<Server>().OnEnemyDeath(ai.GetComponent<AIController>().myId);
                aiList.Remove(ai);
                Destroy(ai);
			}
		}
	}
}
