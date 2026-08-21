using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW.UISystem.Samples
{
    [RequireComponent(typeof(Animator))]
    public class UIDefault : UIBase
    {
        [Header("UI动画控制器 必须包含动画OnOpen,OnClose")]
        public Animator UIAnim;

        public override void OnOpen()
        {
            UIAnim.Play("OnOpen",default,default);
        }

        public override void OnClose()
        {
            UIAnim.Play("OnClose",default,default);
        }
    }

}
