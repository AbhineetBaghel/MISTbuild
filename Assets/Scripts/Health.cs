using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.Animations.Rigging;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UI;






public class Health : MonoBehaviour
{
    public int health;
    public bool isLocalPlayer;
    Animator animator;

    public bool isDying = false;
    public RectTransform healthBar;
    private float originalHealthBarSize;
    public GameObject leaderboardAfterDeath;


    [Header("UI")]
    public TextMeshProUGUI healthText;

    public RigBuilder rig;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        // enableLeaderboardAfterDeath.SetActive(false);
        originalHealthBarSize = healthBar.sizeDelta.x;

    }


    [PunRPC]
    public async void TakeDamage(int _damage)
    {
        if (isDying == false)
        {
            health -= _damage;
            healthBar.sizeDelta = new Vector2(originalHealthBarSize * health / 100f, healthBar.sizeDelta.y);
        }
        

        healthText.text = health.ToString();

        if (health <= 0 && isDying == false)
        {
           
            isDying = true;
            GetComponent<Movement>().enabled = false;
            rig.enabled= false ;

            animator.SetBool("isDead", true);

            Debug.Log("isdead");


            Destroy(gameObject,1);

            leaderboardAfterDeath.SetActive(true);

            // await Task.Delay(3000);
             health = 100;



            if (isLocalPlayer )
            {
                RoomManager.instance.SpawnPlayer();

            }
                

            isDying = false;

            Debug.Log("dead");
            
        }

    }



    
}

