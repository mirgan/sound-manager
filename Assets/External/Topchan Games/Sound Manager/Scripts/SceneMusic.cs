using System.Linq;
using UnityEngine;

namespace TopchanGames
{
    public class SceneMusic : MonoBehaviour
    {
        [SerializeField]
        protected AudioClip[] musics;

        [SerializeField]
        protected float mixDuration;

        [SerializeField]
        protected bool loopMusic;

        [SerializeField]
        protected bool shuffleTracks;

        private AudioClip lastClip;

        private void Awake()
        {
            StartPlayMusic();
        }

        private void OnEnable()
        {
            if (loopMusic)
            {
                SoundManager.Instance.OnMusicPlayCompleted += PlayNextTrackAfterComplete;
            }
        }

        private void OnDisable()
        {
            if (loopMusic && SoundManager.Instance != null)
            {
                SoundManager.Instance.OnMusicPlayCompleted -= PlayNextTrackAfterComplete;
            }
        }

        private void StartPlayMusic()
        {
            if (!enabled)
            {
                return;
            }
            if (shuffleTracks)
            {
                ShuffleTracks();
            }

            lastClip = musics.FirstOrDefault(music => SoundManager.Instance.IsMusicClipPlaying(music));
            if (lastClip != null)
            {
                return;
            }

            PlayNextTrack(true);
        }

        private void PlayNextTrackAfterComplete()
        {
            PlayNextTrack(false);
        }

        private void PlayNextTrack(bool mixMusic)
        {
            var nextTracks = musics.Length > 1 ? musics.Where(m => m != lastClip).ToArray() : musics;
            if (nextTracks.Length == 0)
            {
                Debug.LogWarning("SceneMusic: Next music track not found");
                return;
            }

            lastClip = nextTracks[Random.Range(0, nextTracks.Length)];
            SoundManager.Instance.PlayMusicClip(lastClip, mixMusic ? mixDuration : 0);
        }

        public void ShuffleTracks()
        {
            var shuffledArray = new AudioClip[musics.Length];
            musics.CopyTo(shuffledArray, 0);
            int count = shuffledArray.Length;
            int last = count - 1;
            for (int i = 0; i < last; ++i)
            {
                int r = Random.Range(i, count);
                var tmp = shuffledArray[i];
                shuffledArray[i] = shuffledArray[r];
                shuffledArray[r] = tmp;
            }

            musics = shuffledArray;
        }
    }
}