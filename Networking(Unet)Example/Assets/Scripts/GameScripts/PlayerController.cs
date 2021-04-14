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
    
    [SerializeField] private int playerHealth = 100;
	private void Start()
	{
        rb = GetComponent<Rigidbody>();
        cam = FindObjectOfType<Camera>();

        gunEnd = transform.GetChild(2);
        bulletPrefab = Resources.Load("Bullet") as GameObject;

        healthbar = GetComponentInChildren<Slider>();

        healthbar.maxValue = playerHealth;
	}
	// Update is called once per frame
	void Update()
    {
        GetPlayerInput();

        healthbar.value = playerHealth;
        if (playerHealth <=0)
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
