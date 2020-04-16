using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayerAction : MonoBehaviour
{
    private AudioSource voice;

    private int selectedFile;

    [System.Serializable]
    public class Sound
    {
        public AudioClip soundFile;
        public bool enableParameters;

        [MinMaxRange(0.5f, 2)]
        public RangedFloat pitch;

        public float volume;
        public float pitchRandomness;
    }
    public Sound[] nameOfSound;

    private float timeLastSound;
    private float clipLength;

    public RangedFloat volume;

    public void PlayTheSound()
    {
        TriggerSoundEvent(nameOfSound, voice, ref clipLength, ref timeLastSound);
    }

    private void Start()
    {
        voice = GetComponent<AudioSource>();
        //        voice = transform.GetChild(0).GetComponent<AudioSource>();
    }

    private void TriggerSoundEvent(Sound[] sound, AudioSource audioSource, ref float clipLength, ref float timeLastSound)
    {
        float timeSinceLastSound = Time.fixedTime - timeLastSound;

        if (timeSinceLastSound > clipLength)
        {
            selectedFile = Random.Range(0, sound.Length);
            audioSource.clip = sound[selectedFile].soundFile;

            clipLength = audioSource.clip.length;
            timeLastSound = Time.fixedTime;

            if (sound[selectedFile].enableParameters == true)
            {
                audioSource.pitch = Random.Range(sound[selectedFile].pitch.minValue, sound[selectedFile].pitch.maxValue);
                audioSource.volume = sound[selectedFile].volume;
            }
            else
            {
                audioSource.pitch = 1;
                audioSource.volume = 1;
            }
            audioSource.Play();
        }
    }
}
