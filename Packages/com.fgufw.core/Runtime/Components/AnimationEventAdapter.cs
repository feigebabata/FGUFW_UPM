using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FGUFW
{
    public class AnimationEventAdapter : MonoBehaviour
    {
        [Serializable]
        public class AnimationEvent : UnityEvent<string> {}

        public AnimationEvent KeyEvent;

        public void OnKeyEvent(string key)
        {
            KeyEvent?.Invoke(key);
        }

    }
}
