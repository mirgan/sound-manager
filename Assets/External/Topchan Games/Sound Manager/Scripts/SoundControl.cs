using UnityEngine;
using UnityEngine.UI;

namespace TopchanGames
{
    public class SoundControl : MonoBehaviour
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
                toggle.onValueChanged.AddListener(ToggleSound);
                toggle.SetIsOnWithoutNotify(SoundManager.Instance.SoundEnabled);
            }

            slider = GetComponent<Slider>();
            if (slider != null)
            {
                slider.onValueChanged.AddListener(ChangeSoundVolume);
                slider.SetValueWithoutNotify(SoundManager.Instance.SoundVolume);
            }
        }

        private void ToggleSound(bool value)
        {
            SoundManager.Instance.SoundEnabled = value;
        }

        private void ChangeSoundVolume(float value)
        {
            SoundManager.Instance.SoundVolume = value;
        }
    }
}