using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FGUFW
{
    public class Collider2DTrigger : MonoBehaviour
    {
        public Collider2DTriggerEvent TriggerEnter=new();
        public Collider2DTriggerEvent TriggerExit=new();
        public Collider2DTriggerEvent TriggerStay=new();
        public Collider2DCollisionEvent CollisionEnter=new();
        public Collider2DCollisionEvent CollisionExit=new();
        public Collider2DCollisionEvent CollisionStay=new();

        void OnTriggerEnter2D(Collider2D collision)
        {
            TriggerEnter.Invoke(collision);
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            TriggerExit.Invoke(collision);
        }

        void OnTriggerStay2D(Collider2D collision)
        {
            TriggerStay.Invoke(collision);
        }


        void OnCollisionEnter2D(Collision2D collision)
        {
            CollisionEnter.Invoke(collision);
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            CollisionExit.Invoke(collision);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            CollisionStay.Invoke(collision);
        }

        [Serializable]
        public class Collider2DTriggerEvent : UnityEvent<Collider2D> {}

        [Serializable]
        public class Collider2DCollisionEvent : UnityEvent<Collision2D> {}
    }
}
