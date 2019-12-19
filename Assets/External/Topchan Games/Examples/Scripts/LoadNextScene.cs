using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopchanGames.Examples
{
    public class LoadNextScene : MonoBehaviour
    {
        public string[] sceneNames;
        private int curIndex;

        private void Awake()
        {
            string curSceneName = SceneManager.GetActiveScene().name;
            curIndex = Array.IndexOf(sceneNames, curSceneName);
        }

        public void LoadNext()
        {
            curIndex++;
            if (curIndex == sceneNames.Length)
            {
                curIndex = 0;
            }

            SceneManager.LoadScene(sceneNames[curIndex]);
        }
    }
}
