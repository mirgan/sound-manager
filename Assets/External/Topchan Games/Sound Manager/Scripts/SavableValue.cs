using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace TopchanGames
{
    [Serializable]
    public sealed class SavableValue<T>
    {
        private readonly string playerPrefsPath;
        private T value;
        private bool loaded;

        public event Action OnChanged = () => { };

        public T Value
        {
            get
            {
                if (!loaded)
                {
                    LoadFromPrefs();
                    loaded = true;
                }

                return value;
            }
            set
            {
                PrevValue = this.value;
                this.value = value;
                SaveToPrefs();
                OnChanged.Invoke();
            }
        }

        public T PrevValue
        {
            get;
            private set;
        }

        public SavableValue(string playerPrefsPath, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(playerPrefsPath))
            {
                throw new Exception("empty playerPrefsPath in saveableValue");
            }

            this.playerPrefsPath = playerPrefsPath;

            value = defaultValue;
            PrevValue = defaultValue;
        }

        private void LoadFromPrefs()
        {
            if (!PlayerPrefs.HasKey(playerPrefsPath))
            {
                SaveToPrefs();
                return;
            }

            string stringToDeserialize = PlayerPrefs.GetString(playerPrefsPath, "");

            var bytes = Convert.FromBase64String(stringToDeserialize);
            var memoryStream = new MemoryStream(bytes);
            var bf = new BinaryFormatter();

            value = (T) bf.Deserialize(memoryStream);
        }

        private void SaveToPrefs()
        {
            var memoryStream = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(memoryStream, value);
            string stringToSave = Convert.ToBase64String(memoryStream.ToArray());

            PlayerPrefs.SetString(playerPrefsPath, stringToSave);
        }
    }
}