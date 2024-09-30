using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Messages;
using Solana.Unity.SDK;
using Solana.Unity.Programs;
using Game.Program;
using Game.Accounts;
using static System.Net.WebRequestMethods;

public class SolanaManager : MonoBehaviour
{
    public static PublicKey ProgramId = new("E6XgSpPs7fGYnrQUVtG1CixuZS1GAdYU3XCs3AbkWXpV");
    private PublicKey _globalStatePDA;
    public TextMeshProUGUI availableRoomsText;
    private List<string> availableRooms = new List<string>();
    public Dropdown roomDropdown;

    public GameObject joinGameRoom;
    public GameObject disableConnectRoom;

    private void Awake()
    {
        Web3.OnLogin += OnLogin;
    }

    private void OnDestroy()
    {
        Web3.OnLogin -= OnLogin;
    }

    private void OnLogin(Account _)
    {
        PublicKey.TryFindProgramAddress(new[]{
            Encoding.UTF8.GetBytes("global-state")
        }, ProgramId, out _globalStatePDA, out var _);

        CheckGlobalStateInitialized();
    }

    private async void CheckGlobalStateInitialized()
    {
        // var rpcClient = ClientFactory.GetClient(Cluster.TestNet);
        var rpcClient = ClientFactory.GetClient("https://api.testnet.sonic.game");
        var accountInfo = await rpcClient.GetAccountInfoAsync(_globalStatePDA);

        if (accountInfo.Result?.Value != null)
        {
            Debug.Log("Global state initialized");
            await FetchTotalRooms();
            await FetchAvailableRooms();
        }
        else
        {
            Debug.LogError("Global state is not initialized.");
        }
    }

    public async Task FetchTotalRooms()
    {
        Debug.Log("Fetching total rooms...");
        // var rpcClient = ClientFactory.GetClient(Cluster.TestNet);
       var rpcClient = ClientFactory.GetClient("https://api.testnet.sonic.game");

        try
        {
            var accountInfo = await rpcClient.GetAccountInfoAsync(_globalStatePDA);

            if (accountInfo.WasSuccessful && accountInfo.Result?.Value != null)
            {
                var globalStateData = Convert.FromBase64String(accountInfo.Result.Value.Data[0]);
                var totalRooms = BitConverter.ToInt32(globalStateData, 8);
                Debug.Log($"Total Rooms Created: {totalRooms}");
            }
            else
            {
                Debug.LogError("Failed to fetch global state.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch total rooms: {e.Message}");
        }
    }

    public async Task FetchAvailableRooms()
    {
        Debug.Log("Fetching available rooms...");
        // var rpcClient = ClientFactory.GetClient(Cluster.TestNet);
        var rpcClient = ClientFactory.GetClient("https://api.testnet.sonic.game");

        try
        {
            var accountInfo = await rpcClient.GetAccountInfoAsync(_globalStatePDA);

            if (accountInfo.WasSuccessful && accountInfo.Result?.Value != null)
            {
                var globalStateData = Convert.FromBase64String(accountInfo.Result.Value.Data[0]);
                int totalRoomsCount = BitConverter.ToInt32(globalStateData, 8);
                List<string> rooms = new List<string>();

                for (int i = 0; i < totalRoomsCount; i++)
                {
                    byte[] roomIdBytes = BitConverter.GetBytes((ulong)i);
                    PublicKey.TryFindProgramAddress(new[]{
                        Encoding.UTF8.GetBytes("room"),
                        roomIdBytes
                    }, ProgramId, out PublicKey roomPDA, out var _);

                    var roomAccountInfo = await rpcClient.GetAccountInfoAsync(roomPDA);

                    if (roomAccountInfo.WasSuccessful && roomAccountInfo.Result?.Value != null)
                    {
                        rooms.Add($"Room {i + 1}");
                        availableRooms.Add(roomPDA.ToString());
                    }
                }

                availableRoomsText.text = $"Available Rooms: {string.Join(", ", rooms)}";
                UpdateDropdown(availableRooms);
            }
            else
            {
                Debug.LogError("Failed to fetch global state.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch available rooms: {e.Message}");
        }
    }

    private void UpdateDropdown(List<string> options)
    {
        if (roomDropdown != null && options.Count > 0)
        {
            roomDropdown.ClearOptions();
            roomDropdown.AddOptions(options);
        }
    }

    public async void CreateRoom()
    {
        Debug.Log("Creating room...");
        // var rpcClient = ClientFactory.GetClient(Cluster.TestNet);
        var rpcClient = ClientFactory.GetClient("https://api.testnet.sonic.game");

        try
        {
            if (Web3.Account == null)
            {
                Debug.LogError("Please connect your wallet first.");
                return;
            }

            var globalStateInfo = await rpcClient.GetAccountInfoAsync(_globalStatePDA);
            if (globalStateInfo.Result?.Value == null)
            {
                Debug.LogError("Please initialize the global state first.");
                return;
            }

            var globalStateData = globalStateInfo.Result.Value.Data;
            var globalState = GlobalState.Deserialize(Convert.FromBase64String(globalStateData[0]));
            ulong currentRoomId = globalState.TotalRooms;

            byte[] roomIdBytes = BitConverter.GetBytes(currentRoomId);
            PublicKey.TryFindProgramAddress(new[]{
                Encoding.UTF8.GetBytes("room"),
                roomIdBytes
            }, ProgramId, out PublicKey roomPDA, out var _);

            var createRoomAccounts = new CreateRoomAccounts
            {
                Room = roomPDA,
                GlobalState = _globalStatePDA,
                Creator = Web3.Account.PublicKey,
                SystemProgram = SystemProgram.ProgramIdKey
            };

            var latestBlockhashResponse = await rpcClient.GetLatestBlockHashAsync();
            if (!latestBlockhashResponse.WasSuccessful)
            {
                Debug.LogError($"Failed to get latest block hash: {latestBlockhashResponse.Reason}");
                return;
            }

            var instruction = GameProgram.CreateRoom(createRoomAccounts, ProgramId);

            var tx = new Transaction
            {
                RecentBlockHash = latestBlockhashResponse.Result.Value.Blockhash,
                FeePayer = Web3.Account.PublicKey,
                Instructions = new List<TransactionInstruction> { instruction },
                Signatures = new List<SignaturePubKeyPair>()
            };

            RequestResult<string> result = await Web3.Wallet.SignAndSendTransaction(tx);
            if (result.WasSuccessful)
            {
                Debug.Log($"Room created at: {roomPDA}");
                joinGameRoom.SetActive(true);
                disableConnectRoom.SetActive(false);

                await FetchTotalRooms();
                await FetchAvailableRooms();
            }
            else
            {
                Debug.LogError($"Transaction failed: {result.Reason}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating room: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    public async void JoinRoom()
    {
        Debug.Log("Joining room...");
        // var rpcClient = ClientFactory.GetClient(Cluster.TestNet);
        var rpcClient = ClientFactory.GetClient("https://api.testnet.sonic.game");

        try
        {
            if (Web3.Account == null)
            {
                Debug.LogError("Please connect your wallet first.");
                return;
            }

            if (roomDropdown.options.Count == 0)
            {
                Debug.LogError("Please select a room to join.");
                return;
            }

            string selectedRoom = roomDropdown.options[roomDropdown.value].text;
            PublicKey roomPDA = new PublicKey(selectedRoom);

            var joinRoomAccounts = new JoinRoomAccounts
            {
                Room = roomPDA,
                Player = Web3.Account.PublicKey,
                SystemProgram = SystemProgram.ProgramIdKey
            };

            var latestBlockhashResponse = await rpcClient.GetLatestBlockHashAsync();
            if (!latestBlockhashResponse.WasSuccessful)
            {
                Debug.LogError($"Failed to get latest block hash: {latestBlockhashResponse.Reason}");
                return;
            }

            var instruction = GameProgram.JoinRoom(joinRoomAccounts, ProgramId);

            var tx = new Transaction
            {
                RecentBlockHash = latestBlockhashResponse.Result.Value.Blockhash,
                FeePayer = Web3.Account.PublicKey,
                Instructions = new List<TransactionInstruction> { instruction },
                Signatures = new List<SignaturePubKeyPair>()
            };

            RequestResult<string> result = await Web3.Wallet.SignAndSendTransaction(tx);
            if (result.WasSuccessful)
            {
                Debug.Log($"Joined room: {roomPDA}");
                joinGameRoom.SetActive(true);
                disableConnectRoom.SetActive(false);
            }
            else
            {
                Debug.LogError($"Transaction failed: {result.Reason}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error joining room: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    public void OnCreateRoomButtonClick()
    {
        CreateRoom();
    }

    public void OnJoinRoomButtonClick()
    {
        JoinRoom();
    }
}