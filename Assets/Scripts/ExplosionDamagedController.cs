using UnityEngine;

public class ExplosionDamagedController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ExplosionCollider" && this.tag == "Reptil")
        {
            Debug.Log("Reptil");
            this.gameObject.GetComponent<ReptilController>().Shooted();
        }
        else if (other.tag == "ExplosionCollider" && this.tag == "Wall")
        {
            Destroy(this.gameObject);
        }
    }
}
