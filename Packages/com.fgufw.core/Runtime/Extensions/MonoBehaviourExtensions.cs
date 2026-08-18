using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    public static class MonoBehaviourExtensions
    {
        public static void DelayCall(this MonoBehaviour self,float time,Action callback)
        {
            self.StartCoroutine(delayCall(time,callback));
        }

        static IEnumerator delayCall(float time,Action callback)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }

    }
}