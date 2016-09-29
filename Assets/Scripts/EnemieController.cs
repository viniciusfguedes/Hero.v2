using System;
using UnityEngine;

public class EnemieController : MonoBehaviour
{
    public bool Zaxis;

    public int ShootsToDie;

    public Collider PlayerSensorArea;

    public Collider CurrentSensorArea;

    private int shootedCount;

    private bool isDying;

    /// <summary>
    /// Transfor do alvo, no caso o player
    /// </summary>
    private GameObject target;

    /// <summary>
    /// Velocidade máxima do inimigo
    /// </summary>
    private float MaxVelocity = 2f;

    /// <summary>
    /// SQR máximo do inimigo
    /// </summary>
    private float sqrMaxVelocity;

    /// <summary>
    /// Posição inicial do inimigo
    /// </summary>
    private int startXPosition;

    /// <summary>
    /// Controle para verificar se o inimigo está andando
    /// </summary>
    private bool isEnemieMoving = false;

    /// <summary>
    /// Controle para verificar se o inimigo está atacando
    /// </summary>
    private bool isEnemieAttacking = false;

    /// <summary>
    /// Componente de animação do inimigo
    /// </summary>
    private Animator animatorComponent;

    /// <summary>
    /// Componente de física do inimigo
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
        if (this.isDying)
        {
            this.rigidBodyComponent.velocity = Vector3.zero;
            return;
        }
        
        this.animatorComponent.SetBool("IsEnemieMoving", this.isEnemieMoving);

        if (!this.isEnemieAttacking)
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
                        if (this.Zaxis)
                        {
                            this.transform.Translate(new Vector3(0, 0, -1.5f * Time.deltaTime));
                            this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, -1 * Mathf.Abs(this.transform.localScale.z));
                        }
                        else
                        {
                            this.transform.Translate(new Vector3(-1.5f * Time.deltaTime, 0));
                            this.transform.localScale = new Vector3(Mathf.Abs(this.transform.localScale.x), this.transform.localScale.y, this.transform.localScale.z);
                        }
                    }
                    else
                    {
                        if (this.Zaxis)
                        {
                            this.transform.Translate(new Vector3(0, 0, 1.5f * Time.deltaTime));
                            this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, Mathf.Abs(this.transform.localScale.z));
                        }
                        else
                        {
                            this.transform.Translate(new Vector3(1.5f * Time.deltaTime, 0));
                            this.transform.localScale = new Vector3(-1 * Mathf.Abs(this.transform.localScale.x), this.transform.localScale.y, this.transform.localScale.z);
                        }
                    }
                }

                this.isEnemieMoving = true;
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
                            if (this.Zaxis)
                            {
                                this.transform.Translate(new Vector3(0, 0, -1.5f * Time.deltaTime));
                                this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, -1 * Mathf.Abs(this.transform.localScale.z));
                            }
                            else
                            {
                                this.transform.Translate(new Vector3(-1.5f * Time.deltaTime, 0));
                                this.transform.localScale = new Vector3(Mathf.Abs(this.transform.localScale.x), this.transform.localScale.y, this.transform.localScale.z);
                            }
                        }
                        else
                        {
                            if (this.Zaxis)
                            {
                                this.transform.Translate(new Vector3(0, 0, 1.5f * Time.deltaTime));
                                this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, Mathf.Abs(this.transform.localScale.z));
                            }
                            else
                            {
                                this.transform.Translate(new Vector3(1.5f * Time.deltaTime, 0));
                                this.transform.localScale = new Vector3(-1 * Mathf.Abs(this.transform.localScale.x), this.transform.localScale.y, this.transform.localScale.z);
                            }
                        }
                    }

                    this.isEnemieMoving = true;
                }
                else
                {
                    this.isEnemieMoving = false;
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
        if (!isEnemieAttacking)
        {
            this.isEnemieMoving = false;
            this.isEnemieAttacking = true;
            this.animatorComponent.SetTrigger("Attack");
            this.rigidBodyComponent.velocity = Vector3.zero;
        }
    }

    void FinishedAttack()
    {
        float distance = 0.4f;
        RaycastHit hit = new RaycastHit();

        Vector3 direction;
        if (this.Zaxis)
        {
            distance = 0.7f;
            direction = this.transform.localScale.z > 0 ? Vector3.right : Vector3.left;
        }
        else
            direction = this.transform.localScale.x > 0 ? Vector3.left : Vector3.right;

        Vector3 position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);

        Debug.DrawRay(position, direction * distance, Color.yellow);

        if (Physics.Raycast(position, direction, out hit, distance))
        {
            if (hit.collider.gameObject.tag == "Player")
                hit.collider.gameObject.GetComponent<PlayerController>().StartDie();
        }

        this.isEnemieAttacking = false;
    }

    void DieFinished()
    {
        Destroy(this.gameObject);
    }

    public void Shooted()
    {
        this.shootedCount += 1;

        if (this.shootedCount >= this.ShootsToDie)
        {
            CapsuleCollider collider = this.GetComponent<CapsuleCollider>();

            collider.center = new Vector3(0, 0, 0);
            collider.radius = 0.05f;
            collider.height = 0.25f;
            collider.direction = 0;

            this.isDying = true;
            this.animatorComponent.SetTrigger("Die");
        }
    }
}
