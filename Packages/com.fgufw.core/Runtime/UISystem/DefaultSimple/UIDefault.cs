using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    [RequireComponent(typeof(Animator))]
    public class UIDefault : UIBase
    {
        [Header("UI动画控制器 必须包含动画OnOpen,OnClose")]
        public Animator UIAnim;

        public override void OnOpen()
        {
            gameObject.SetActive(true);
            UIAnim.Play("OnOpen",default,default);
        }

        public override void OnClose()
        {
            UIAnim.Play("OnClose",default,default);
        }

        /// <summary>
        /// 在OnClose动画结束Key事件帧调用
        /// </summary>
        void OnCloseAnimEnd()
        {
            gameObject.SetActive(false);
        }
    }

}
