using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    public class SimpleAudioPlayer : MonoSingleton<SimpleAudioPlayer>
    {
        
        List<AudioSource> _audioSources = new List<AudioSource>();
        AudioSource _bgmAudioSource;

        protected override bool IsDontDestroyOnLoad()=>true;

        float _audioVolume=1;
        public float AudioVolume
        {
            set
            {
                _audioVolume = value;
                foreach (var item in _audioSources)
                {
                    item.volume = value;
                }
            }
            get
            {
                return _audioVolume;
            }
        }

        public float BGMVolume
        {
            set
            {
                _bgmAudioSource.volume = value;
            }
            get
            {
                return _bgmAudioSource.volume;
            }
        }

        [SerializeField]
        bool _bgmPause=false;
        public bool BGMPause
        {
            set
            {
                if(value)
                {
                    _bgmAudioSource.Pause();
                }
                else
                {
                    _bgmAudioSource.UnPause();
                }
                _bgmPause = value;
            }
            get
            {
                return _bgmPause;
            }
        }

        [SerializeField]
        bool _audioPause=false;
        public bool AudioPause
        {
            set
            {
                if(value)
                {
                    foreach (var item in _audioSources)
                    {
                        item.Stop();
                    }
                }
                _audioPause = value;
            }
            get
            {
                return _audioPause;
            }
        }

        protected override void Init()
        {
            base.Init();

            _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            _bgmAudioSource.volume = BGMVolume;
            _bgmAudioSource.loop = true;
        }

        public void PlayBGM(AudioClip clip)
        {
            _bgmAudioSource.clip = clip;
            if(BGMPause) return;
            _bgmAudioSource.Play();
        }

        public AudioSource Play(AudioClip clip,bool loop = false,float startTime=0)
        {
            if(AudioPause) return default;

            var source = getNoneAudioSource();
            source.clip = clip;
            source.volume = AudioVolume;
            source.loop = loop;
            source.time = startTime;
            source.Play();

            return source;
        }


        AudioSource getNoneAudioSource()
        {
            foreach (var item in _audioSources)
            {
                if(!item.isPlaying && item.time%item.clip.length==0)
                {
                    return item;
                }
            }
            return addAudioSource();
        }



        AudioSource addAudioSource()
        {
            var comp = gameObject.AddComponent<AudioSource>();
            _audioSources.Add(comp);
            return comp;
        }

    }
}
