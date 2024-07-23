using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.Animations.Rigging;


public class Health : MonoBehaviour
{
    public int health;
    public bool isLocalPlayer;
    Animator animator;

    //public GameObject disableRigForDeathAniamation;

   // public RigBuilder afterDeath;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    public RigBuilder rig;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        
    }

    [PunRPC]
    public void TakeDamage(int _damage)
    {
        health -= _damage;

        healthText.text = health.ToString();

        if (health <= 0 )
        {
            // Destroy(RigBuilder);
            //disableRigForDeathAniamation.SetActive(false);

            rig.enabled= false ;

            animator.SetBool("isDead", true);

            Debug.Log("isdead");

            Destroy(gameObject, this.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).length);

            if (isLocalPlayer)

            RoomManager.instance.SpawnPlayer();

           // Destroy(gameObject);

            Debug.Log("dead");
        }

    }

}


/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Health : MonoBehaviour
{
    public int health;
    public bool isLocalPlayer;
    Animator animator;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    [PunRPC]
    public void TakeDamage(int _damage)
    {
        health -= _damage;

        healthText.text = health.ToString();

        if (health <= 0)
        {
            animator.SetBool("isDying", true);

            if (isLocalPlayer)
            {
                StartCoroutine(HandleDeath());
            }
        }
    }

    private IEnumerator HandleDeath()
    {
        // Assume your death animation length is 3 seconds. Adjust as necessary.
        yield return new WaitForSeconds(3f);

        // Respawn player
        RoomManager.instance.SpawnPlayer();

        // Destroy the current game object
        Destroy(gameObject);
    }
}
*/