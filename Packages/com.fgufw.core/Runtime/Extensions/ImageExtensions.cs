using UnityEngine;
using System;
using UnityEngine.UI;

namespace FGUFW
{
    public static class ImageExtensions
    {
        public static void SetSizeFlexibleWidth(this Image self)
        {
            var sprite = self.sprite;
            if(sprite.IsNull())return;

            
            var width = sprite.texture.width;
            var height = sprite.texture.height;

            var sizeDelta = self.rectTransform.sizeDelta;
            sizeDelta.y = sizeDelta.x * height / width;

            self.rectTransform.sizeDelta = sizeDelta;
        }

        public static void SetSizeFlexibleHeight(this Image self)
        {
            var sprite = self.sprite;
            if(sprite.IsNull())return;

            var width = sprite.texture.width;
            var height = sprite.texture.height;

            var sizeDelta = self.rectTransform.sizeDelta;
            sizeDelta.x = sizeDelta.y * width / height;

            self.rectTransform.sizeDelta = sizeDelta;
        }

        public static void SetGameobjectNameBySpriteName(this Image self)
        {
            var sprite = self.sprite;
            if(sprite.IsNull())return;

            self.name = sprite.name;
        }



#if UNITY_EDITOR

        [UnityEditor.MenuItem("CONTEXT/Image/SetSizeFlexibleWidth")]
        static void setSizeFlexibleWidth(UnityEditor.MenuCommand command)
        {
            var comp = (Image)command.context;
            comp.SetSizeFlexibleWidth();
            UnityEditor.EditorUtility.SetDirty(comp);
        }

        [UnityEditor.MenuItem("CONTEXT/Image/SetSizeFlexibleHeight")]
        static void setSizeFlexibleHeight(UnityEditor.MenuCommand command)
        {
            var comp = (Image)command.context;
            comp.SetSizeFlexibleHeight();
            UnityEditor.EditorUtility.SetDirty(comp);
        }

        [UnityEditor.MenuItem("CONTEXT/Image/NameBySprite")]
        static void setGameobjectNameBySpriteName(UnityEditor.MenuCommand command)
        {
            var comp = (Image)command.context;
            comp.SetGameobjectNameBySpriteName();
            UnityEditor.EditorUtility.SetDirty(comp);
        }

#endif

    }
}
