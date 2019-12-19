using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopchanGames.Examples
{
    public class PlayRandomSound : MonoBehaviour
    {
        public AudioClip[] clips;

        public void PlayRandomClip()
        {
            if (clips.Length == 0)
            {
                return;
            }
            var clip = clips[Random.Range(0, clips.Length)];
            SoundManager.Instance.PlayAudioClip(clip);
        }

    }
}
