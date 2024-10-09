using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class Weapon : MonoBehaviour
{
    public int damage;

    public Image ammoCircle;

    public Camera camera;

    public float fireRate;

    [Header("VFX")]
    public GameObject hitVFX;
    public GameObject hitVFX2;
    //public GameObject hit1VFX;



    private float nextFire;

    [Header("Ammo")]
    public int mag = 5;

    public int ammo = 30;
    public int magAmmo = 30;

    [Header("UI")]
    public TextMeshProUGUI magText;
    public TextMeshProUGUI ammoText;

    //[Header("Animation")]
    //public Animation animation;
    //public AnimationClip reload;

    [Header("Recoil Settings")]
    //[Range(0, 1)]
   // public float recoilPercent = 0.3f;

    [Range(0, 2)]
    public float recoverPercent = 0.7f;

    [Space]
    public float recoilUp = 1f;

    public float recoilBack = 0f;


    private Vector3 originalPosition;
    private Vector3 recoilVelocity = Vector3.zero;

    private float recoilLength;
    private float recoverLength;


    private bool recoiling;
    private bool recovering;

    AudioSource shootingSound;

   
    void SetAmmo()
    {
        ammoCircle.fillAmount = (float)ammo / magAmmo;
    }


     void Start()
    {
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;
        SetAmmo();

        originalPosition = transform.localPosition;


        recoilLength = 0;
        recoverLength = 1 / fireRate * recoverPercent;

        shootingSound = GetComponent<AudioSource>();

        

        
    }
    void Update()
    {

        hitVFX.SetActive(true);
        if (nextFire > 0)
            nextFire -= Time.deltaTime;

    //   if (Input.GetButton("Fire1") && nextFire <= 0 && ammo > 0)     //&& animation.isPlaying == false)

    if (CrossPlatformInputManager.GetButton("Shoot") && nextFire <= 0 && ammo > 0)
        {
            nextFire = 1 / fireRate;

            ammo--;

            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;
            SetAmmo();

            shootingSound.Play();

             
           

            Fire();
            

        }

        if (Input.GetKeyDown(KeyCode.R) && mag > 0)
        {
            
            Reload();
           

        }

        if (recoiling)
        {
            Recoil();
        }

        if (recovering)
        {
            Recovering();
        }

    }

    void Reload()
    {
       
        
        //animation.Play(reload.name);
       

        if (mag>0)
        {
            mag--;

            ammo = magAmmo;
        }

        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;
        SetAmmo();

        


    }

    void Fire()
    {

        recoiling = true;
        recovering = false;

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);

        RaycastHit hit;

        /* if (Physics.Raycast(ray.origin, ray.direction, out hit, 100f))
         {
             hitVFX.SetActive(false);
             hitVFX2.SetActive(true);
             PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);

             if (hit.transform.gameObject.GetComponent<Health>())
             {

                 PhotonNetwork.LocalPlayer.AddScore(damage);

                 if (damage > hit.transform.gameObject.GetComponent<Health>().health)
                 {
                     //kill
                     PhotonNetwork.LocalPlayer.AddScore(100);
                 }


               hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage",RpcTarget.All, damage);
             }
         }
        */


        if (Physics.Raycast(ray.origin, ray.direction, out hit, 100f))
        {
            if (hit.transform.gameObject.GetComponent<Health>())
            {
                hitVFX.SetActive(false);
                hitVFX2.SetActive(true);
                PhotonNetwork.Instantiate(hitVFX2.name, hit.point, Quaternion.identity);

                PhotonNetwork.LocalPlayer.AddScore(damage);
                if (damage > hit.transform.gameObject.GetComponent<Health>().health)
                {
                    PhotonNetwork.LocalPlayer.AddScore(100);
                }
                hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);
            }
            else
            {
                hitVFX.SetActive(true);
                hitVFX2.SetActive(false);
                PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);
            }
        }
    }

    void Recoil()
    {
        Vector3 finalPosition = new Vector3(originalPosition.x, originalPosition.y + recoilUp, originalPosition.z - recoilBack);

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition,ref recoilVelocity, recoilLength);


        if (transform.localPosition == finalPosition)
        {
            recoiling = false;
            recovering = true;
        }
    }

    void Recovering()
    {
        Vector3 finalPosition = originalPosition;

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoverLength);


        if (transform.localPosition == finalPosition)
        {
            recoiling = false;
            recovering = false;
        }
    }


}