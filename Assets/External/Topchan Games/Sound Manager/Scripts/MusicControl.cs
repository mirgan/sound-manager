using UnityEngine;
using UnityEngine.UI;

namespace TopchanGames
{
    public class MusicControl : MonoBehaviour
    {
        private Toggle toggle;
        private Slider slider;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            toggle = GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(ToggleMusic);
                toggle.SetIsOnWithoutNotify(SoundManager.Instance.MusicEnabled);
            }

            slider = GetComponent<Slider>();
            if (slider != null)
            {
                slider.onValueChanged.AddListener(ChangeMusicVolume);
                slider.SetValueWithoutNotify(SoundManager.Instance.MusicVolume);
            }
        }

        private void ToggleMusic(bool value)
        {
            SoundManager.Instance.MusicEnabled = value;
        }

        private void ChangeMusicVolume(float value)
        {
            SoundManager.Instance.MusicVolume = value;
        }
    }
}