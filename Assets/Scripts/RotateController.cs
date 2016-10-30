using UnityEngine;
using System.Collections;

public class RotateController : MonoBehaviour
{
    private void Update()
    {
        this.transform.Rotate(Vector3.up * Time.deltaTime * 100);
    }
}
