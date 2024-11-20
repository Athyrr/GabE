using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        transform.rotation *= new Quaternion(3f, 1f, 1f, 1f);
    }
}
