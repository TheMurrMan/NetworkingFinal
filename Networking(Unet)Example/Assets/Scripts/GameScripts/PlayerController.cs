﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

public class PlayerController : MonoBehaviour
{
    private float moveSpeed = 5;
    Vector3 movement = Vector3.zero;

    private Camera cam;
    private Rigidbody rb;

    public GameObject bulletPrefab;
    public Transform gunEnd;
    
    [SerializeField] private int playerHealth = 100;
	private void Start()
	{
        rb = GetComponent<Rigidbody>();
        cam = FindObjectOfType<Camera>();

        gunEnd = transform.GetChild(2);
        bulletPrefab = Resources.Load("Bullet") as GameObject;
        
	}
	// Update is called once per frame
	void Update()
    {
        GetPlayerInput();

        if(playerHealth <=0)
		{
            Debug.Log("DIE");
            //Die();
        }
    }

	private void FixedUpdate()
	{
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        //transform.Translate(movement * moveSpeed * Time.fixedDeltaTime);
    }

    void GetPlayerInput()
	{
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.z = Input.GetAxisRaw("Vertical");


        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if(groundPlane.Raycast(cameraRay, out rayLength))
		{
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);

            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
		}

        if(Input.GetMouseButtonDown(0))
		{
            SpawnBullet();
		}
    }

    void SpawnBullet()
	{
        Instantiate(bulletPrefab, gunEnd.position, gunEnd.rotation);
	}

    void Die()
	{
        Destroy(gameObject);
	}

    public void TakeDamage(int damage)
	{
        playerHealth -= damage;
	}

    public int GetHealth()
	{
        return playerHealth;
	}

    public Vector3 GetPosition()
	{
        return transform.position;
	}
}