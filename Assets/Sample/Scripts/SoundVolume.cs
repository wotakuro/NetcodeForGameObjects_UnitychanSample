using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.MLAPISample
{
    public class SoundVolume : MonoBehaviour
    {
        // SoundVolumeの保存場所です
        public static float VoiceValue = 1.0f;

        public void OnVoiceValueChanged(float val)
        {
            VoiceValue = val;
        }
        
    }
}