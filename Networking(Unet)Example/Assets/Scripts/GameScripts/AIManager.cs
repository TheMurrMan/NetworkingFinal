using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public GameObject aiPrefab;
    List<GameObject> aiList;
	[SerializeField] private int numUnitToSpawn;

	// Start is called before the first frame update
	void Start()
    {
        aiList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnAI();
        }
    }

    void SpawnAI()
	{
        Vector3 newPos;

        for (int i = 0; i < numUnitToSpawn; ++i)
        {
            GameObject newAI = Instantiate(aiPrefab);
            newPos = new Vector3(Random.Range(-10, 10), 0.25f, Random.Range(-10, 10));
            newAI.transform.position = newPos;
            aiList.Add(newAI);
        }
    }
}
