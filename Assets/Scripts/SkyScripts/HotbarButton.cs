using UnityEngine;
using UnityEngine.UI;

public class HotbarButton : MonoBehaviour
{
    public LevelEditor levelEditor;
    public Image buttonImage;


    [Tooltip("Prefab to choose")]
    public GameObject objectPrefab;

    void Start()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = objectPrefab.GetComponent<SpriteRenderer>().sprite;
        }
    }

    public void SetObjectToSpawn()
    {
        levelEditor.ChangeObjectToSpawn(objectPrefab);
    }
}
