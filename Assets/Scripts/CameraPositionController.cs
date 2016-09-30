using UnityEngine;
using System.Collections;

public class CameraPositionController : MonoBehaviour
{
    private const float FLOOR_000_Y = 1.09f;
    private const float FLOOR_001_Y = -6.65f;
    private const float FLOOR_002_Y = -14.39f;
    private const float FLOOR_003_Y = -22.15f;
    private const float FLOOR_004_Y = -29.87f;
    private const float FLOOR_005_Y = -37.61f;
    private const float FLOOR_006_Y = -45.35f;
    private const float FLOOR_007_Y = -53.09f;

    private const float BLOCK_000_X = 0f;
    private const float BLOCK_001_X = 11.88f;
    private const float BLOCK_002_X = 24.2f;

    void Update()
    {
        Vector3 position = Camera.main.transform.position;

        if(this.transform.position.x > 18.2f)
            position.x = BLOCK_002_X;
        else if (this.transform.position.x > 6.59f)
            position.x = BLOCK_001_X;
        else 
            position.x = BLOCK_000_X;

        if (this.transform.position.y > -2.65f)
            position.y = FLOOR_000_Y;
        else if (this.transform.position.y > -10.54f)
            position.y = FLOOR_001_Y;
        else if (this.transform.position.y > -19.5f)
            position.y = FLOOR_002_Y;
        else if (this.transform.position.y > -27.4f)
            position.y = FLOOR_003_Y;
        else if (this.transform.position.y > -35.26f)
            position.y = FLOOR_004_Y;
        else if (this.transform.position.y > -42.96f)
            position.y = FLOOR_005_Y;
        else if(this.transform.position.y > -50.6f)
            position.y = FLOOR_006_Y;
        else
            position.y = FLOOR_007_Y;

        if (Camera.main.transform.position != position)
        {
            GameObject particleSetArmor = this.GetComponent<PlayerController>().ParticleSetArmor;

            if(particleSetArmor != null)
                particleSetArmor.SetActive(false);
        }

        Camera.main.transform.position = position;
    }
}
