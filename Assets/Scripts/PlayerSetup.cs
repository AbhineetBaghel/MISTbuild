using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerSetup : MonoBehaviour
{
    public Movement movement;
    public GameObject camera;

    public string nickname;

    public TextMeshPro nicknameText;

    public Transform TPweaponHolder;

    public GameObject Playerbody;

  //  public GameObject playerArms;




   //void Start()
  //  {
    //    Playerbody = GameObject.Find("/TestPlayer/SK_Character_Samurai_Ninja_01");
  //  }


    public void IsLocalPlayer()
    {
        movement.enabled = true;
        camera.SetActive(true);

        TPweaponHolder.gameObject.SetActive(false);

       
        Playerbody.SetActive(false);

       // playerArms.SetActive(true);

      

    }

    [PunRPC]
    public void SetTPWeapon(int _weaponIndex)
    {
        foreach (Transform _weapon in TPweaponHolder)
        {
            _weapon.gameObject.SetActive(false);
        }


        TPweaponHolder.GetChild(_weaponIndex).gameObject.SetActive(true);
    }

    [PunRPC]
    public void SetNickname(string _name)
    {
        nickname = _name;

        nicknameText.text = nickname;
    }
}
