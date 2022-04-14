using Unity.Mathematics;
using UnityEngine;

public class QuestUIPointer : MonoBehaviour
{
    private KeyObject targetObject;
    [SerializeField]private RectTransform pointerTransform;

    bool pointing = false;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        pointerTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(pointing){
            Vector3 toPosition = int2ToVector3(targetObject.tile.pos);
            Vector3 fromPosition = Camera.main.transform.position;
            Vector3 dir = (toPosition - fromPosition).normalized;
            float angle = GetAngleFromVectorFloat(dir);
            pointerTransform.localEulerAngles = new Vector3(0,0,angle);
        }
    }

    public static float GetAngleFromVectorFloat(Vector3 dir) {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }
    public void StartPointing(KeyObject obj){
        this.targetObject = obj;
        pointing = true;
        this.gameObject.SetActive(true);
        obj.UIpointer = this;
    }

    /// <summary>
    /// Quickly converts int2 coords to vector3 ( ints )
    /// </summary>
    /// <param name="pos">position in ints</param>
    /// <returns>Tile int2 position represented by vetcor3</returns>
    private Vector3 int2ToVector3(int2 pos){
        return new Vector3(pos.x, pos.y);
    }
}
