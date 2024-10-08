using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UTJ.NetcodeGameObjectSample
{
    public class IpAdressInfo : MonoBehaviour
    {
        private LocalizeStringEvent localizeStringEvent;
        private Text textComponent;

        private void Awake()
        {
            UpdateText();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChange;
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChange;
        }

        private void OnLocaleChange(Locale locale)
        {
            UpdateText();
        }

        void UpdateText()
        {
            if(!textComponent)
            {
                textComponent = this.GetComponent<Text>();
            }

            var localizedString = new LocalizedString("StringTable", "IpAddrInfo");
            localizedString.Arguments = new object[]
            {
                new Dictionary<string, object>
                {
                     ["ipaddr"] = NetworkUtility.GetLocalIP()
                }
            };
            textComponent.text = localizedString.GetLocalizedString();
        }
    }
}