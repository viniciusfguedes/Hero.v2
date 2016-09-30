using UnityEngine;

public class ExplosionDamagedController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ExplosionCollider" && this.tag == "Enemie")
        {
            this.gameObject.GetComponent<EnemieController>().Shooted();
        }
        else if (other.tag == "ExplosionCollider" && this.tag == "Wall")
        {
            Destroy(this.gameObject);
        }
    }
}
