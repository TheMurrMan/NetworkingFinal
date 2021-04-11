using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed;
	public float timeToLive;
	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.CompareTag("AI"))
		{
			Debug.Log("ENEMY");
			collision.gameObject.GetComponent<AIController>().TakeDamage(10);
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		Destroy(gameObject, timeToLive);
	}

	// Update is called once per frame
	void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
