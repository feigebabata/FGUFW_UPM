using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.Playables;

namespace FGUFW
{
    public static class AnimatorExtensions
    {
        public static void ReplaceClip(this Animator self,AnimationClip clip)
        {
            AnimatorOverrideController overrideController = null;
            if(self.runtimeAnimatorController is AnimatorOverrideController)
            {
                overrideController = self.runtimeAnimatorController as AnimatorOverrideController;
            }
            else
            {
                overrideController = new AnimatorOverrideController(self.runtimeAnimatorController);
                self.runtimeAnimatorController = overrideController;
            }
            var clipName = clip.name;
            overrideController[clipName] = clip;
        }

        public static void PlayClip(this Animator self,AnimationClip clip)
        {
            AnimationPlayableUtilities.PlayClip(self,clip,out _);
        }

        public static void PlayClip(this Animator self,string clipName)
        {
            AnimatorOverrideController overrideController = null;
            if(self.runtimeAnimatorController is AnimatorOverrideController)
            {
                overrideController = self.runtimeAnimatorController as AnimatorOverrideController;
            }
            else
            {
                overrideController = new AnimatorOverrideController(self.runtimeAnimatorController);
                self.runtimeAnimatorController = overrideController;
            }
            var clip = overrideController[clipName];
            AnimationPlayableUtilities.PlayClip(self,clip,out _);
        }
        
    }
}