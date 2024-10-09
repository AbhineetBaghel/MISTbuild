using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{

    public static RoomManager instance;

    public GameObject player;

    [Space]
    public Transform[] spawnPoints;

    [Space]
    public GameObject roomCam;

    [Space]
    public GameObject nameUI;
    public GameObject connectingUI;

    private string nickname = "unnanmed";

    public string roomNameToJoin = "test";

    private GameTimer gameTimer;

  //  public GameObject createScreen;
   // public GameObject joinScreen;

    void Awake()
    {
        instance = this;
        gameTimer = GetComponent<GameTimer>();
    }

    public void ChangeNickname(string _name)
    {
        nickname = _name;
    }

    public void JoinRoomButtonPressed()
    {
       Debug.Log("Connecting..");

       PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, null, null);

        nameUI.SetActive(false);
       connectingUI.SetActive(true);
    }

  /*  public  void JoinRoomButtonPressed(string _namee)
    {
        //Debug.Log("Connecting.. custom room create");

     //   PhotonNetwork.JoinOrCreateRoom(_namee, null, null);

       // joinScreen.SetActive(false);
       // createScreen.SetActive(true);

        nameUI.SetActive(false);
        connectingUI.SetActive(true);
    }

    */



    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("We are Connected and in a room now");

        roomCam.SetActive(false);

        //enable the mobile controls from here

        SpawnPlayer();

        if (PhotonNetwork.IsMasterClient)
        {
            StartGame();
        }

       
    }

    private void StartGame()
    {
        if (gameTimer != null)
        {
            gameTimer.StartTimer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];


        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
        //_player.GetComponent<playerBody>().isLocalPlayer = true;
        _player.GetComponent<PlayerSetup>().IsLocalPlayer();
        _player.GetComponent<Health>().isLocalPlayer = true;
       


        _player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBuffered, nickname);
        PhotonNetwork.LocalPlayer.NickName = nickname;

        if (PhotonNetwork.IsMasterClient)
        {
            StartGame();
        }
    }


}
