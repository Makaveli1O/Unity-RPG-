using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;

public class KeyObject : MonoBehaviour
{
    /*  *   *   *   *   *   *   *
        Key object's stuff
    *   *   *   *   *   *   *  */
    [System.Serializable]private class SerializedLocks{
        public string name;
        public bool unlocked;
        public Sprite sprite;
    }
    public int2 pos;
    public BiomePreset biome;
    public bool completed;
    private Animator animator;
    [SerializeField] private SerializedLocks[] locks;
    
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    private void Start() {
        if (this.completed)
        {
            animator.Play("Destroy");
        }else{
            animator.Play("Idle");
        }
        SetUILockStatus(biome.name);
    }

    /// <summary>
    /// Handles top UI bar, altering lock icons to indicate, whener key objects
    /// have been claimed or not.
    /// </summary>
    /// <param name="biome">biome name.</param>
    private void SetUILockStatus(string biome){
        biome = biome.ToLower();
        if (biome.Equals("rainforest")) biome = "jungle";
        if (completed)
        {
            GameObject icon = GameObject.Find(biome);
            Image img = icon.GetComponent<Image>();
            img.sprite = GetLock(biome, true);
        }else{
            GameObject icon = GameObject.Find(biome);
            Debug.Log(biome);
            Image img = icon.GetComponent<Image>();
            img.sprite = GetLock(biome, false);
        }  
    }

    /// <summary>
    /// Finds corresponding lock sprite icon.
    /// </summary>
    /// <param name="name">Name of the sprite ( colour )</param>
    /// <param name="unlocked">Boolean whenever lock should be unlocked or not.</param>
    /// <returns>Corresponding sprite from locks list.</returns>
    private Sprite GetLock(string name, bool unlocked){
        Sprite retVal = null;
        foreach (SerializedLocks item in locks)
        {
            if (name.Equals(item.name) && item.unlocked == unlocked)
            {
                retVal = item.sprite;
            }
        }
        return retVal;
    }
}

