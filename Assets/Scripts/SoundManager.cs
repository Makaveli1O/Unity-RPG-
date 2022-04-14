using System.Collections.Generic;
using UnityEngine;
using System.Collections;

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
        Error,
        Miss,
        Casting,
        WindSpell,
        ShieldHit,
        ShieldPulse,
        ShieldToggle,
        Theme_menu,
        Theme_gameplay,
        Keystone_acquired,
        Heal,
        SmashGround,
        ButtonPressed,
        ButtonHover,
        CooldownDone,
        ShieldBreak,
        Theme_victory
    }
    private static GameObject looping = null;
    public static bool casting;
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

    public static void PlayVictoryMusic(Sound sound){
        GameObject.Destroy(looping);
        PlaySound(sound);
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
                    float maxTime = .35f;   //delay
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

    public static void LoopMusic(Sound sound){
        if (CanPlaySound(sound))
        {
            GameObject soundGameObject = new GameObject("Musictheme");
            AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = 0.3f;
            //sound sent from preset
            audioSource.clip = GetAudioClip(sound);
            looping = soundGameObject;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Loops sound for given length, then destroys the GO.
    /// </summary>
    /// <param name="length">Time to loop</param>
    /// <param name="sound">Desired sound effet</param>
    /// <param name="position">Position of the sound effect.</param>
    /// <returns></returns>
    public static IEnumerator Loop(float length, Sound sound, Vector3 position){
        GameObject soundGameObject = new GameObject("loopSfx");
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        //sound sent from preset
        audioSource.clip = GetAudioClip(sound);
        audioSource.Play();
        yield return new WaitUntil(()=>!casting);
        //yield return new WaitForSeconds(length);
        GameObject.Destroy(soundGameObject);
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

