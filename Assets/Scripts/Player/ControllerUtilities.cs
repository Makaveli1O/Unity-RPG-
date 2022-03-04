using UnityEngine;

public class ControllerUtilities : MonoBehaviour
{
    public Vector3 GetMouseWorldPosition(){
        var v = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1);
        return Camera.main.ScreenToWorldPoint(v);
    }

    /// <summary>
    /// Calculate vector between player and mouse.
    /// </summary>
    /// <param name="mousePos">Vector3 position of the mouse</param>
    /// <returns>Correct looking direction</returns>
    public Vector3 GetLookingDir(Vector3 mousePos){
        float x = mousePos.x - this.transform.position.x;
        float y = mousePos.y - this.transform.position.y;
        
        return new Vector3(x,y).normalized;
    }
}
