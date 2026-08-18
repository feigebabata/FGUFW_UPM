using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FGUFW
{
    [ExecuteAlways]
    public class SimpleSpriteAnimation : MonoBehaviour
    {
#if UNITY_EDITOR
        public bool RuningEditor;
#endif
        
        public Image Img;
        public SpriteRenderer Render;

        public bool Loop;
        public float Duration;
        public Sprite[] Sprites;


        private int _index;
        private float _startTime;

        void OnEnable()
        {
            _index = 0;
            _startTime = Time.time;

            if(Sprites.Length==0)
            {
                enabled = false;
                return;
            }
        }

        void Update()
        {
            
#if UNITY_EDITOR
            if(!RuningEditor)return;
#endif

            var t = Time.time-_startTime;

            if(!Loop && t>Duration)
            {
                setSprite(Sprites.LastIndex());
                enabled = false;
                return;
            }

            _index = (int)(t/Duration)%Sprites.Length;
            setSprite(_index);
        }

        void setSprite(int idx)
        {
            if(!Img.IsNull())
            {
                Img.sprite = Sprites[idx];
            }
            if(!Render.IsNull())
            {
                Render.sprite = Sprites[idx];
            }
        }

    }
    
}
