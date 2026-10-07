using UnityEngine;

namespace FGUFW
{
    public class AudioServiceSwitchBGM : MonoBehaviour
    {
        public string BGM_AudioId;

        void OnEnable()
        {
            AudioService.I.SwitchBGM(BGM_AudioId);
        }
    }
}
