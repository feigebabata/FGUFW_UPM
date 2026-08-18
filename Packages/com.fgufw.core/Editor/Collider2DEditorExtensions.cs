using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FGUFW.Editor
{
    public static class Collider2DEditorExtensions
    {

        [MenuItem("CONTEXT/BoxCollider2D/FitToRect")]
        static void BoxCollider2DFitToRect(MenuCommand command)
        {
            BoxCollider2D controller = (BoxCollider2D)command.context;
            controller.FitToRect(controller.transform as RectTransform);
        }

        [MenuItem("CONTEXT/CapsuleCollider2D/FitToRect")]
        static void CapsuleCollider2DFitToRect(MenuCommand command)
        {
            CapsuleCollider2D controller = (CapsuleCollider2D)command.context;
            controller.FitToRect(controller.transform as RectTransform);
        }

    }
}