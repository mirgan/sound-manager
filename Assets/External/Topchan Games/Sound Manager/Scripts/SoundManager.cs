using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TopchanGames
{
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField]
        private bool dontDestroyOnLoad;

        private readonly SavableValue<bool> soundEnabled = new SavableValue<bool>("SoundManager.soundEnabled", true);
        private readonly SavableValue<bool> musicEnabled = new SavableValue<bool>("SoundManager.musicEnabled", true);
        private readonly SavableValue<float> soundVolume = new SavableValue<float>("SoundManager.soundVolume", 1f);
        private readonly SavableValue<float> musicVolume = new SavableValue<float>("SoundManager.musicVolume", 1f);
        private readonly Dictionary<int, AudioSourceData> sourceMedia = new Dictionary<int, AudioSourceData>();

        #region Props

        public event Action OnMusicPlayCompleted;

        public bool SoundEnabled
        {
            get => soundEnabled.Value;
            set
            {
                if (soundEnabled.Value == value)
                {
                    return;
                }

                foreach (var sourceData in sourceMedia.Keys.Select(key => sourceMedia[key]).Where(sourceData => !sourceData.IsMusic))
                {
                    sourceData.Source.volume = value ? sourceData.RequestedVolume * soundVolume.Value : 0;
                }

                soundEnabled.Value = value;
            }
        }

        public bool MusicEnabled
        {
            get => musicEnabled.Value;
            set
            {
                if (musicEnabled.Value == value)
                {
                    return;
                }

                foreach (var sourceData in sourceMedia.Keys.Select(key => sourceMedia[key]).Where(sourceData => sourceData.IsMusic))
                {
                    sourceData.Source.volume = value ? sourceData.RequestedVolume * musicVolume.Value : 0;
                }

                musicEnabled.Value = value;
            }
        }

        public float SoundVolume
        {
            get => soundVolume.Value;
            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                soundVolume.Value = value;

                if (!soundEnabled.Value)
                {
                    return;
                }

                foreach (var sourceData in sourceMedia.Keys.Select(key => sourceMedia[key]).Where(sourceData => !sourceData.IsMusic))
                {
                    sourceData.Source.volume = sourceData.RequestedVolume * soundVolume.Value;
                }
            }
        }

        public float MusicVolume
        {
            get => musicVolume.Value;
            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                musicVolume.Value = value;

                if (!musicEnabled.Value)
                {
                    return;
                }

                foreach (var sourceData in sourceMedia.Keys.Select(key => sourceMedia[key]).Where(sourceData => sourceData.IsMusic))
                {
                    sourceData.Source.volume = sourceData.RequestedVolume * musicVolume.Value;
                }
            }
        }

        #endregion Props

        private AudioPresenter presenter;
        private int audioCodeIndex;

        private void Awake()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(this);
            }

            Initialization();
        }

        private void Initialization()
        {
            presenter = gameObject.AddComponent<AudioPresenter>();
            var listener = new GameObject("LISTENER").AddComponent<AudioListener>();
            listener.transform.SetParent(presenter.transform);
            presenter.AudioListener = listener;
        }

        #region Music

        public int PlayMusicClipWithMix(AudioClip clip, float mixDuration = 0, float volumeProportion = 1)
        {
            if (mixDuration < 0)
            {
                mixDuration = 0;
            }

            if (volumeProportion > 1)
            {
                volumeProportion = 1;
            }
            else if (volumeProportion < 0)
            {
                volumeProportion = 0;
            }

            StopAllCoroutines();
            StopPlayPrevMusic(mixDuration);
            ScanForEndedSources();

            audioCodeIndex++;

            var source = presenter.gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.priority = 0;
            source.spatialBlend = 0;
            source.minDistance = 0.06f;

            var data = new AudioSourceData
            {
                IsMusic = true,
                OnPause = false,
                RequestedVolume = volumeProportion,
                Source = source,
                AudioCode = audioCodeIndex
            };

            float volume = musicEnabled.Value ? volumeProportion * musicVolume.Value : 0;
            sourceMedia.Add(data.AudioCode, data);
            source.volume = mixDuration == 0 ? volume : 0;
            ;
            source.Play();

            if (mixDuration > 0)
            {
                StartCoroutine(DoFade(source, mixDuration, volume));
            }

            return audioCodeIndex;
        }

        public int PlayMusicClip(AudioClip clip, float fadeDuration = 0, float volumeProportion = 1)
        {
            if (fadeDuration < 0)
            {
                fadeDuration = 0;
            }

            if (volumeProportion > 1)
            {
                volumeProportion = 1;
            }
            else if (volumeProportion < 0)
            {
                volumeProportion = 0;
            }

            StopAllCoroutines();
            bool musicPlaying = StopPlayPrevMusic(fadeDuration);
            ScanForEndedSources();

            audioCodeIndex++;

            var source = presenter.gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = false;
            source.priority = 0;
            source.spatialBlend = 0;
            source.minDistance = 0.06f;

            var data = new AudioSourceData
            {
                IsMusic = true,
                OnPause = false,
                RequestedVolume = volumeProportion,
                Source = source,
                AudioCode = audioCodeIndex
            };

            float volume = musicEnabled.Value ? volumeProportion * musicVolume.Value : 0;
            sourceMedia.Add(data.AudioCode, data);

            if (fadeDuration == 0)
            {
                source.volume = volume;
                source.Play();
                StartCoroutine(WaitForPlayFinished(source));
            }
            else
            {
                source.volume = 0;
                float delay = musicPlaying ? fadeDuration : 0;
                StartCoroutine(DoFadeAfterDelay(source, fadeDuration, volume, delay));
            }

            return audioCodeIndex;
        }

        private IEnumerator WaitForPlayFinished(AudioSource source)
        {
            const float checkInterval = 1f;

            while (source != null && source.isPlaying)
            {
                yield return new WaitForSeconds(checkInterval);
            }

            OnMusicPlayCompleted?.Invoke();
        }

        private bool StopPlayPrevMusic(float fadeDuration)
        {
            var prevMusics = GetPrevMusic();

            void StopMusic(int index)
            {
                var source = prevMusics[index];
                sourceMedia.Remove(index);

                SmartDestroy(source.Source);
            }

            foreach (int k in prevMusics.Keys)
            {
                if (fadeDuration == 0)
                {
                    StopMusic(k);
                }
                else
                {
                    int index = k;
                    StartCoroutine(DoFade(prevMusics[k].Source, fadeDuration, 0, () => StopMusic(index)));
                }
            }

            return prevMusics.Any();
        }

        public void StopPlayingMusicClip(AudioClip clip)
        {
            var sources = sourceMedia.Where(m => m.Value.Source.clip == clip).ToArray();
            if (!sources.Any())
            {
                return;
            }

            foreach (var source in sources)
            {
                var s = source.Value;
                sourceMedia.Remove(source.Key);
                s.Source.Stop();
                SmartDestroy(s.Source);
            }
        }

        public void StopPlayingMusicClip(int audioCode)
        {
            if (!sourceMedia.ContainsKey(audioCode))
            {
                return;
            }

            var s = sourceMedia[audioCode];
            sourceMedia.Remove(audioCode);
            s.Source.Stop();
            SmartDestroy(s.Source);
        }

        public void PausePlayingClip(int audioCode)
        {
            if (!sourceMedia.ContainsKey(audioCode))
            {
                return;
            }

            var s = sourceMedia[audioCode];
            s.Source.Pause();
            s.OnPause = true;
        }

        public void ResumeClipIfInPause(int audioCode)
        {
            if (!sourceMedia.ContainsKey(audioCode))
            {
                return;
            }

            var s = sourceMedia[audioCode];
            s.Source.UnPause();
            s.OnPause = false;
        }

        public bool IsMusicClipCodePlaying(int audioCode)
        {
            if (!sourceMedia.ContainsKey(audioCode))
            {
                return false;
            }

            var s = sourceMedia[audioCode];
            return s.Source.isPlaying;
        }

        public bool IsMusicClipPlaying(AudioClip audioClip)
        {
            bool audioClipIsPlaying =
                sourceMedia.Keys.Any(k => sourceMedia[k].IsMusic && sourceMedia[k].Source.clip == audioClip && sourceMedia[k].Source.isPlaying);
            return audioClipIsPlaying;
        }

        #endregion Music

        #region Sound

        public int PlayAudioClip(AudioClip clip, bool looped = false, float volumeProportion = 1)
        {
            if (volumeProportion > 1)
            {
                volumeProportion = 1;
            }
            else if (volumeProportion < 0)
            {
                volumeProportion = 0;
            }

            ScanForEndedSources();
            audioCodeIndex++;

            var source = presenter.gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = looped;
            source.spatialBlend = 0;
            source.minDistance = 0.06f;
            var data = new AudioSourceData
            {
                IsMusic = false,
                OnPause = false,
                RequestedVolume = volumeProportion,
                Source = source,
                AudioCode = audioCodeIndex
            };

            source.volume = soundEnabled.Value ? volumeProportion * soundVolume.Value : 0;
            source.Play();
            sourceMedia.Add(data.AudioCode, data);
            return audioCodeIndex;
        }

        public void StopPlayingClip(int audioCode)
        {
            if (!sourceMedia.ContainsKey(audioCode))
            {
                return;
            }

            var s = sourceMedia[audioCode];
            sourceMedia.Remove(audioCode);
            s.Source.Stop();

            SmartDestroy(s.Source);
        }

        public void StopPlayingClip(AudioClip clip)
        {
            var sources = sourceMedia.Where(m => m.Value.Source.clip == clip).ToArray();
            if (!sources.Any())
            {
                return;
            }

            foreach (var source in sources)
            {
                var s = source.Value;
                sourceMedia.Remove(source.Key);
                s.Source.Stop();
                SmartDestroy(s.Source);
            }
        }

        public bool IsAudioClipCodePlaying(int audioCode)
        {
            if (!sourceMedia.ContainsKey(audioCode))
            {
                return false;
            }

            var s = sourceMedia[audioCode];
            return s.Source.isPlaying;
        }

        #endregion Sound

        #region Utils

        private Dictionary<int, AudioSourceData> GetPrevMusic()
        {
            var prevMusics = sourceMedia.Keys.Where(k => sourceMedia[k].IsMusic).ToDictionary(k => k, k => sourceMedia[k]);
            return prevMusics;
        }

        private int GetLastMusic()
        {
            var prevMusics = sourceMedia.Keys.Where(k => sourceMedia[k].IsMusic).ToDictionary(k => k, k => sourceMedia[k]);
            int lastIndex = prevMusics.OrderBy(m => m.Key).Select(m => m.Key).First();
            return lastIndex;
        }

        private IEnumerator DoFadeAfterDelay(AudioSource audioSource, float duration, float targetVolume, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (audioSource == null)
            {
                yield break;
            }

            audioSource.Play();
            StartCoroutine(WaitForPlayFinished(audioSource));
            StartCoroutine(DoFade(audioSource, duration, targetVolume));
        }

        private IEnumerator DoFade(AudioSource audioSource, float duration, float targetVolume, Action callback = null)
        {
            float currentTime = 0;
            float start = audioSource.volume;

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                if (audioSource == null)
                {
                    callback?.Invoke();
                    yield break;
                }

                audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
                yield return null;
            }

            callback?.Invoke();
        }

        private void ScanForEndedSources()
        {
            var model = new Dictionary<int, AudioSourceData>();

            foreach (int k in sourceMedia.Keys)
            {
                var source = sourceMedia[k];
                if (!source.OnPause && !source.Source.isPlaying)
                {
                    model.Add(k, source);
                }
            }

            foreach (int k in model.Keys)
            {
                var source = model[k];
                sourceMedia.Remove(k);

                SmartDestroy(source.Source);
            }
        }

        private void SmartDestroy(Object obj)
        {
#if UNITY_EDITOR
            DestroyImmediate(obj);
#else
            Object.Destroy(obj);
#endif
        }

        #endregion Utils

        //Source - https://github.com/hexgrimm/Audio/blob/master/AudioController.cs
        internal class AudioSourceData
        {
            public AudioSource Source;
            public int AudioCode;
            public bool IsMusic;
            public float RequestedVolume;
            public bool OnPause;
        }

        internal class AudioPresenter : MonoBehaviour
        {
            public AudioListener AudioListener;
        }
    }
}