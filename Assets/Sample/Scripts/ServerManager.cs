﻿//using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking.Match;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Localization;

namespace UTJ.NetcodeGameObjectSample
{
    // ホスト接続した際に、MLAPIからのコールバックを管理して切断時等の処理をします
    public class ServerManager : MonoBehaviour
    {
        public Button stopButton;
        public GameObject configureObject;

        [SerializeField]
        public ServerConnectInfo serverConnectInfoUI;

        private ConnectInfo cachedConnectInfo;




        public void Setup(ConnectInfo connectInfo)
        {
            this.cachedConnectInfo = connectInfo;
            // サーバーとして起動したときのコールバック設定
            Unity.Netcode.NetworkManager.Singleton.OnServerStarted += this.OnStartServer;
            // クライアントが接続された時のコールバック設定
            Unity.Netcode.NetworkManager.Singleton.OnClientConnectedCallback += this.OnClientConnect;
            // クライアントが切断された時のコールバック設定
            Unity.Netcode.NetworkManager.Singleton.OnClientDisconnectCallback += this.OnClientDisconnect;
            // transportの初期化
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.Initialize();
        }

        // Information用のテキストをセットします
        public void SetHostNetworkInformation()
        {
            serverConnectInfoUI.EnableInfoUI(this.cachedConnectInfo, null);
        }


        // Information用のテキストをセットします
        public void SetInformationTextWithRelay(string joinCode)
        {
            this.serverConnectInfoUI.EnableInfoUI(this.cachedConnectInfo, joinCode);
        }


        private void RemoveCallBack()
        {
            // サーバーとして起動したときのコールバック設定
            Unity.Netcode.NetworkManager.Singleton.OnServerStarted -= this.OnStartServer;
            // クライアントが接続された時のコールバック設定
            Unity.Netcode.NetworkManager.Singleton.OnClientConnectedCallback -= this.OnClientConnect;
            // クライアントが切断された時のコールバック設定
            Unity.Netcode.NetworkManager.Singleton.OnClientDisconnectCallback -= this.OnClientDisconnect;
            if (this.cachedConnectInfo.useRelay)
            {
//                Unity.Netcode.Transports.UNET.RelayTransport.OnRemoteEndpointReported -= OnRelayEndPointReported;
            }
        }

        // クライアントが接続してきたときの処理
        private void OnClientConnect(ulong clientId)
        {
            Debug.Log("Connect Client " + clientId);
            SpawnNetworkPrefab(this.networkedPrefab, clientId);
        }

        // クライアントが切断した時の処理
        private void OnClientDisconnect(ulong clientId)
        {
            Debug.Log("Disconnect Client " + clientId);

        }

        // サーバー開始時の処理
        private void OnStartServer()
        {
            Debug.Log("Start Server");
            // hostならば生成します

            configureObject.SetActive(false);
            stopButton.GetComponentInChildren<Text>().text = "Stop Host";
            stopButton.onClick.AddListener(OnClickDisconnectButton);
            stopButton.gameObject.SetActive(true);
        }

        // 切断ボタンが呼び出された時の処理
        private void OnClickDisconnectButton()
        {
            Unity.Netcode.NetworkManager.Singleton.Shutdown();
            this.RemoveCallBack();

            this.configureObject.SetActive(true);
            this.stopButton.gameObject.SetActive(false);
            this.serverConnectInfoUI.DisableInfoUI();
        }
        [SerializeField]
        private GameObject networkedPrefab;
        // ネットワーク同期するNetworkPrefabを生成します
        private void SpawnNetworkPrefab(GameObject prefab,ulong clientId)
        {
            Debug.Log("SpawnNetworkPrefab");
            var netMgr = Unity.Netcode.NetworkManager.Singleton;
            var randomPosition = new Vector3(Random.Range(-7, 7), 5.0f, Random.Range(-7, 7));
            var gmo = GameObject.Instantiate(prefab, randomPosition, Quaternion.identity);
            var netObject = gmo.GetComponent<NetworkObject>();
            // このNetworkオブジェクトをクライアントでもSpawnさせます
            netObject.SpawnWithOwnership(clientId);
        }


    }
}