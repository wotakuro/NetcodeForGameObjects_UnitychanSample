using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UTJ.MLAPISample
{
    // クライアント接続した際に、MLAPIからのコールバックを管理して切断時等の処理をします
    public class ClientManager : MonoBehaviour
    {
        public Button stopButton;
        public GameObject configureObject;
        private bool previewConnected;


        private MLAPI.Transports.Tasks.SocketTasks socketTasks;
        public void SetSocketTasks(MLAPI.Transports.Tasks.SocketTasks tasks)
        {
            socketTasks = tasks;
        }

        public void Setup()
        {
            MLAPI.NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            MLAPI.NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void ReoveCallbacks()
        {
            MLAPI.NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            MLAPI.NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        private void Disconnect()
        {
#if ENABLE_AUTO_CLIENT
            // クライアント接続時に切断したらアプリ終了させます
            if (NetworkUtility.IsBatchModeRun)
            {
                Application.Quit();
            }
#endif
            // UIを戻します
            configureObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            stopButton.onClick.RemoveAllListeners();
            // コールバックも削除します
            ReoveCallbacks();
        }

        private void OnClickStopButton()
        {
            MLAPI.NetworkManager.Singleton.StopClient();
            Disconnect();
        }

        private void OnClientConnect(ulong clientId)
        {
            // 自身の接続の場合
            if (clientId == MLAPI.NetworkManager.Singleton.LocalClientId)
            {
                configureObject.SetActive(false);

                stopButton.GetComponentInChildren<Text>().text = "Disconnect";
                stopButton.onClick.AddListener(this.OnClickStopButton);
                stopButton.gameObject.SetActive(true);
            }
            Debug.Log("Connect Client:" + clientId + "::" + MLAPI.NetworkManager.Singleton.LocalClientId);
        }
        private void OnClientDisconnect(ulong clientId)
        {
            // 自身が外された
            Debug.Log("Disconnect Client: " + clientId);
        }

        private void Update()
        {
            var netMgr = MLAPI.NetworkManager.Singleton;
            // 3人以上接続時に切断が呼び出されないので対策
            if (!netMgr.IsConnectedClient && previewConnected)
            {
                Disconnect();
            }
            previewConnected = netMgr.IsConnectedClient;
        }
    }
}