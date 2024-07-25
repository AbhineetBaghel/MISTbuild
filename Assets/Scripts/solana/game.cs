using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Solana.Unity;
using Solana.Unity.Programs.Abstract;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Wallet;
using Game;
using Game.Program;
using Game.Errors;
using Game.Accounts;
using Game.Types;

namespace Game
{
    namespace Accounts
    {
        public partial class GlobalState
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 7099216558086041251UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{163, 46, 74, 168, 216, 123, 133, 98};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "UJ4PYxGNvxZ";
            public ulong TotalRooms { get; set; }

            public static GlobalState Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                GlobalState result = new GlobalState();
                result.TotalRooms = _data.GetU64(offset);
                offset += 8;
                return result;
            }
        }

        public partial class Room
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 6825512952964302748UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{156, 199, 67, 27, 222, 23, 185, 94};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "TDwuDPD9pEH";
            public PublicKey Creator { get; set; }

            public ulong StakingAmount { get; set; }

            public PublicKey[] Players { get; set; }

            public GameState State { get; set; }

            public long CreationTime { get; set; }

            public PublicKey Winner { get; set; }

            public ulong RoomId { get; set; }

            public static Room Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Room result = new Room();
                result.Creator = _data.GetPubKey(offset);
                offset += 32;
                result.StakingAmount = _data.GetU64(offset);
                offset += 8;
                int resultPlayersLength = (int)_data.GetU32(offset);
                offset += 4;
                result.Players = new PublicKey[resultPlayersLength];
                for (uint resultPlayersIdx = 0; resultPlayersIdx < resultPlayersLength; resultPlayersIdx++)
                {
                    result.Players[resultPlayersIdx] = _data.GetPubKey(offset);
                    offset += 32;
                }

                result.State = (GameState)_data.GetU8(offset);
                offset += 1;
                result.CreationTime = _data.GetS64(offset);
                offset += 8;
                result.Winner = _data.GetPubKey(offset);
                offset += 32;
                result.RoomId = _data.GetU64(offset);
                offset += 8;
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum GameErrorKind : uint
        {
            PlayerAlreadyJoined = 6000U,
            RoomIsFull = 6001U,
            GameNotStarted = 6002U,
            InvalidWinner = 6003U,
            ArithmeticOverflow = 6004U,
            TooEarlyToEndGame = 6005U,
            RoomNotInitialized = 6006U,
            RoomClosed = 6007U
        }
    }

    namespace Types
    {
        public enum GameState : byte
        {
            Init,
            Started,
            Finished
        }
    }

    public partial class GameClient : TransactionalBaseClient<GameErrorKind>
    {
        public GameClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId) : base(rpcClient, streamingRpcClient, programId)
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<GlobalState>>> GetGlobalStatesAsync(string programAddress, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = GlobalState.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<GlobalState>>(res);
            List<GlobalState> resultingAccounts = new List<GlobalState>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => GlobalState.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<GlobalState>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Room>>> GetRoomsAsync(string programAddress, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Room.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Room>>(res);
            List<Room> resultingAccounts = new List<Room>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Room.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Room>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<GlobalState>> GetGlobalStateAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<GlobalState>(res);
            var resultingAccount = GlobalState.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<GlobalState>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Room>> GetRoomAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Room>(res);
            var resultingAccount = Room.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Room>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeGlobalStateAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, GlobalState> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                GlobalState parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = GlobalState.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeRoomAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Room> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Room parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Room.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        protected override Dictionary<uint, ProgramError<GameErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<GameErrorKind>>{{6000U, new ProgramError<GameErrorKind>(GameErrorKind.PlayerAlreadyJoined, "Player has already joined the room")}, {6001U, new ProgramError<GameErrorKind>(GameErrorKind.RoomIsFull, "Room is full")}, {6002U, new ProgramError<GameErrorKind>(GameErrorKind.GameNotStarted, "Game has not started yet")}, {6003U, new ProgramError<GameErrorKind>(GameErrorKind.InvalidWinner, "Winner is not a player in the room")}, {6004U, new ProgramError<GameErrorKind>(GameErrorKind.ArithmeticOverflow, "Arithmetic overflow occurred")}, {6005U, new ProgramError<GameErrorKind>(GameErrorKind.TooEarlyToEndGame, "Too Early To End Game")}, {6006U, new ProgramError<GameErrorKind>(GameErrorKind.RoomNotInitialized, "Room is not in initialized state")}, {6007U, new ProgramError<GameErrorKind>(GameErrorKind.RoomClosed, "Room is closed for joining")}, };
        }
    }

    namespace Program
    {
        public class InitializeAccounts
        {
            public PublicKey GlobalState { get; set; }

            public PublicKey User { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class CreateRoomAccounts
        {
            public PublicKey Room { get; set; }

            public PublicKey GlobalState { get; set; }

            public PublicKey Creator { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class JoinRoomAccounts
        {
            public PublicKey Room { get; set; }

            public PublicKey Player { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class EndGameAccounts
        {
            public PublicKey Room { get; set; }

            public PublicKey Winner { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public static class GameProgram
        {
            public const string ID = "11111111111111111111111111111111";
            public static Solana.Unity.Rpc.Models.TransactionInstruction Initialize(InitializeAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.GlobalState, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.User, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(17121445590508351407UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction CreateRoom(CreateRoomAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Room, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.GlobalState, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Creator, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(3869288032152626818UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction JoinRoom(JoinRoomAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Room, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Player, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(10038104089914304607UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction EndGame(EndGameAccounts accounts, PublicKey winner, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Room, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Winner, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(18192764873714534368UL, offset);
                offset += 8;
                _data.WritePubKey(winner, offset);
                offset += 32;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}