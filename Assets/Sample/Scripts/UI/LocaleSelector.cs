using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UTJ.NetcodeGameObjectSample
{
    public class LocaleSelector : MonoBehaviour
    {
        [SerializeField] private Dropdown dropDown;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if( LocalizationSettings.InitializationOperation.IsDone)
            {
                SetUpUI();
            }
            else
            {
                LocalizationSettings.InitializationOperation.Completed += OnComplete;
            }
        }
        void OnComplete(AsyncOperationHandle<LocalizationSettings> asyncOperation)
        {
            SetUpUI();
        }

        // Update is called once per frame
        void SetUpUI()
        {

            dropDown.options.Clear();
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                dropDown.options.Add(new Dropdown.OptionData(locale.name));
            }

            dropDown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);


            dropDown.onValueChanged.AddListener((index) =>
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
            });
        }
    }
}
