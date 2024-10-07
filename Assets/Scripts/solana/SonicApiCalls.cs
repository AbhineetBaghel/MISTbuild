using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;

public class SonicApiCalls : MonoBehaviour
{
    [SerializeField] private Authentication authScript;
    [SerializeField] private InputField roomIdInput;
    [SerializeField] private Text statusText;
    [SerializeField] private Button initializeButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button endGameButton;
    [SerializeField] private Button getRoomDataButton;

    private SonicApiClient apiClient;
    private string currentRoomId;

    private void Start()
    {
        apiClient = GetComponent<SonicApiClient>();
        if (apiClient == null)
        {
            Debug.LogError("SonicApiClient not found on this GameObject!");
            return;
        }

        if (authScript == null)
        {
            Debug.LogError("Authentication script reference not set!");
            return;
        }

        initializeButton.onClick.AddListener(InitializeGame);
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
        endGameButton.onClick.AddListener(EndGame);
        getRoomDataButton.onClick.AddListener(GetRoomData);

        // Disable buttons until wallet is connected
        SetButtonsInteractable(false);

        // Subscribe to login event
        Web3.OnLogin += OnLogin;
    }

    private void OnDestroy()
    {
        // Unsubscribe from login event
        Web3.OnLogin -= OnLogin;
    }

    private void OnLogin(Account account)
    {
        SetButtonsInteractable(true);
        UpdateStatus($"Logged in with account: {account.PublicKey}");
    }

    private async void InitializeGame()
    {
        try
        {
            bool success = await apiClient.InitializeGame();
            UpdateStatus(success ? "Game initialized successfully" : "Failed to initialize game");
        }
        catch (Exception e)
        {
            HandleError("initializing game", e);
        }
    }

    private async void CreateRoom()
    {
        try
        {
            string creatorPublicKey = Web3.Account.PublicKey.ToString();
            string roomId = await apiClient.CreateRoom(creatorPublicKey);
            if (!string.IsNullOrEmpty(roomId))
            {
                currentRoomId = roomId;
                UpdateStatus($"Room created with ID: {roomId}");
                roomIdInput.text = roomId;
            }
            else
            {
                UpdateStatus("Failed to create room");
            }
        }
        catch (Exception e)
        {
            HandleError("creating room", e);
        }
    }

    private async void JoinRoom()
    {
        string roomId = roomIdInput.text;
        if (string.IsNullOrEmpty(roomId))
        {
            UpdateStatus("Please enter a room ID");
            return;
        }

        try
        {
            string playerPublicKey = Web3.Account.PublicKey.ToString();
            bool success = await apiClient.JoinRoom(playerPublicKey, roomId);
            if (success)
            {
                currentRoomId = roomId;
                UpdateStatus($"Joined room: {roomId}");
            }
            else
            {
                UpdateStatus("Failed to join room");
            }
        }
        catch (Exception e)
        {
            HandleError("joining room", e);
        }
    }

    private async void EndGame()
    {
        if (string.IsNullOrEmpty(currentRoomId))
        {
            UpdateStatus("No active room");
            return;
        }

        try
        {
            string winnerPublicKey = Web3.Account.PublicKey.ToString();
            bool success = await apiClient.EndGame(currentRoomId, winnerPublicKey);
            if (success)
            {
                UpdateStatus("Game ended successfully");
                currentRoomId = null;
            }
            else
            {
                UpdateStatus("Failed to end game");
            }
        }
        catch (Exception e)
        {
            HandleError("ending game", e);
        }
    }

    private async void GetRoomData()
    {
        string roomId = roomIdInput.text;
        if (string.IsNullOrEmpty(roomId))
        {
            UpdateStatus("Please enter a room ID");
            return;
        }

        try
        {
            RoomData roomData = await apiClient.GetRoomData(roomId);
            if (roomData != null)
            {
                UpdateStatus($"Room {roomId} data: Creator: {roomData.creator}, Players: {roomData.players.Length}, State: {roomData.state}");
            }
            else
            {
                UpdateStatus("Failed to get room data");
            }
        }
        catch (Exception e)
        {
            HandleError("getting room data", e);
        }
    }

    private void UpdateStatus(string message)
    {
        statusText.text = message;
        Debug.Log(message);
    }

    private void HandleError(string action, Exception e)
    {
        string errorMessage = $"Error {action}: {e.Message}";
        UpdateStatus(errorMessage);
        Debug.LogError(errorMessage);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        initializeButton.interactable = interactable;
        createRoomButton.interactable = interactable;
        joinRoomButton.interactable = interactable;
        endGameButton.interactable = interactable;
        getRoomDataButton.interactable = interactable;
    }
}