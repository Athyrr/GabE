using UnityEngine;

public class Rotate : MonoBehaviour
{
    void Update()
    {
        transform.rotation *= new Quaternion(3f, 1f, 1f, 1f);
    }
}
