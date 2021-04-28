using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5;
    public Vector3 movement = Vector3.zero;

    private Camera cam;
    private Rigidbody rb;

    public GameObject bulletPrefab;
    public Transform gunEnd;

    public Slider healthbar;

    private Client client;
    [SerializeField] private int playerHealth = 100;
	private void Start()
	{
        rb = GetComponent<Rigidbody>();
        cam = FindObjectOfType<Camera>();
        client = FindObjectOfType<Client>();

        gunEnd = transform.GetChild(2);
        bulletPrefab = Resources.Load("Bullet") as GameObject;

        healthbar = GetComponentInChildren<Slider>();

        healthbar.maxValue = playerHealth;
	}
	// Update is called once per frame
	void Update()
    {
        GetPlayerInput();

        playerHealth = client.ourHealth;
        
        if (playerHealth <= 0)
		{
            Debug.Log("DIE");
            Die();
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

            Quaternion targetRot = Quaternion.LookRotation(pointToLook - transform.position);
            targetRot.x = 0;
            targetRot.z = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 7f * Time.deltaTime);

            //transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
		}

        if(Input.GetMouseButtonDown(0))
		{
            SpawnBullet();
		}
    }

    void SpawnBullet()
    {
	    GameObject g = Instantiate(bulletPrefab, gunEnd.position, gunEnd.rotation);
        //Client.m_Instance.OnBulletSpawn(g);
        FindObjectOfType<Client>().OnBulletSpawn(g);
        Destroy(g);
	}

    void Die()
	{
        //Destroy(gameObject);
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
