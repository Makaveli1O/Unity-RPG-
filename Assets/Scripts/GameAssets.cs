using UnityEngine;

/// <summary>
/// Class holding all game assets, so they can be accessed even in non monobehaviour scripts.
/// </summary>
public class GameAssets : MonoBehaviour
{
    public CursorHandler cursorHandler;
    private static GameAssets _i;
    public static GameAssets Instance{
        get{
            //internal reference does not exist, instantiate one
            if (_i == null) _i = (Instantiate(Resources.Load("GameAssets")) as GameObject).GetComponent<GameAssets>();
            return _i;
        }
    }

    [System.Serializable]
    public class SoundAudioClip{
        public SoundManager.Sound sound;
        public AudioClip audioClip;

    }

    [System.Serializable]
    public class Cursors{
        public CursorType type;
        public Texture2D texture;

    }

    public Transform pfDamagePopup;
    public SoundAudioClip[] playerSoundAudioClipArray;
    public Cursors[] cursors;
    
}