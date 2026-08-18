using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FGUFW
{
    public static class CameraExtensions
    {
        public static Collider2D[] OverlapPointAll(this Camera self,UnityEngine.EventSystems.PointerEventData eventData)
        {
            Vector3 pos = eventData.position;
            pos.z = eventData.pointerEnter.transform.position.z;
            
            var worldPosition = self.ScreenToWorldPoint(pos);

            return Physics2D.OverlapPointAll(worldPosition);
        }

        public static T OverlapPoint<T>(this Camera self,UnityEngine.EventSystems.PointerEventData eventData)
        {
            var colliders = OverlapPointAll(self,eventData);
            foreach (var item in colliders)
            {
                if(item.gameObject == eventData.pointerEnter)continue;

                var tObj = item.GetComponent<T>();
                if(tObj!=null)
                {
                    return tObj;
                }
            }
            return default;
        }

    }
}
