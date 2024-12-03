using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    // Time in seconds before the GameObject is destroyed
    public float destroyTime = 5f;

    // Start is called before the first frame update
    void Start()
    {
        // Destroy this GameObject after 'destroyTime' seconds
        Destroy(gameObject, destroyTime);
    }
}
