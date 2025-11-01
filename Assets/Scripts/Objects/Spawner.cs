using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public bool DestroyOldCube = true;
    public bool SpawnOnStart = false;


    [Header("Cube Settings")]
    public CollisionMode collisionMode;
    public ObjColor objColor;
    public enum CollisionMode
    {
        DisableWhenHeld,
        NeverDisable,
        AlwaysDisable
    }
    public enum ObjColor
    {
        Red,
        Green,
        Blue
    }
    public float scaleSpeed = 5f;
    public float massSpeed = 5f;
    public float minScale = 1f;
    public float maxScale = 2f;
    public float minMass = 5f;
    public float maxMass = 15f;

    [Header("Cube prefab")]
    [SerializeField] private GameObject cubePrefab; // Префаб куба
    private GameObject currentCube; // Текущий спавненный куб
    public ShatterManager shatterManager;

    public void SpawnCube()
    {
        // Если уже есть куб — удаляем его
        if (currentCube != null && DestroyOldCube)
        {
            Destroy(currentCube);
        }

        // Спавним новый куб в позиции спавнера
        currentCube = Instantiate(cubePrefab, transform.position, Quaternion.identity);
    }

    public void DestroyCube()
    {
        
    }
}
