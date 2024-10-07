using UnityEngine;
using UnityEngine.Networking;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using System.Collections;
using System.Text;

public class ApiManager : MonoBehaviour
{
    private string apiUrl = "http://localhost:3000"; // Replace with your API server URL

    private string playerPublicKey;

    private void Start()
    {
        // Ensure the player is logged in
        StartCoroutine(EnsureLoggedIn());
    }

    private IEnumerator EnsureLoggedIn()
    {
        if (Web3.Account == null)
        {
            // If the player is not logged in, call the login function
            yield return StartCoroutine(LoginPlayer());
        }

        // Fetch the player's public key after login
        playerPublicKey = Web3.Account.PublicKey.ToString();
    }

    private IEnumerator LoginPlayer()
    {
        Debug.Log("Logging in...");
        var loginTask = Web3.Instance.LoginWalletAdapter();
        while (!loginTask.IsCompleted)
        {
            yield return null;
        }
        
        if (Web3.Account != null)
        {
            playerPublicKey = Web3.Account.PublicKey.ToString();
            Debug.Log("Logged in successfully! Public Key: " + playerPublicKey);
        }
        else
        {
            Debug.LogError("Login failed.");
        }
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(playerPublicKey))
        {
            Debug.LogError("Player is not logged in. Cannot create room.");
            return;
        }

        // Data for room creation
        var roomData = new { creatorPublicKey = playerPublicKey };
        StartCoroutine(PostRequest("/create-room", roomData));
    }

    public void JoinRoom(string roomId)
    {
        if (string.IsNullOrEmpty(playerPublicKey))
        {
            Debug.LogError("Player is not logged in. Cannot join room.");
            return;
        }

        // Data for joining room
        var roomData = new { playerPublicKey = playerPublicKey, roomId = roomId };
        StartCoroutine(PostRequest("/join-room", roomData));
    }

    private IEnumerator PostRequest(string endpoint, object data)
    {
        string url = apiUrl + endpoint;

        // Serialize the data object to JSON
        string jsonData = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        // Create a UnityWebRequest for the POST
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("API Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
}
