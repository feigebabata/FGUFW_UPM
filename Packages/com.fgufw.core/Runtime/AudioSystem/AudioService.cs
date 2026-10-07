using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FGUFW
{
    public class AudioService : MonoSingleton<AudioService>
    {        
        private AudioSource bgmPlayer;
        private List<AudioSource> onceAudioPlayers = new();


        protected override bool IsDontDestroyOnLoad() => true;

        protected override void Init()
        {
            base.Init();

            checkBGMPlayer();
        }

        private void checkBGMPlayer()
        {
            if(bgmPlayer==default)
            {
                bgmPlayer = gameObject.AddComponent<AudioSource>();
                bgmPlayer.loop = true;
            }
        }

        public async Task SwitchBGM(string audioId)
        {
            checkBGMPlayer();
            var clip = await getAudioClip(audioId);
            bgmPlayer.clip = clip;
            bgmPlayer.Play();
        }

        public async Task PlayOnce(string audioId)
        {
            var player = getOnceAudioSource();
            var clip = await getAudioClip(audioId);
            player.clip = clip;
            player.Play();
        }

        Task<AudioClip> getAudioClip(string audioId)
        {
            return fg.assetLoader.LoadAsync<AudioClip>(audioId);
        }

        AudioSource getOnceAudioSource()
        {
            foreach (var item in onceAudioPlayers)
            {
                if(!item.isPlaying)
                {
                    return item;
                }
            }
            
            var player = gameObject.AddComponent<AudioSource>();
            onceAudioPlayers.Add(player);
            player.loop = false;
            return player;
        }


    }
}
