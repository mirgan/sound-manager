using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopchanGames.Examples
{
    public class PlayMusic : MonoBehaviour
    {
        public float fadeDuration = 2f;
        public float mixDuration = 5f;
        public AudioClip[] musics;
        private int curIndex;
        private int lastClipId;
        public void PlayNextMusic()
        {
            var musicClip = GetNextClip();
            lastClipId = SoundManager.Instance.PlayMusicClip(musicClip, fadeDuration);
        }

        public void PlayNextMusicWithMix()
        {
            var musicClip = GetNextClip();
            lastClipId = SoundManager.Instance.PlayMusicClipWithMix(musicClip, mixDuration);
        }

        public void StopPlayLastMusic()
        {
            SoundManager.Instance.StopPlayingClip(lastClipId);
        }

        public void PauseLastMusic()
        {
            SoundManager.Instance.PausePlayingClip(lastClipId);
        }

        public void ResumeLastMusic()
        {
            SoundManager.Instance.ResumeClipIfInPause(lastClipId);
        }

        private AudioClip GetNextClip()
        {
            curIndex++;
            if (curIndex == musics.Length)
            {
                curIndex = 0;
            }

            return musics[curIndex];
        }
    }
}
