using UnityEngine;

public class ExplosionDamagedController : MonoBehaviour
{
    void Start()
    {
        AudioSource audioSource = this.GetComponent<AudioSource>();

        if (audioSource != null)
            this.GetComponent<AudioSource>().volume = GameObject.Find("PreferencesController").GetComponent<PreferencesController>().EffectsVolume;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ExplosionCollider" && this.tag == "Enemie")
        {
            this.gameObject.GetComponent<EnemieController>().Shooted();
        }
        else if (other.tag == "ExplosionCollider" && this.tag == "LavaWall")
        {
            this.GetComponent<AudioSource>().Play();
            this.GetComponent<Renderer>().enabled = false;
            Destroy(this.gameObject, 1);
        }
    }
}
