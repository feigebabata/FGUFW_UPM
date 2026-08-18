using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Reflection;

namespace FGUFW
{
    public static class PhysicsExtensions
    {
        public static int CompareTo(this RaycastHit self, RaycastHit target)
        {
            return self.distance.CompareTo(target.distance);
        }
        public static int CompareTo(this RaycastHit self, object target)
        {
            return self.distance.CompareTo(((RaycastHit)target).distance);
        }

    }
}
