using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;


namespace UTJ.NetcodeGameObjectSample
{
    public class ServerConnectInfo : MonoBehaviour
    {

        private ConnectInfo cachedConnectInfo;
        [SerializeField]
        public Text serverInfoText;
        [SerializeField]
        public GameObject serverInfoRoot;


        public void EnableInfoUI(ConnectInfo info,string relayCode)
        {
            cachedConnectInfo = new ConnectInfo(info);
            cachedConnectInfo.relayCode = relayCode;
            serverInfoRoot.SetActive(true);
            UpdateText();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChange;
        }

        public void DisableInfoUI()
        {
            serverInfoRoot.SetActive(false);
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChange;
            cachedConnectInfo = null;
        }

        private void OnLocaleChange(Locale locale)
        {
            UpdateText();
        }

        // Information用のテキストをセットします
        private void UpdateText()
        {
            if(cachedConnectInfo == null)
            {
                return;
            }
            var connectInfo = cachedConnectInfo;
            string cachedJoinCode = connectInfo.relayCode;

            if (!connectInfo.useRelay)
            {

                var localizedString = new LocalizedString("StringTable", "ServerInfo");

                localizedString.Arguments = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["ConnectIp"] = NetworkUtility.GetLocalIP(),
                        ["Port"] = connectInfo.port
                    }
                };
                this.serverInfoText.text = localizedString.GetLocalizedString();
            }
            else if (!string.IsNullOrEmpty(cachedJoinCode))
            {
                var localizedString = new LocalizedString("StringTable", "RelayInfo");

                localizedString.Arguments = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["RelayCode"] = cachedJoinCode
                    }
                };
            }
        }


    }
}