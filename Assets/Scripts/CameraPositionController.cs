using UnityEngine;
using System.Collections;

public class CameraPositionController : MonoBehaviour
{
    private const float FLOOR_000_Y = 1.09f;
    private const float FLOOR_001_Y = -6.65f;
    private const float FLOOR_002_Y = -14.39f;

    private const float BLOCK_000_X = 0f;
    private const float BLOCK_001_X = 11.38f;

    void Update()
    {
        Vector3 position = Camera.main.transform.position;

        //Lado direito
        if (this.transform.position.x > 6.69f)
            position.x = BLOCK_001_X;
        //Lado esquerdo
        else
            position.x = BLOCK_000_X;

        //Andar 0
        if(this.transform.position.y > -2.65f)
            position.y = FLOOR_000_Y;
        //Andar 1
        else if(this.transform.position.y > -10.54f)
            position.y = FLOOR_001_Y;
        //Andar 2
        else
            position.y = FLOOR_002_Y;

        Camera.main.transform.position = position;
    }
}
