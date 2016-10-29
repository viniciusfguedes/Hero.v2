using UnityEngine;
using System.Collections;

public class RotateController : MonoBehaviour {

    float time = 1.0f;
    float angle = 360.0f;
    Vector3 axis = Vector3.up;

    private void Update()
    {
        this.transform.Rotate(Vector3.up * Time.deltaTime * 100);
    }
}
