using System;
using UnityEngine;

namespace FGUFW
{
    public sealed class ApplicationLifecycle : MonoSingleton<ApplicationLifecycle>
    {
        public const string Msg_OnApplicationPause = "OnApplicationPause";
        public const string Msg_OnApplicationFocus = "OnApplicationFocus";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _ = I;
        }

        protected override bool IsDontDestroyOnLoad()
        {
            return true;
        }

        private void OnApplicationPause(bool pause)
        {
            fg.messenger.Broadcast(Msg_OnApplicationPause,pause);
        }

        private void OnApplicationFocus(bool focus)
        {
            fg.messenger.Broadcast(Msg_OnApplicationFocus,focus);
        }

    }


    
    

}
