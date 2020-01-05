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

        private Dictionary<string, Queue<AudioSource>> audioPool = new Dictionary<string, Queue<AudioSource>>();
        private List<AudioSource> loopingSourceList = new List<AudioSource>();

        private static AudioManager instance;
        private AudioBus masterBus;

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        public static AudioManager GetInstance()
        {
            if (instance != null) return instance;
            instance = new GameObject("AudioUtility").AddComponent<AudioManager>();
            instance.masterBus = new AudioBus {Name = "MasterBus"};
            instance.audioPool.Add(instance.masterBus.Name,new Queue<AudioSource>());
            return instance;
        }

        public AudioManager AddBus(AudioBus bus)
        {
            instance.audioPool.Add(bus.Name,new Queue<AudioSource>());
            return instance;
        }

        public AudioManager SetPoolSize(int poolSize)
        {
            instance.maxPoolSize = poolSize;
            instance.PoolSources();
            return instance;
        }

        public AudioSource PlayLooping(AudioClip clip, AudioBus bus = null)
        {
            bus = bus ?? masterBus;
            var source = PlayLoopingFromPool(clip, bus);
            loopingSourceList.Add(source);
            return source;
        }

        public void StopLooping(AudioSource source)
        {
            loopingSourceList.Remove(source);
            Destroy(source.gameObject);
        }

        public void PlayOneShot(AudioClip clip, AudioBus bus = null)
        {
            bus = bus ?? masterBus;
            if (maxPoolSize == -1)
                PlayAndDie(clip, bus);
            else
                StartCoroutine(PlayFromPool(clip, bus));
        }

        private void PlayAndDie(AudioClip clip, AudioBus bus)
        {
            var playerInstance = new GameObject($"[{bus}]TemporaryAudioSource").AddComponent<AudioSource>();
            bus.ApplyBus(playerInstance);
            playerInstance.PlayOneShot(clip);
            Destroy(playerInstance.gameObject, clip.length);
        }

        private IEnumerator PlayFromPool(AudioClip clip, AudioBus bus = null)
        {
            if(audioPool[bus.Name].Count==0)
                PoolSources();
            var audioSource = audioPool[bus.Name].Dequeue();
            audioSource.gameObject.SetActive(true);
            bus.ApplyBus(audioSource);
            audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
            audioPool[bus.Name].Enqueue(audioSource);
            audioSource.gameObject.SetActive(false);
        }

        private AudioSource PlayLoopingFromPool(AudioClip clip, AudioBus bus = null)
        {
            var audioSource = audioPool[bus.Name].Dequeue();
            audioSource.gameObject.SetActive(true);
            bus.ApplyBus(audioSource);
            audioSource.loop = true;
            audioSource.clip = clip;
            audioSource.Play();
            return audioSource;
        }

        private void PoolSources()
        {
            if (maxPoolSize == -1) return;
            GameObject objectCache;
            foreach (var bus in audioPool.Keys)
            {
                var poolSizePrecalculated = maxPoolSize - audioPool.Count;
                if (poolSizePrecalculated < 0)
                {
                    for (var i = 0; i > poolSizePrecalculated; i--)
                    {
                        Destroy(audioPool[bus].Dequeue().gameObject);
                    }
                }
                else
                {
                    for (var i = 0; i < poolSizePrecalculated; i++)
                    {
                        objectCache = new GameObject($"PooledAudioSource[{i}]", typeof(AudioSource));
                        objectCache.transform.SetParent(transform);
                        objectCache.SetActive(false);
                        audioPool[bus].Enqueue(objectCache.GetComponent<AudioSource>());
                    }
                }
            }
        }
    }
}