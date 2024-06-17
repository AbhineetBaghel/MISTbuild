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
            if(isLocalPlayer)
            RoomManager.instance.SpawnPlayer();

            animator.SetBool("isDying", true);

             Destroy(gameObject);
        }
    }

}
