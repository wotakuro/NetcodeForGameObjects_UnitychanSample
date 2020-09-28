using UnityEngine;
using System.IO;
using JetBrains.Annotations;

namespace UTJ.MLAPISample
{
    // 接続先を保存するための設定
    [System.Serializable]
    public class ConnectInfo
    {
        //接続先IPアドレス
        [SerializeField]
        public string ipAddr;
        // ポート番号
        [SerializeField]
        public int port;
        //リレーサーバーの有無
        [SerializeField]
        public bool useRelay;

        // リレーサーバーのIPアドレス
        [SerializeField]
        public string relayIpAddr;
        // リレーサーバーのポート番号
        [SerializeField]
        public int relayPort;

        // プレイヤー名
        // ※接続情報とは関係ないのですが、せめて名前くらいはと思い…
        [SerializeField]
        public string playerName;



        public static ConnectInfo GetDefault()
        {
            ConnectInfo info = new ConnectInfo();
            info.useRelay = false;
            info.ipAddr = "127.0.0.1";
            info.port = 7777;
            info.playerName = "大鳥こはく";

            info.relayIpAddr = "184.72.104.138";
            info.relayPort = 8888;
            return info;
        }

        private static string ConfigFile
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                return "connectInfo.json";
#else
            return Path.Combine(Application.persistentDataPath, "connectInfo.json");
#endif
            }
        }

        public static ConnectInfo LoadFromFile()
        {
            var configFilePath = ConfigFile;
            if (!File.Exists(configFilePath))
            {
                return GetDefault();
            }
            string jsonStr = File.ReadAllText(configFilePath);
            var connectInfo = JsonUtility.FromJson<ConnectInfo>(jsonStr);
            return connectInfo;
        }

        public void SaveToFile()
        {
            string jsonStr = JsonUtility.ToJson(this);
            File.WriteAllText(ConfigFile, jsonStr);
        }
    }
}