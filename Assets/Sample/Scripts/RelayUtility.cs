using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using System;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;

namespace UTJ.NetcodeGameObjectSample
{
    // Relay関連のコード
    // 下記のRelayのコードを参考にしています
    // https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop
    public class RelayServiceUtility
    {
        // Host側のコード
        #region HOST_CODE
        // RelayでJoinするためのコード
        public static string HostJoinCode { get; private set; }
        // Relayサーバーでの最大接続数
        private static readonly int k_MaxUnityRelayConnections = 10;

        // UnityのRelayに参加します
        public static async void StartUnityRelayHost(Action onSuccess,Action onFailed)
        {
            try
            {
                // Unityサービスの初期化及びSignInを行います
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    var playerId = AuthenticationService.Instance.PlayerId;
                    Debug.Log("認証後のPlayerID："+playerId);
                }

                var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
                // Relayサーバーの確保をしてJoinコードを取得します
                var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(k_MaxUnityRelayConnections);
                await serverRelayUtilityTask;
                // Relayサーバーの諸々を取得してきて、Transportに設定をします
                var (ipv4Address, port, allocationIdBytes, connectionData, key, joinCode) = serverRelayUtilityTask.Result;
                utp.SetRelayServerData(ipv4Address, port, allocationIdBytes, key, connectionData);
                HostJoinCode = joinCode;
                NetworkManager.Singleton.StartHost();

                if (onSuccess!=null)
                {
                    onSuccess();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                if (onFailed != null)
                {
                    onFailed();
                }
            }

        }

        // RelayServerの確保を行います
        private static async Task<(string ipv4address, ushort port,
            byte[] allocationIdBytes, byte[] connectionData, byte[] key, string joinCode)> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
        {
            Allocation allocation;
            string joinCode;
            try
            {
                allocation = await Relay.Instance.CreateAllocationAsync(maxConnections, region);
            }
            catch (Exception exception)
            {
                throw new Exception($"Creating allocation request has failed: \n {exception.Message}");
            }

            //Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
            //Debug.Log($"server: {allocation.AllocationId}");

            try
            {
                joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            }
            catch (Exception exception)
            {
                throw new Exception($"Creating join code request has failed: \n {exception.Message}");
            }
            return (allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes,
                allocation.ConnectionData, allocation.Key, joinCode);
        }


        #endregion HOST_CODE



        #region CLIENT_CODE
        public static async void StartClientUnityRelayModeAsync(string joinCode)
        {
            try
            {
                // UnityServiceを初期化してSignInします
                await UnityServices.InitializeAsync();
                //Debug.Log(AuthenticationService.Instance);
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    var playerId = AuthenticationService.Instance.PlayerId;
                    //Debug.Log(playerId);
                }
                // Joinコードから接続に関する情報を取得してセットします
                var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
                var clientRelayUtilityTask = JoinRelayServerFromJoinCode(joinCode);
                await clientRelayUtilityTask;
                var (ipv4Address, port, allocationIdBytes, connectionData, hostConnectionData, key) = clientRelayUtilityTask.Result;
                utp.SetRelayServerData(ipv4Address, port, allocationIdBytes, key, connectionData, hostConnectionData);
                NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

        }

        // JoinCodeから接続情報諸々を取得などします
        private static async
            Task<(string ipv4address, ushort port, byte[] allocationIdBytes, byte[] connectionData, byte[]
                hostConnectionData, byte[] key)> JoinRelayServerFromJoinCode(string joinCode)
        {
            JoinAllocation allocation;
            try
            {
                allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
            }
            catch (Exception exception)
            {
                throw new Exception($"Creating join code request has failed: \n {exception.Message}");
            }
            // Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
            // Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
            // Debug.Log($"client: {allocation.AllocationId}");
            return (allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes,
                allocation.ConnectionData, allocation.HostConnectionData, allocation.Key);
        }

        #endregion CLIENT_CODE

    }
}
