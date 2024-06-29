using System.Collections;
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

        if (health <= 0 )
        {
            
            if (isLocalPlayer)

            RoomManager.instance.SpawnPlayer();


             Destroy(gameObject);
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