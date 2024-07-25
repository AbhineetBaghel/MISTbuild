using System.Text;
using Solana.Unity.Wallet;
using UnityEngine;
using Solana.Unity.SDK;
using Solana.Unity.Rpc.Models;
using Game.Program;
using Game;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using TMPro;
using UnityEngine.UI;
using Solana.Unity.Programs;
using System.Collections.Generic;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Solana.Unity.Rpc.Core.Http;

// using Newtonsoft.Json;


public class SolanaManager : MonoBehaviour
{
    public static PublicKey programId = new("EpwFSsE5z58Tc9MrUa16868pkUG43uAXY6edjyJw35bq");
    private PublicKey globalStatePDA;
    public TextMeshProUGUI availableRoomsText;
    private List<string> availableRooms = new List<string>();
    // public TMP_Dropdown roomDropdown; // Add reference to your Dropdown
    public Dropdown dropdown;

    public GameObject joinGameRoom;
    public GameObject disableConnectRoom;


    //public RoomManager fakeRoomManager;

    private void Awake()
    {

        //fakeRoomManager = GameObject.FindObjectOfType(typeof(RoomManager)) as RoomManager;
        Web3.OnLogin += _ =>
        {
            PublicKey.TryFindProgramAddress(new[]{
                Encoding.UTF8.GetBytes("global-state")
            }, programId, out globalStatePDA, out var bump);

            CheckGlobalStateInitialized();
        };
    }
    private async void CheckGlobalStateInitialized()
    {
        var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
        var accountInfo = await rpcClient.GetAccountInfoAsync(globalStatePDA);

        if (accountInfo.Result != null && accountInfo.Result.Value != null)
        {
            // Global state is initialized
            FetchTotalRooms();
            FetchAvailableRooms();
            Debug.LogError("Global state init");
        }
        else
        {
            Debug.LogError("Global state is not initialized.");
        }
    }

    public async Task FetchTotalRooms()
    {
        Debug.Log("Fetching total rooms...");
        var rpcClient = ClientFactory.GetClient(Cluster.DevNet);

        try
        {
            var accountInfo = await rpcClient.GetAccountInfoAsync(globalStatePDA);

            if (accountInfo.WasSuccessful && accountInfo.Result.Value != null)
            {
                var globalStateData = accountInfo.Result.Value.Data;
                var globalState = Convert.FromBase64String(globalStateData[0]);
                Debug.Log($"Global state raw data: {BitConverter.ToString(globalState)}");

                // Extract total rooms from bytes 8-11 (index 8 to 11)
                int totalRooms = BitConverter.ToInt32(globalState, 8);
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
        var rpcClient = ClientFactory.GetClient(Cluster.DevNet);

        try
        {
            var accountInfo = await rpcClient.GetAccountInfoAsync(globalStatePDA);

            if (accountInfo.WasSuccessful && accountInfo.Result.Value != null)
            {
                var globalStateData = accountInfo.Result.Value.Data;
                var globalState = Convert.FromBase64String(globalStateData[0]);
                Debug.Log($"Global state raw data: {BitConverter.ToString(globalState)}");
                int totalRoomsCount = BitConverter.ToInt32(globalState, 8);
                List<string> rooms = new List<string>();

                for (int i = 0; i < totalRoomsCount; i++)
                {
                    byte[] roomIdBytes = new byte[8];
                    BigInteger bigI = new BigInteger(i);
                    byte[] bigIBytes = bigI.ToByteArray();
                    Array.Copy(bigIBytes, roomIdBytes, Math.Min(bigIBytes.Length, 8));

                    PublicKey.TryFindProgramAddress(new[]{
                    Encoding.UTF8.GetBytes("room"),
                    roomIdBytes
                }, programId, out PublicKey roomPDA, out var bump);

                    Debug.Log($"Fetching room {i + 1} at {roomPDA}");

                    try
                    {
                        var roomAccountInfo = await rpcClient.GetAccountInfoAsync(roomPDA);

                        if (roomAccountInfo.WasSuccessful && roomAccountInfo.Result.Value != null)
                        {
                            rooms.Add($"Room {i + 1}");
                            availableRooms.Add(roomPDA.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("Account does not exist"))
                        {
                            Debug.Log($"Room {i} does not exist, skipping.");
                        }
                        else
                        {
                            Debug.LogError($"Error fetching room {i}: {e.Message}");
                        }
                    }
                }

                // availableRoomsText.text = $"Available Rooms: {string.Join(", ", rooms)}";
                // availableRooms = rooms;

                // roomDropdown.ClearOptions();
                // roomDropdown.AddOptions(availableRooms);
                availableRoomsText.text = $"Available Rooms actual: {string.Join(", ", rooms)}";
                Debug.Log($"Available Rooms: {rooms}");
                // UpdateAvailableRoomsText();
                if (dropdown != null && availableRooms.Count > 0)
                {
                    dropdown.options.Clear();
                    foreach (string option in availableRooms)
                    {
                        dropdown.options.Add(new Dropdown.OptionData(option));
                    }
                }
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

    private void UpdateAvailableRoomsText()
    {
        availableRoomsText.text = string.Join("\n", availableRooms);
    }

    private async void CreateRoomAsync()
    {
        Debug.Log("Creating room...");
        var rpcClient = ClientFactory.GetClient(Cluster.DevNet);

        try
        {
            if (Web3.Account == null)
            {
                Debug.LogError("Please connect your wallet first.");
                return;
            }

            var globalStateInfo = await rpcClient.GetAccountInfoAsync(globalStatePDA);
            if (globalStateInfo.Result == null || globalStateInfo.Result.Value == null)
            {
                Debug.LogError("Please initialize the global state first.");
                return;
            }

            var globalStateData = globalStateInfo.Result.Value.Data;
            var globalState = Game.Accounts.GlobalState.Deserialize(Convert.FromBase64String(globalStateData[0]));
            ulong currentRoomId = globalState.TotalRooms;
            // ulong currentRoomId = 6;

            Debug.Log($"Current room ID: {currentRoomId}");

            byte[] roomIdBytes = BitConverter.GetBytes(currentRoomId);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(roomIdBytes);

            // Find Program Address
            PublicKey.TryFindProgramAddress(new[]{
            Encoding.UTF8.GetBytes("room"),
            roomIdBytes
        }, programId, out PublicKey roomPDA, out var bump);

            Debug.Log($"Room PDA: {roomPDA}");
            Debug.Log($"globalStatePDA: {globalStatePDA}");
            Debug.Log($"Web3.Account.PublicKey: {Web3.Account.PublicKey}");
            Debug.Log($"SystemProgram.ProgramIdKey: {SystemProgram.ProgramIdKey}");

            var createRoomAccounts = new Game.Program.CreateRoomAccounts()
            {
                Room = roomPDA,
                GlobalState = globalStatePDA,
                Creator = Web3.Account.PublicKey,
                SystemProgram = SystemProgram.ProgramIdKey
            };

            var recentBlockHash = await rpcClient.GetRecentBlockHashAsync();
            if (!recentBlockHash.WasSuccessful)
            {
                Debug.LogError($"Failed to get recent block hash: {recentBlockHash.Reason}");
                return;
            }

            var instruction = Game.Program.GameProgram.CreateRoom(createRoomAccounts, programId);

            Debug.Log($"Instruction: {instruction}");

            var tx = new Transaction()
            {
                RecentBlockHash = recentBlockHash.Result.Value.Blockhash,
                FeePayer = Web3.Account.PublicKey,
                Instructions = new List<TransactionInstruction> { instruction },
                Signatures = new List<SignaturePubKeyPair>()
            };


            Debug.Log($"Transaction details: {tx}");

            RequestResult<string> result = await Web3.Wallet.SignAndSendTransaction(tx);
            if (result.WasSuccessful)
            {
                Debug.Log($"Transaction signature: {result.Result}");
                Debug.Log($"Room created at: {roomPDA}");

                // GetComponent<RoomManager>().JoinRoomButtonPressed();
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
    public void OnCreateRoomButtonClick()
    {
        CreateRoomAsync();
    }

    private async void JoinRoomAsync()
    {
        Debug.Log("Joining room...");
        var rpcClient = ClientFactory.GetClient(Cluster.DevNet);

        try
        {
            if (Web3.Account == null)
            {
                Debug.LogError("Please connect your wallet first.");
                return;
            }

            if (dropdown.options.Count == 0)
            {
                Debug.LogError("Please select a room to join.");
                return;
            }

            string selectedRoom = dropdown.options[dropdown.value].text;
            PublicKey roomPDA = new PublicKey(selectedRoom);

            var joinRoomAccounts = new Game.Program.JoinRoomAccounts()
            {
                Room = roomPDA,
                Player = Web3.Account.PublicKey,
                SystemProgram = SystemProgram.ProgramIdKey
            };

            var recentBlockHash = await rpcClient.GetRecentBlockHashAsync();
            if (!recentBlockHash.WasSuccessful)
            {
                Debug.LogError($"Failed to get recent block hash: {recentBlockHash.Reason}");
                return;
            }

            var instruction = Game.Program.GameProgram.JoinRoom(joinRoomAccounts, programId);

            var tx = new Transaction()
            {
                RecentBlockHash = recentBlockHash.Result.Value.Blockhash,
                FeePayer = Web3.Account.PublicKey,
                Instructions = new List<TransactionInstruction> { instruction },
                Signatures = new List<SignaturePubKeyPair>()
            };

            RequestResult<string> result = await Web3.Wallet.SignAndSendTransaction(tx);
            if (result.WasSuccessful)
            {
                Debug.Log($"Transaction signature: {result.Result}");
                Debug.Log($"Joined room: {roomPDA}");
                joinGameRoom.SetActive(true);
                disableConnectRoom.SetActive(false);
                // await FetchAvailableRooms(); // Refresh the room list
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

    public void OnJoinRoomButtonClick()
    {
        JoinRoomAsync();
    }
}
