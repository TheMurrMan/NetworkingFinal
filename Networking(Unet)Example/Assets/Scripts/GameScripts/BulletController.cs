using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed;
	public float timeToLive;
	public int myID;
	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.CompareTag("AI"))
		{
			Debug.Log("HIT");
			
			collision.gameObject.GetComponent<AIController>().TakeDamage(10, myID);
			Destroy(gameObject);
		}
	}
	
	// Update is called once per frame
	void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        
        timeToLive -= Time.deltaTime;

        if (timeToLive <= 0f)
        {
	        FindObjectOfType<Client>().RemoveBullet(myID);
        }
    }
	
}
