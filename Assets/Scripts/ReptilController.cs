using System;
using UnityEngine;

public class ReptilController : MonoBehaviour
{
    public Collider PlayerSensorArea;

    public Collider CurrentSensorArea;

    /// <summary>
    /// Transfor do alvo, no caso o player
    /// </summary>
    private GameObject target;

    /// <summary>
    /// Velocidade máxima do reptil
    /// </summary>
    private float MaxVelocity = 2f;

    /// <summary>
    /// SQR máximo do reptil
    /// </summary>
    private float sqrMaxVelocity;

    /// <summary>
    /// Posição inicial do reptil
    /// </summary>
    private int startXPosition;

    /// <summary>
    /// Controle para verificar se o reptil está andando
    /// </summary>
    private bool isReptilMoving = false;

    /// <summary>
    /// Controle para verificar se o reptil está atacando
    /// </summary>
    private bool isReptilAttacking = false;

    /// <summary>
    /// Componente de animação do reptil
    /// </summary>
    private Animator animatorComponent;

    /// <summary>
    /// Componente de física do reptil
    /// </summary>
    private Rigidbody rigidBodyComponent;

    void Start()
    {
        this.sqrMaxVelocity = this.MaxVelocity * this.MaxVelocity;
        this.startXPosition = Convert.ToInt32(this.transform.position.x);

        this.target = GameObject.FindWithTag("Player");
        this.animatorComponent = this.GetComponent<Animator>();
        this.rigidBodyComponent = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        this.animatorComponent.SetBool("IsReptilMoving", this.isReptilMoving);

        if (!this.isReptilAttacking)
        {
            //Persegue o jogador
            if (this.PlayerSensorArea != null && this.PlayerSensorArea.Equals(this.CurrentSensorArea))
            {
                //Ultrapassou a velocidade máxima permitida
                if (this.rigidBodyComponent.velocity.sqrMagnitude > this.sqrMaxVelocity)
                {
                    this.rigidBodyComponent.velocity = this.rigidBodyComponent.velocity.normalized * this.MaxVelocity;
                }
                else
                {
                    if (this.target.transform.position.x < this.transform.position.x)
                    {
                        this.rigidBodyComponent.AddForce(new Vector3(-7, 0));
                        this.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    }
                    else
                    {
                        this.rigidBodyComponent.AddForce(new Vector3(7, 0));
                        this.transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
                    }
                }

                this.isReptilMoving = true;
            }
            //Faz com que o inimigo volte ao ponto inicial
            else
            {
                int currentPosition = Convert.ToInt32(this.transform.position.x);

                if (currentPosition != this.startXPosition)
                {
                    //Ultrapassou a velocidade máxima permitida
                    if (this.rigidBodyComponent.velocity.sqrMagnitude > this.sqrMaxVelocity)
                    {
                        this.rigidBodyComponent.velocity = this.rigidBodyComponent.velocity.normalized * this.MaxVelocity;
                    }
                    else
                    {

                        if (this.startXPosition < currentPosition)
                        {
                            this.rigidBodyComponent.AddForce(new Vector3(-7, 0));
                            this.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                        }
                        else
                        {
                            this.rigidBodyComponent.AddForce(new Vector3(7, 0));
                            this.transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
                        }
                    }

                    this.isReptilMoving = true;
                }
                else
                {
                    this.isReptilMoving = false;
                    this.rigidBodyComponent.velocity = Vector3.zero;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            this.StartAttack();
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            this.StartAttack();
    }

    void StartAttack()
    {
        if (!isReptilAttacking)
        {
            this.isReptilMoving = false;
            this.isReptilAttacking = true;
            this.animatorComponent.SetTrigger("Attack");
            this.rigidBodyComponent.velocity = Vector3.zero;
        }
    }

    void FinishedAttack()
    {
        float distance = 0.4f;
        RaycastHit hit = new RaycastHit();

        Vector3 direction = this.transform.localScale.x > 0 ? Vector3.left : Vector3.right;
        Vector3 position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);

        Debug.DrawRay(position, direction * distance, Color.yellow);

        if (Physics.Raycast(position, direction, out hit, distance))
        {
            if (hit.collider.gameObject.tag == "Player")
                hit.collider.gameObject.GetComponent<PlayerController>().StartDie();
        }

        this.isReptilAttacking = false;
    }

    void DieFinished()
    {
        Destroy(this.gameObject);
    }

    public void Shooted()
    {
        CapsuleCollider collider = this.GetComponent<CapsuleCollider>();

        collider.center = new Vector3(0, 0, 0);
        collider.radius = 0.05f;
        collider.height = 0.25f;
        collider.direction = 0;

        this.animatorComponent.SetTrigger("Die");
    }
}
