using System;
using UnityEngine;

namespace Subtegral.AudioUtility
{
    [Serializable]
    public sealed class AudioBus
    {
        public string Name;
        public float Volume = 1f;
        public float Pitch = 1f;
        public bool Mute = false;

        internal void ApplyBus(AudioSource source)
        {
            source.volume = Volume;
            source.pitch = Pitch;
            source.mute = Mute;
        }
    }
}