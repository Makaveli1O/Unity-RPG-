using UnityEngine;
using Unity.Mathematics;
using System.Collections;

/// <summary>
/// Gamestate handling class. This class handles retrieving correct gamestate. Correctly
/// retrieve map and player's states. Such as positions, numbers textures etc.
/// </summary>
public class GameHandler : MonoBehaviour
{
    private string SAVE_FOLDER;
    float world_seed; //parent folder for all things within this seed
    private Map mapRef; 
    [SerializeField] GameObject PauseMenuGO;
    [SerializeField] GameObject VictoryMenuGO;
    private PauseMenu pauseMenuScript;
    private GameOver victoryMenuScript;
    private enum KeyObjectHandlerType
    {
        Save,
        Load
    }
    private void Start(){
        mapRef = GetComponent<Map>();
        world_seed = mapRef.seed;
        SaveSystem.Init();
        SAVE_FOLDER = Application.dataPath + "/Saves/";
        SoundManager.Init();    //initialize soundManager
        GameAssets.Instance.cursorHandler.SetCursorByType(CursorType.Basic);
        pauseMenuScript = PauseMenuGO.GetComponent<PauseMenu>();
        victoryMenuScript = VictoryMenuGO.GetComponent<GameOver>();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        //Pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenuScript.OpenMenu();
        }   
    }

    public void PlayMainTheme(){
        SoundManager.LoopMusic(SoundManager.Sound.Theme_gameplay);
    }

    public void LoadVictoryScreen(){
        victoryMenuScript.OpenMenu();
        SoundManager.PlayVictoryMusic(SoundManager.Sound.Theme_victory);
    }

    
    /// <summary>
    /// Saves given object to JSON file. ObjType object are used to save specific object type. Files are
    /// classified by world seed number.
    /// </summary>
    /// <param name="saveObj">Object to be saved</param>
    /// <param name="key">Type of object</param>
    /// <param name="position">Position ( used in player and objects)</param>
    /// <typeparam name="T"></typeparam>
     public void Save<T>(T saveObj, ObjType key, Vector3 position, BiomePreset biome = null){
        string json = JsonUtility.ToJson(saveObj);

        if(key == ObjType.Player){
            SaveSystem.Save(json,key+".json", world_seed.ToString());
            //saving key objects
        }else if(key == ObjType.KeyObject){
            KeyObjectHandler(biome, json, key, KeyObjectHandlerType.Save);
        //save chunks with key in name
        }else{
            SaveSystem.Save(json,key+"_"+position.x+","+position.y+".json", world_seed.ToString());
        }
    }

    /// <summary>
    /// Load specific object from JSON file.
    /// </summary>
    /// <param name="key">Type of object to search for.</param>
    /// <param name="x_pos">X position of object(optional)</param>
    /// <param name="y_pos">Y position of object(optional)</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Loaded object or null</returns>
    public T Load<T>(ObjType key, BiomePreset biome = null, int x_pos = -1, int y_pos = -1){
        //read saved json
        string saveString = null;

        //load on specific coords
        if (x_pos != -1 || y_pos != -1){
            saveString = SaveSystem.Load(key+"_"+x_pos+","+y_pos+".json", world_seed.ToString());
        //key objects
        }else if(key == ObjType.KeyObject){
            saveString = KeyObjectHandler(biome, null, key, KeyObjectHandlerType.Load);
        }else{
            saveString = SaveSystem.Load(key+".json", world_seed.ToString());
        }
        
        T returnValue = default(T);
        if (saveString != null)
        {
            //json serialize
            returnValue = (T)JsonUtility.FromJson<T>(saveString);
        }

        return returnValue;
    }

    /// <summary>
    /// Support function for handling save and load of key objects.
    /// </summary>
    /// <param name="biome">Type of biome</param>
    /// <param name="json">json string</param>
    /// <param name="key">Object key type</param>
    /// <param name="type">Type of key object operation (save/load)</param>
    /// <returns></returns>
    private string KeyObjectHandler (BiomePreset biome,string json, ObjType key, KeyObjectHandlerType type){
        string saveString = null;
        //save key ob
        if (type == KeyObjectHandlerType.Save)
        {
            switch (biome.name)
            {
                case "Rainforest":
                    SaveSystem.Save(json,key+"_rainforest.json", world_seed.ToString());
                    break;
                case "Desert":
                    SaveSystem.Save(json,key+"_desert.json", world_seed.ToString());
                    break;
                case "Forest":
                    SaveSystem.Save(json,key+"_fores.json", world_seed.ToString());
                    break;
                case "Ashland":
                    SaveSystem.Save(json,key+"_ashland.json", world_seed.ToString());
                    break;
            }
        //load key obj
        }else{
            switch (biome.name)
            {
                case "Rainforest":
                    saveString = SaveSystem.Load(key+"_rainforest.json", world_seed.ToString());
                    break;
                case "Desert":
                    saveString = SaveSystem.Load(key+"_desert.json", world_seed.ToString());
                    break;
                case "Forest":
                    saveString = SaveSystem.Load(key+"_fores.json", world_seed.ToString());
                    break;
                case "Ashland":
                    saveString = SaveSystem.Load(key+"_ashland.json", world_seed.ToString());
                    break;
            }
        }
        return saveString;
    }
}

/// <summary>
/// Class holding positional information about to save.
/// </summary>
[System.Serializable]
public class SavePosition
{
    public Vector3 pos;
    public int healthAmount;
    public int shieldAmount;
    public SavePosition(Vector3 pos, int health, int shield){
        this.pos = pos;
        this.healthAmount = health;
        this.shieldAmount = shield;
    }
}

/// <summary>
/// Class representing key object to save in regular form.
/// </summary>
[System.Serializable]
public class SaveKeyObject
{
    [System.NonSerialized] public TDTile tile;
    public Vector3 position;
    public string biome;
    public bool completed;

    public SaveKeyObject(TDTile tile, bool cmpl = false){
        this.tile = tile;
        int2 coord = new int2(tile.pos.x, tile.pos.y);
        Vector3 pos = new Vector3(coord.x, coord.y, 0);
        this.position = pos;
        this.biome = tile.biome.name;
        this.completed = cmpl;
    }
}

/// <summary>
/// Class for saving chunks.(only necessary intel)
/// </summary>
[System.Serializable]
public class SaveChunk
{
    public Vector3 pos;

    public SaveChunk(Vector3 pos){
        this.pos = pos;
    }
}

/// <summary>
/// Determining type of saving object to adjust behaviour when saving and loading.
/// </summary>
public enum ObjType{
    Player,
    Entity,
    Chunk,
    KeyObject,
}


    