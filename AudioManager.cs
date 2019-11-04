using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Subtegral.AudioUtility
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] [Tooltip("Put -1 for no pooling")]
        private int maxPoolSize;

        private static AudioManager instance;

        public static AudioManager GetInstance()
        {
            if (instance != null) return instance;
            instance = new GameObject("AudioUtility").AddComponent<AudioManager>();
            return instance;
        }

        public AudioManager SetPoolSize(int poolSize)
        {
            instance.maxPoolSize = poolSize;
            instance.PoolSources();
            return instance;
        }


        public void PlayOneShot(AudioClip clip)
        {
            if (maxPoolSize == -1)
                PlayAndDie(clip);
            else
                StartCoroutine(PlayFromPool(clip));
        }

        private void PlayAndDie(AudioClip clip)
        {
            var playerInstance = new GameObject("TemporaryAudioSource").AddComponent<AudioSource>();
            playerInstance.PlayOneShot(clip);
            Destroy(playerInstance.gameObject, clip.length);
        }

        private IEnumerator PlayFromPool(AudioClip clip)
        {
            var audioSource = audioPool.Dequeue();
            audioSource.gameObject.SetActive(true);
            audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
            audioPool.Enqueue(audioSource);
            audioSource.gameObject.SetActive(false);
        }

        private Queue<AudioSource> audioPool = new Queue<AudioSource>();

        private void PoolSources()
        {
            if (maxPoolSize == -1) return;

            GameObject objectCache;
            var poolSizePrecalculated = maxPoolSize - audioPool.Count;
            if (poolSizePrecalculated < 0)
            {
                for (var i = 0; i >poolSizePrecalculated; i--)
                {
                    Destroy(audioPool.Dequeue().gameObject);
                }
            }
            else
            {
                for (var i = 0; i < poolSizePrecalculated; i++)
                {
                    objectCache = new GameObject($"PooledAudioSource[{i}]", typeof(AudioSource));
                    objectCache.transform.SetParent(transform);
                    objectCache.SetActive(false);
                    audioPool.Enqueue(objectCache.GetComponent<AudioSource>());
                }
            }
        }
    }
}