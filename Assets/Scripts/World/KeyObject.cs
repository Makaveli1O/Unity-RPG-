using UnityEngine;
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
    public TDTile tile;
    public BiomePreset biome;
    public bool completed;
    private Animator animator;
    private float interactDistance = 2.5f;
    private float interactionTime = 10f;
    [SerializeField] private SerializedLocks[] locks;
    private GameObject player;
    private PlayerController pc;
    private Map mapRef;
    
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        pc = player.GetComponent<PlayerController>();
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
    /// Called every frame while the mouse is over the GUIElement or Collider.
    /// </summary>
    void OnMouseOver()
    {
        GameAssets.Instance.cursorHandler.SetCursorByType(CursorType.Interact);
        if (Input.GetMouseButtonDown(1))
        {
            if (Vector3.Distance(transform.position, player.transform.position) < interactDistance && !completed)
            {
                //simulate player interacting with the keystone
                pc.Interaction(interactionTime, this.gameObject);
            }else{
                SoundManager.PlaySound(SoundManager.Sound.Error, this.transform.position);
            }
        }
    }

    /// <summary>
    /// Called when the mouse is not any longer over the GUIElement or Collider.
    /// </summary>
    void OnMouseExit()
    {
        GameAssets.Instance.cursorHandler.SetCursorByType(CursorType.Basic);
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

    /// <summary>
    /// Sets this keystone to complete
    /// </summary>
    public void Completed(){
        //TODO sfx
        this.completed = true;
        SetUILockStatus(biome.name);
        animator.Play("Destroy");
        //save keystone's state
        mapRef = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();
        SaveKeyObject keystone = new SaveKeyObject(tile, true);
        mapRef.SaveKeyObject(keystone);
    }
}

