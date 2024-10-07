using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAnimationController : MonoBehaviour
{
    // Start is called before the first frame update

    Animator animator;
    int isWalkingHash;
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
    }

    // Update is called once per frame
    void Update()
    {
        bool isWalking = animator.GetBool("isWalking");
        bool forwardPressed = Input.GetKey("w") || Input.GetKey("up");

        if (!isWalking && forwardPressed)
        {
            animator.SetBool(isWalkingHash, true);
        }

        if (isWalking && !forwardPressed)
        {
            animator.SetBool(isWalkingHash, false);
        }

        
    }
}
