using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Movement : MonoBehaviour
{
    public float walkSpeed = 4f;
    public float maxVelocityChange = 10f;
    public float sprintSpeed = 14f;
   

    [Space]
    public float airControl = 0.5f; 

    [Space]
    public float jumpHeight = 5f; 


    private Vector2 input;
    private Rigidbody rb;

    private bool sprinting;
    private bool jumping;

    private bool grounded = false;

    public Animator animator;

    int isRunningHash;

    //float adsCamFov;
    


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //animator = GetComponentInChildren<Animator>();
        isRunningHash = Animator.StringToHash("isRunning");

        //adsCamFov = 60f;

       // Camera.main.fieldOfView = 60;
    }

   
    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        sprinting = Input.GetButton("Sprint");
        jumping = Input.GetButton("Jump");

        if (!sprinting)
        {
            animator.SetBool(isRunningHash, false);
        }

        if (Input.GetMouseButton(1))
        {
            Camera.main.fieldOfView = 30;  
        }

        if (!Input.GetMouseButton(1))
        {
            Camera.main.fieldOfView = 60;
        }


    }

    

    private void OnTriggerStay(Collider other)
    {
        grounded = true;
    }


    void FixedUpdate()
    {

        if (grounded)
        {
            if (jumping)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
            }

           else if (input.magnitude > 0.5f)
            {
                rb.AddForce(CalculateMovement(sprinting ? sprintSpeed : walkSpeed), ForceMode.VelocityChange);
                Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
                

                    animator.SetFloat("X", localVelocity.x);
                    animator.SetFloat("Y", localVelocity.z);
                    animator.SetBool("isMoving", true);

                if (sprinting )
                {
                    animator.SetBool(isRunningHash, true);
                }
                   

            }

   

            else
            {
                var velocity1 = rb.velocity;
                velocity1 = new Vector3(velocity1.x * 0.2f * Time.fixedDeltaTime, velocity1.y, velocity1.z * 0.2f * Time.fixedDeltaTime);
                rb.velocity = velocity1;
                animator.SetBool("isMoving", false);


            }

            
        }

        else
        {
            if (input.magnitude > 0.5f)
            {
                rb.AddForce(CalculateMovement(sprinting ? sprintSpeed * airControl: walkSpeed * airControl), ForceMode.VelocityChange);
            }
            else
            {
                var velocity1 = rb.velocity;
                velocity1 = new Vector3(velocity1.x * 0.2f * Time.fixedDeltaTime, velocity1.y, velocity1.z * 0.2f * Time.fixedDeltaTime);
                rb.velocity = velocity1;
            }
        }

        grounded = false; 
    }




    Vector3 CalculateMovement(float _speed)
    {
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);
        targetVelocity = transform.TransformDirection(targetVelocity);


        targetVelocity *= _speed;

        Vector3 velocity = rb.velocity;

        if (input.magnitude > 0.5f)
            {
            Vector3 velocityChange = targetVelocity - velocity;

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

            velocityChange.y = 0;

            return (velocityChange);


            }

            else
        {
            return new Vector3();
        }

    }



}
