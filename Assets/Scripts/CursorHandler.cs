using UnityEngine;

public class CursorHandler : MonoBehaviour
{
    public void SetCursorByType(CursorType type){
        foreach (GameAssets.Cursors cursor in GameAssets.Instance.cursors)
        {
            if (cursor.type.Equals(type))
            {
                Cursor.SetCursor(cursor.texture, Vector2.zero, CursorMode.ForceSoftware);
            }
        }
    }
}

/// <summary>
/// Enum differing types of curors.
/// </summary>
public enum CursorType
{
    Basic,
    Apply,
    Attack,
    Interact
}