/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class playerBody : MonoBehaviour
{
    public bool isLocalPlayer;
    public GameObject playerBodyToShow;

    void Start()
    {
        playerBodyToShow = GameObject.Find("/TestPlayer/SK_Character_Samurai_Ninja_01");
    }

    [PunRPC]
    public void HideCharacter()
    {
        if (isLocalPlayer)
        {
            RoomManager.instance.SpawnPlayer();
           
            playerBodyToShow.SetActive(false);
        }
    }
}
*/