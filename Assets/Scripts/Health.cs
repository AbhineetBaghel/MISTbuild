using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.Animations.Rigging;
using System.Threading.Tasks;
using Unity.VisualScripting;


public class Health : MonoBehaviour
{
    public int health;
    public bool isLocalPlayer;
    Animator animator;

    public bool isDying = false;

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
    public async void TakeDamage(int _damage)
    {
        if (isDying == false)
        {
            health -= _damage;
        }
        

        healthText.text = health.ToString();

        if (health <= 0 && isDying == false)
        {
            // Destroy(RigBuilder);
            //disableRigForDeathAniamation.SetActive(false);
            isDying = true;
            

            GetComponent<Movement>().enabled = false;
            GetComponent<MouseLook>().enabled = false;
           

            rig.enabled= false ;

            animator.SetBool("isDead", true);

            Debug.Log("isdead");

            //  Destroy(gameObject, this.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).length);

            Destroy(gameObject, 2);

            await Task.Delay(3000);
            health = 100;

            if (isLocalPlayer)
            {
                
                Debug.Log("spawn1");
                RoomManager.instance.SpawnPlayer();
                //GetComponent<MouseLook>().enabled = true;

                

            }

            // Destroy(gameObject,1);

            // Destroy(gameObject, this.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).length);

            isDying = false;

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