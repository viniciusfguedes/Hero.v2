using UnityEngine;
using System.Collections;

public class ProjectileScript : MonoBehaviour 
{
    private float currentTimeToDismiss;
    private float timeToDismiss = 0.4f;
    public GameObject impactParticle;
    public GameObject projectileParticle;
    public GameObject[] trailParticles;
    [HideInInspector]
    public Vector3 impactNormal;
	
	void Start () 
	{
        projectileParticle = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
        projectileParticle.transform.parent = transform;
	}

    void Update()
    {
        this.currentTimeToDismiss += Time.deltaTime;

        if (this.currentTimeToDismiss >= this.timeToDismiss)
            Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider hit)
    {
        if (!hit.gameObject.CompareTag("Player") && !hit.gameObject.CompareTag("SensorArea"))
        {
            impactParticle = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;

            if (hit.gameObject.tag == "Reptil") 
            {
                hit.gameObject.GetComponent<ReptilController>().Shooted();
            }

            foreach (GameObject trail in trailParticles)
            {
                GameObject curTrail = transform.Find(projectileParticle.name + "/" + trail.name).gameObject;
                curTrail.transform.parent = null;
                Destroy(curTrail, 3f);
            }
            Destroy(projectileParticle, 3f);
            Destroy(impactParticle, 5f);
            Destroy(gameObject);
        }
	}
}