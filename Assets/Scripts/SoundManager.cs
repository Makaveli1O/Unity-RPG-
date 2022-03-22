using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible for playing all of sounds.
/// </summary>
public static class SoundManager
{
    public enum Sound{
        GrassStep,
        Attack,
        Death,
        Hit,
        Hurt,
        Dash,
        Error
    }

    private static Dictionary<Sound,float> soundTimerDictionary;
    private static GameObject oneShotGameObject;
    private static AudioSource oneShotAudioSource;
    private static AudioClip lastPlayed;

    public static void Init(){
        soundTimerDictionary = new Dictionary<Sound, float>();
        soundTimerDictionary[Sound.GrassStep] = 0f;
    }

    /// <summary>
    /// Plays sounds in 3D space.
    /// </summary>
    /// <param name="sound">Desired sound effect.</param>
    /// <param name="position">Position of the sound</param>
    public static void PlaySound(Sound sound, Vector3 position, AudioClip presetClip = null){
        if (CanPlaySound(sound))
        {
            GameObject soundGameObject = new GameObject("Sound");
            soundGameObject.transform.position = position;
            AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            //sound sent from preset
            if (presetClip != null)
            {
                audioSource.clip = presetClip;
            }else{
                audioSource.clip = GetAudioClip(sound);
            }
            
            audioSource.Play();

            //destroy after being played
            Object.Destroy(soundGameObject, audioSource.clip.length);
        }
    }

    /// <summary>
    /// Plays specific sound
    /// </summary>
    /// <param name="sound">Desired sound to be played</param>
    public static void PlaySound(Sound sound){
        if (CanPlaySound(sound))
        {
            if (oneShotGameObject == null)
            {
                oneShotGameObject = new GameObject("sfx");
                oneShotAudioSource = oneShotGameObject.AddComponent<AudioSource>();
            }
            oneShotAudioSource.PlayOneShot(GetAudioClip(sound));
        }
    }

    /// <summary>
    /// Returns boolean if sound can be played or not
    /// </summary>
    /// <param name="sound">Specified sound type.</param>
    /// <returns>Sound can be played or not</returns>
    private static bool CanPlaySound(Sound sound){
        switch (sound)
        {
            case Sound.GrassStep:
                if (soundTimerDictionary.ContainsKey(sound))
                {
                    float lastTimePlayed = soundTimerDictionary[sound]; //last time this sound was played
                    float maxTime = .25f;   //delay
                    //is last time after delay we want?
                    if (lastTimePlayed + maxTime < Time.time)
                    {
                        soundTimerDictionary[sound] = Time.time;
                        return true;
                    }else{
                        return false;
                    }
                }else{
                    return true;
                }
            //most sounds will return true, but for example footstep it won't
            default:
                return true;
        }
        
    }

    /// <summary>
    /// Find proper sound file.
    /// </summary>
    /// <param name="sound">Required sound</param>
    /// <returns>Found audio clip.</returns>
    private static AudioClip GetAudioClip(Sound sound){
        foreach(GameAssets.SoundAudioClip soundAudioClip in GameAssets.Instance.playerSoundAudioClipArray){
            if (soundAudioClip.sound == sound && soundAudioClip.audioClip != lastPlayed)
            {
                lastPlayed = soundAudioClip.audioClip;
                return soundAudioClip.audioClip;
            }
        }

        return lastPlayed;
    }

}

