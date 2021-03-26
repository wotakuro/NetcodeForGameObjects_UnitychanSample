using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace UTJ.MLAPISample
{
    // ホスト接続した際に、MLAPIからのコールバックを管理して切断時等の処理をします
    public class ServerManager : MonoBehaviour
    {
        public Button stopButton;
        public GameObject configureObject;

        public GameObject serverInfoRoot;
        public Text serverInfoText;

        private MLAPI.Transports.Tasks.SocketTasks socketTasks;
        private ConnectInfo cachedConnectInfo;


        public void SetSocketTasks(MLAPI.Transports.Tasks.SocketTasks tasks)
        {
            socketTasks = tasks;
        }

        public void Setup(ConnectInfo connectInfo, string localIp)
        {
            this.cachedConnectInfo = connectInfo;
            // サーバーとして起動したときのコールバック設定
            MLAPI.NetworkManager.Singleton.OnServerStarted += this.OnStartServer;
            // クライアントが接続された時のコールバック設定
            MLAPI.NetworkManager.Singleton.OnClientConnectedCallback += this.OnClientConnect;
            // クライアントが切断された時のコールバック設定
            MLAPI.NetworkManager.Singleton.OnClientDisconnectCallback += this.OnClientDisconnect;

            if (connectInfo.useRelay)
            {
                MLAPI.Transports.UNET.RelayTransport.OnRemoteEndpointReported += OnRelayEndPointReported;
                this.serverInfoRoot.SetActive(true);
                this.serverInfoText.text = "Relayサーバーに繋がっていません";
            }
            else
            {
                var stringBuilder = new System.Text.StringBuilder(256);
                this.serverInfoRoot.SetActive(true);
                stringBuilder.Append("サーバー接続情報\n").
                    Append("接続先IP:").Append(localIp).Append("\n").
                    Append("Port番号:").Append(connectInfo.port);
                this.serverInfoText.text = stringBuilder.ToString();
            }
            // transportの初期化
            MLAPI.NetworkManager.Singleton.NetworkConfig.NetworkTransport.Init();
        }

        private void OnRelayEndPointReported(System.Net.IPEndPoint endPoint)
        {
            var stringBuilder = new System.Text.StringBuilder(256);
            this.serverInfoRoot.SetActive(true);
            stringBuilder.Append("サーバー接続情報\n").
                Append("接続先IP:").Append(endPoint.Address.ToString()).Append("\n").
                Append("Port番号:").Append(endPoint.Port).Append("\n").
                Append("Relay IP:").Append(cachedConnectInfo.relayIpAddr).Append("\n").
                Append("Relay Port:").Append(cachedConnectInfo.relayPort);
            this.serverInfoText.text = stringBuilder.ToString();
        }

        private void RemoveCallBack()
        {
            // サーバーとして起動したときのコールバック設定
            MLAPI.NetworkManager.Singleton.OnServerStarted -= this.OnStartServer;
            // クライアントが接続された時のコールバック設定
            MLAPI.NetworkManager.Singleton.OnClientConnectedCallback -= this.OnClientConnect;
            // クライアントが切断された時のコールバック設定
            MLAPI.NetworkManager.Singleton.OnClientDisconnectCallback -= this.OnClientDisconnect;
            if (this.cachedConnectInfo.useRelay)
            {
                MLAPI.Transports.UNET.RelayTransport.OnRemoteEndpointReported -= OnRelayEndPointReported;
            }
        }

        // クライアントが接続してきたときの処理
        private void OnClientConnect(ulong clientId)
        {
            Debug.Log("Connect Client " + clientId);
            SpawnNetworkPrefab(0, clientId);
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
            var clientId = MLAPI.NetworkManager.Singleton.ServerClientId;
            // hostならば生成します
            if (MLAPI.NetworkManager.Singleton.IsHost)
            {
                SpawnNetworkPrefab(0,clientId);
            }

            configureObject.SetActive(false);
            stopButton.GetComponentInChildren<Text>().text = "Stop Host";
            stopButton.onClick.AddListener(OnClickDisconnectButton);
            stopButton.gameObject.SetActive(true);
        }

        // 切断ボタンが呼び出された時の処理
        private void OnClickDisconnectButton()
        {
            MLAPI.NetworkManager.Singleton.StopHost();
            this.RemoveCallBack();

            this.configureObject.SetActive(true);
            this.stopButton.gameObject.SetActive(false);
            this.serverInfoRoot.SetActive(false);
        }

        // ネットワーク同期するNetworkPrefabを生成します
        private void SpawnNetworkPrefab(int idx,ulong clientId)
        {
            var netMgr = MLAPI.NetworkManager.Singleton;
            var networkedPrefab = netMgr.NetworkConfig.NetworkPrefabs[idx];
            var randomPosition = new Vector3(Random.Range(-7, 7), 5.0f, Random.Range(-7, 7));
            var gmo = GameObject.Instantiate(networkedPrefab.Prefab, randomPosition, Quaternion.identity);
            var netObject = gmo.GetComponent<NetworkObject>();
            // このNetworkオブジェクトをクライアントでもSpawnさせます
            netObject.SpawnWithOwnership(clientId);
        }


    }
}