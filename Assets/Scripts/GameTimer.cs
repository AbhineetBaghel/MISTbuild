using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviourPunCallbacks
{
    public float gameDuration = 300f; // 5 minutes in seconds
    private float timeRemaining;
    private bool isGameActive = false;
    public MonoBehaviour[] scriptsToDisable;

    public void StartTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartTimerRPC", RpcTarget.All, PhotonNetwork.Time);
        }
    }

    [PunRPC]
    private void StartTimerRPC(double startTime)
    {
        timeRemaining = gameDuration;
        isGameActive = true;
        // Synchronize the start time across all clients
        if (!PhotonNetwork.IsMasterClient)
        {
            double timeSinceStart = PhotonNetwork.Time - startTime;
            timeRemaining -= (float)timeSinceStart;
        }
    }

    void Update()
    {
        if (isGameActive)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                EndGame();
            }

            // Master client syncs time every second
            if (PhotonNetwork.IsMasterClient && Mathf.FloorToInt(timeRemaining) % 1 == 0)
            {
                photonView.RPC("SyncTime", RpcTarget.All, timeRemaining);
            }
        }
    }

    [PunRPC]
    void SyncTime(float time)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            timeRemaining = time;
        }
    }

    void EndGame()
    {
        Debug.Log(" Over!");
        //isGameActive = false;
        photonView.RPC("GameOver", RpcTarget.All);
        SceneManager.LoadScene("GameOver");

    }

    [PunRPC]
    void GameOver()
    {
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = false;
            }
        }
        Debug.Log("Game Over!");
    }
}