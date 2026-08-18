using System;
using UnityEngine;
using UnityEngine.UI;

namespace FGUFW.I18N
{
    public class I18N_UID : MonoBehaviour
    {
        public string UID;

        void OnValidate()
        {
            ResetTextValue();    
        }

        void Awake()
        {
            ResetTextValue();
        }

        public void ResetTextValue()
        {
            Text textComp = GetComponent<Text>();
            if(!textComp.IsNull())
            {
                textComp.text = I18N_Utility.GetText(UID);

                return;
            }

            // TMP_Text tmp_textComp = GetComponent<TMP_Text>();
            // if(!tmp_textComp.IsNull())
            // {
            //     tmp_textComp.text = I18N_Utility.GetText(UID);

            //     return;
            // }

        }
    }
}
