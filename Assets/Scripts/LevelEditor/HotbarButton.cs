using UnityEngine;
using UnityEngine.UI;

public class HotbarButton : MonoBehaviour
{
    public LevelEditor levelEditor;

    [Tooltip("Prefab to choose")]
    public GameObject objectPrefab;

    public void SetObjectToSpawn()
    {
        levelEditor.ChangeObjectToSpawn(objectPrefab);
    }
}