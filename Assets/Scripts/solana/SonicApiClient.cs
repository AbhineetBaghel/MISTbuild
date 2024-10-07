using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class SonicApiClient : MonoBehaviour
{
    private const string API_BASE_URL = "http://localhost:3000"; // Change this when you deploy your server

    public async Task<bool> InitializeGame()
    {
        string url = $"{API_BASE_URL}/initialize";
        string json = JsonConvert.SerializeObject(new {});
        
        var response = await SendPostRequest(url, json);
        return response.success;
    }

    public async Task<string> CreateRoom(string creatorPublicKey)
    {
        string url = $"{API_BASE_URL}/create-room";
        string json = JsonConvert.SerializeObject(new { creatorPublicKey });
        
        var response = await SendPostRequest(url, json);
        return response.success ? response.roomId : null;
    }

    public async Task<bool> JoinRoom(string playerPublicKey, string roomId)
    {
        string url = $"{API_BASE_URL}/join-room";
        string json = JsonConvert.SerializeObject(new { playerPublicKey, roomId });
        
        var response = await SendPostRequest(url, json);
        return response.success;
    }

    public async Task<bool> EndGame(string roomId, string winnerPublicKey)
    {
        string url = $"{API_BASE_URL}/end-game";
        string json = JsonConvert.SerializeObject(new { roomId, winnerPublicKey });
        
        var response = await SendPostRequest(url, json);
        return response.success;
    }

    public async Task<RoomData> GetRoomData(string roomId)
    {
        string url = $"{API_BASE_URL}/room/{roomId}";
        
        var response = await SendGetRequest(url);
        return response.success ? JsonConvert.DeserializeObject<RoomData>(response.roomData.ToString()) : null;
    }

    private async Task<dynamic> SendPostRequest(string url, string jsonBody)
    {
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, jsonBody))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                return new { success = false, error = www.error };
            }
            else
            {
                return JsonConvert.DeserializeObject<dynamic>(www.downloadHandler.text);
            }
        }
    }

    private async Task<dynamic> SendGetRequest(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                return new { success = false, error = www.error };
            }
            else
            {
                return JsonConvert.DeserializeObject<dynamic>(www.downloadHandler.text);
            }
        }
    }
}

public class RoomData
{
    public string creator { get; set; }
    public string stakingAmount { get; set; }
    public string[] players { get; set; }
    public int state { get; set; }
    public string creationTime { get; set; }
    public string winner { get; set; }
    public string roomId { get; set; }
}