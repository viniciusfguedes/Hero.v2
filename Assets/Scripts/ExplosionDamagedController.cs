using UnityEngine;

public class ExplosionDamagedController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "ExplosionCollider")
        {
            Destroy(this.gameObject);
        }
    }
}
