#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FGUFW.Editor
{
    public static class AnimationClipCreateHelper
    {
        [MenuItem("Assets/Delete Animation In AnimatorController",true,19)]
        private static bool ValidateRemoveClipMenu()
        {
            var clip = Selection.activeObject as AnimationClip;
            
            return ClipInController(clip);
        }
        
        [MenuItem("Assets/Delete Animation In AnimatorController",false,19)]
        private static void RemoveClipMenu()
        {
            var clip = Selection.activeObject as AnimationClip;
            
            RemoveClipFromController(clip);
        }

        #region 鼠标右键 Create/Animation In AnimatorController
        [MenuItem("Assets/Create/Animation In AnimatorController", true, 402)]
        public static bool ValidateCreateAnimationClipInAnimatorController()
        {
            // 只对选中的第一个对象进行验证
            var controller = Selection.activeObject as AnimatorController;
            return controller != null; // 如果是 AnimatorController 就显示菜单
        }

        [MenuItem("Assets/Create/Animation In AnimatorController", false, 402)]
        public static void CreateAnimationClipInAnimatorController()
        {
            var controller = Selection.activeObject as AnimatorController;
            if (controller == default) return;

            TextInputWindow.Open("Create AnimationClip Name", "NewAnimationClip", clipName => CreateClip(controller, clipName));
        }
        #endregion

        [MenuItem("CONTEXT/Animator/Create AnimationClip In AnimatorController")]
        static void AnimatorCreateAnimationClipInAnimatorController(MenuCommand command)
        {
            Animator animator = (Animator)command.context;
            TextInputWindow.Open("Create AnimationClip Name", "NewAnimationClip", clipName => CreateClip(animator, clipName));
        }

        public static void CreateClip(Animator animator, string clipName)
        {
            if (string.IsNullOrWhiteSpace(clipName)) return;

            if (animator.runtimeAnimatorController.IsNull())
            {
                string path = EditorUtility.SaveFilePanelInProject("Create AnimatorController", animator.name, "controller", "Select location for AnimatorController");
                if (path.IsNull()) return;
                var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
                animator.runtimeAnimatorController = controller;

                CreateClip(controller as AnimatorController, clipName);
            }
            else
            {
                CreateClip(animator.runtimeAnimatorController as AnimatorController, clipName);
            }
        }

        public static void CreateClip(AnimatorController controller, string clipName)
        {
            string path = AssetDatabase.GetAssetPath(controller);

            if (AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Any(c => c.name == clipName))
            {
                EditorUtility.DisplayDialog("AnimationClip 已存在", $"{controller.name}.controller中已存在[{clipName}]!", "OK");
                return;
            }

            var clip = new AnimationClip { name = clipName };
            AssetDatabase.AddObjectToAsset(clip, controller);

            var state = controller.layers[0].stateMachine.AddState(clipName);

            state.motion = clip;
            state.writeDefaultValues = false;

            AssetDatabase.SaveAssets();
            Selection.activeObject = clip;
        }

        public static void RemoveClipFromController(AnimationClip clip)
        {
            string path = AssetDatabase.GetAssetPath(clip);
            var controller = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimatorController>().FirstOrDefault();

            // 移除 StateMachine 中引用
            foreach (var layer in controller.layers)
            {
                var sm = layer.stateMachine;

                var state = layer.stateMachine.states.Find(s=>s.state.motion==clip);

                layer.stateMachine.RemoveState(state.state);
            }

            // 删除 Clip
            Object.DestroyImmediate(clip, true);
            AssetDatabase.SaveAssets();
        }

        public static bool ClipInController(AnimationClip clip)
        {
            string path = AssetDatabase.GetAssetPath(clip);
            var controller = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimatorController>().FirstOrDefault();

            return controller != default;
        }

    }
}
#endif
