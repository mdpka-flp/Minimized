using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture;

    void Start()
    {
        if (cursorTexture != null)
        {
            // центр картинки как точка курсора
            Vector2 hotSpot = new Vector2(cursorTexture.width / 2f, cursorTexture.height / 2f);
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        }
    }
}
