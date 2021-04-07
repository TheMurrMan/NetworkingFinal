using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private float verticleVelocity;
    private int jumpForce = 10;
    private float gravity = 14.0f;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputs = Vector3.zero;

        inputs.x = Input.GetAxis("Horizontal");

        if(controller.isGrounded)
		{
            verticleVelocity = -1;

            if(Input.GetKeyDown(KeyCode.Space))
			{
                verticleVelocity = jumpForce;
			}
		}

        else
		{
            verticleVelocity -= gravity * Time.deltaTime;
		}

        inputs.y = verticleVelocity;

        controller.Move(inputs * Time.deltaTime);
    }
}
