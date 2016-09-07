using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    #region Public Attributes

    public ArmorType CurrentArmorType;

    public GameObject ExplosionCollider;
    public GameObject ParticleCharging;
    public GameObject ParticleExplosion;

    public Material NormalArmorMaterial;
    public Material NormalHelmetMaterial;
    public Material ExplosiveArmorMaterial;
    public Material ExplosiveHelmetMaterial;

    #endregion

    #region Attributes

    /// <summary>
    /// Indica se a explosão foi carregada para disparar o collider com o impacto
    /// </summary>
    private bool isExplosionCharged;

    /// <summary>
    /// Tempo que a explosão está sendo carregada
    /// </summary>
    private float currentChargeTime = 0;

    /// <summary>
    /// Tamanho X do collider do personagem em idle, walk e hover
    /// </summary>
    private float PlayerColliderX = 0f;

    /// <summary>
    /// Tamanho X do collider do personagem em fly
    /// </summary>
    private float PlayerColliderXFlying = 0.3f;

    /// <summary>
    /// Tamanho X do collider do personagem em run
    /// </summary>
    private float PlayerColliderXRunning = 0.3f;

    /// <summary>
    /// Controle de tempo para alternar entre walk e run
    /// </summary>
    private float walkingTime = 0;

    /// <summary>
    /// Controle de tempo para alternar entre walk e run
    /// </summary>
    private float runningTime = 0;

    /// <summary>
    /// Indica se o jogador está no chão
    /// </summary>
    private bool isGrounded = true;

    /// <summary>
    /// Indica se o jogador está correndo
    /// </summary>
    private bool isRunning = false;

    /// <summary>
    /// Indica se o personagem está se movendo horizontalmente
    /// </summary>
    private bool horizontalMoving = false;

    /// <summary>
    /// Indica se o jetpack está sendo desativado
    /// </summary>
    private bool deactivatingJetPack = false;

    /// <summary>
    /// Tempo máximo que o personagem flutua antes de cair
    /// </summary>
    private float deactivateTimeJetPack = 1.0f;

    /// <summary>
    /// Tempo corrido em que o jetpack está ativo
    /// </summary>
    private float currentActiveTimeJetPack = 0f;

    /// <summary>
    /// Tempo corrido em que o jetpack está sendo desativado
    /// </summary>
    private float currentDeactivationTimeJetPack = 0f;
    
    /// <summary>
    /// Força aplicada ao ligar o jetpack
    /// </summary>
    private float jetPackForce = 15.0f;

    /// <summary>
    /// Velocidade aplicada ao mover o personagem horizontalmente
    /// </summary>
    private float horizontalVelocity = 1.2f;

    /// <summary>
    /// Ponto de divisão da tela para verificar o touch do lado esquerdo ou direito
    /// </summary>
    private float midScreen;

    /// <summary>
    /// Posição em que o touch do lado esquerdo que controla o movimento horizontal se iniciou
    /// </summary>
    private Vector2 movementStartTouch;

    /// <summary>
    /// Componente collider do personagem
    /// </summary>
    private CapsuleCollider colliderComponente;

    /// <summary>
    /// Componente de física do personagem
    /// </summary>
    private Rigidbody rigidBodyComponent;

    /// <summary>
    /// Componente de animação do modelo do personagem
    /// </summary>
    private Animator animatorComponent;

    /// <summary>
    /// Lista com os touches ativos para controlar o que deve ser realizado 
    /// do lado esquerdo da tela e o que deve ser realizado do lado direito da tela
    /// </summary>
    private List<TouchData> touches;

    #endregion

    string teste = string.Empty;

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), teste);
    }

    void Start()
    {
        //Define o meio da tela
        this.midScreen = Screen.width / 2.0f;

        //Inicia a lista vazia
        this.touches = new List<TouchData>();

        //Obtém os componentes do GameObject
        this.animatorComponent = this.GetComponent<Animator>();
        this.rigidBodyComponent = this.GetComponent<Rigidbody>();
        this.colliderComponente = this.GetComponent<CapsuleCollider>();

        this.SetArmor(this.CurrentArmorType);
    }

    void Update()
    {
        //Botão sair
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //TODO: Exibir o menu de pause
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        //Touches
        if (Input.touches.Length > 0)
        {
            //if (!EventSystem.current.IsPointerOverGameObject())
            //{
                foreach (Touch touch in Input.touches)
                {
                    #region Touch Began

                    if (touch.phase == TouchPhase.Began)
                    {
                        //Identifica o lado que o touch iniciou
                        TouchSide touchSide = touch.position.x < midScreen ? TouchSide.Left : TouchSide.Right;

                        //Define o ponto central de controle da movimentação horizontal
                        if (touchSide == TouchSide.Left)
                            this.movementStartTouch = touch.position;
                        else
                        {
                            if (this.CurrentArmorType == ArmorType.Normal)
                                this.ActiveJetPack();
                        }

                        //Guarda a posição inicial do touch
                        this.touches.Add(new TouchData(touch.fingerId, touch.position, Time.time, touchSide));
                    }

                    #endregion

                    #region Touch Moved & Stationary

                    else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        int index = this.touches.FindIndex(x => x.TouchId == touch.fingerId);

                        if (index == -1)
                            continue;

                        //Move o personagem para o lado em que o dedo está se movendo
                        if (touch.position.x < midScreen && this.touches[index].TouchSide == TouchSide.Left)
                        {
                            #region Lado esquerdo

                            //Somente permite o personagem se mover caso não esteja carregando o poder 
                            if (this.CurrentArmorType == ArmorType.Normal || (this.CurrentArmorType == ArmorType.Explosive && !this.ParticleCharging.activeInHierarchy && !this.ParticleExplosion.activeInHierarchy))
                            {
                                //Movimento horizontal
                                this.rigidBodyComponent.velocity = new Vector2((touch.position.x - this.movementStartTouch.x) * this.horizontalVelocity * Time.deltaTime, this.rigidBodyComponent.velocity.y);

                                //Verifica se o personagem está se movendo
                                if (Mathf.Abs(this.rigidBodyComponent.velocity.x) > 0.05f)
                                {
                                    this.horizontalMoving = true;
                                    this.animatorComponent.SetBool("IsOnHorizontalMovement", true);

                                    //Vira o personagem para o lado correto
                                    this.transform.localScale = new Vector3(touch.position.x > this.movementStartTouch.x ? 1 : -1, 1, 1);

                                    //Verifica se ele está no chão, caso esteja, executa o 
                                    //algoritmo para determinar se o personagem deve andar ou correr
                                    if (this.isGrounded)
                                    {
                                        if (Mathf.Abs(this.rigidBodyComponent.velocity.x) > 2.5f)
                                        {
                                            this.walkingTime = 0;
                                            this.runningTime += Time.deltaTime;
                                        }
                                        else
                                        {
                                            this.runningTime = 0;
                                            this.walkingTime += Time.deltaTime;
                                        }

                                        if (this.runningTime > 0.2f)
                                            this.isRunning = true;
                                        else if (this.walkingTime > 0.2f)
                                            this.isRunning = false;

                                        this.animatorComponent.SetBool("IsRunning", this.isRunning);
                                    }
                                    else
                                    {
                                        this.animatorComponent.SetBool("IsOnHorizontalMovement", true);
                                        this.SetPlayerCollider(PlayerColliderType.Flying);
                                    }
                                }
                                else
                                {
                                    this.walkingTime = 0;
                                    this.runningTime = 0;

                                    this.isRunning = false;
                                    this.horizontalMoving = false;
                                    this.animatorComponent.SetBool("IsRunning", false);
                                    this.animatorComponent.SetBool("IsOnHorizontalMovement", false);

                                    if (!this.isGrounded)
                                        this.SetPlayerCollider(PlayerColliderType.Normal);
                                }
                            }
                            else
                            {
                                this.walkingTime = 0;
                                this.runningTime = 0;

                                this.isRunning = false;
                                this.horizontalMoving = false;

                                this.animatorComponent.SetBool("IsRunning", false);
                                this.animatorComponent.SetBool("IsOnHorizontalMovement", false);

                                if (!this.isGrounded)
                                    this.SetPlayerCollider(PlayerColliderType.Normal);
                            }

                            #endregion
                        }
                        else if (this.touches[index].TouchSide == TouchSide.Right)
                        {
                            #region Lado direito

                            //Ativa ou mantém o jetpack ativo
                            if (this.CurrentArmorType == ArmorType.Normal)
                            {
                                this.ActiveJetPack();
                                this.currentChargeTime = 0;
                                this.ParticleCharging.SetActive(false);
                            }
                            else
                            {
                                this.currentChargeTime += Time.deltaTime;
                                this.ParticleCharging.SetActive(true);

                                if (this.currentChargeTime >= 2.0f)
                                {
                                    //Limpa o contador de tempo
                                    this.currentChargeTime = 0;

                                    //Desativa as particulas de carregamento
                                    this.ParticleCharging.SetActive(false);

                                    //Desativa as particulas de explosão
                                    this.ParticleExplosion.SetActive(false);

                                    //Ativa as particulas de explosão
                                    this.ParticleExplosion.SetActive(true);

                                    //Ativa o collider da explosão
                                    this.isExplosionCharged = true;
                                }
                            }

                            #endregion
                        }
                    }

                    #endregion

                    #region Touch Ended

                    else if (touch.phase == TouchPhase.Ended)
                    {
                        int index = this.touches.FindIndex(x => x.TouchId == touch.fingerId);

                        if (index == -1)
                            continue;

                        //Mantém o personagem parado horizontalmente
                        if (touch.position.x < midScreen && this.touches[index].TouchSide == TouchSide.Left)
                        {
                            #region Lado esquerdo

                            if (!this.isGrounded)
                                this.SetPlayerCollider(PlayerColliderType.Normal);

                            this.horizontalMoving = false;
                            this.animatorComponent.SetBool("IsOnHorizontalMovement", false);

                            #endregion
                        }
                        else if (this.touches[index].TouchSide == TouchSide.Right)
                        {
                            #region Lado direito

                            if (this.CurrentArmorType == ArmorType.Normal)
                            {
                                //Mantém o personagem flutuando por um tempo antes de mandá-lo de volta para o chão
                                this.StartDeactiveJetPack();
                            }
                            else
                            {
                                //Limpa o contador de tempo
                                this.currentChargeTime = 0;

                                //Desativa as particulas de carregamento
                                this.ParticleCharging.SetActive(false);

                                //Desativa as particulas de explosão
                                this.ParticleExplosion.SetActive(false);
                            }

                            #endregion
                        }

                        this.touches.RemoveAt(index);
                    }

                    #endregion
                }
            //}
        }
        else
        {
            this.horizontalMoving = false;
            this.animatorComponent.SetBool("IsOnHorizontalMovement", false);

            this.movementStartTouch = Vector2.zero;
            this.rigidBodyComponent.velocity = new Vector3(0, this.rigidBodyComponent.velocity.y);

            this.touches = new List<TouchData>();
        }
    }

    void FixedUpdate()
    {
        #region Jetpack

        if (this.currentDeactivationTimeJetPack > 0)
        {
            this.currentDeactivationTimeJetPack -= Time.deltaTime;
        }
        else
        {
            this.deactivatingJetPack = false;
            this.rigidBodyComponent.useGravity = true;
        }

        #endregion

        #region Explosion

        if(this.isExplosionCharged)
        {
            this.isExplosionCharged = false;
            this.ExplosionCollider.SetActive(true);
            this.ExplosionCollider.transform.localScale = new Vector3(7f, 0.1f, 7f);
        }
        else
        {
            this.ExplosionCollider.SetActive(false);
            this.ExplosionCollider.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }

        #endregion

        #region GroundCheck

        float distance = 0.05f;
        Vector3 direction = Vector3.down;
        RaycastHit hit = new RaycastHit();
        Vector3 position = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);

        Debug.DrawRay(position, direction * distance, Color.yellow);

        if (Physics.Raycast(position, direction, out hit, distance))
        {
            if (hit.collider.gameObject.CompareTag("Ground"))
            {
                this.isGrounded = true;
                this.animatorComponent.SetBool("IsGrounded", true);

                if (this.horizontalMoving)
                {
                    if (this.isRunning)
                    {
                        //TODO: Running
                        this.SetPlayerCollider(PlayerColliderType.Running);
                    }
                    else
                    {
                        this.animatorComponent.SetBool("IsOnHorizontalMovement", true);
                        this.SetPlayerCollider(PlayerColliderType.Normal);
                    }
                }
                else
                {
                    this.animatorComponent.SetBool("IsOnHorizontalMovement", false);
                    this.SetPlayerCollider(PlayerColliderType.Normal);
                }
            }
        }
        else
        {
            this.animatorComponent.SetBool("IsGrounded", false);

            if (this.horizontalMoving)
            {
                this.animatorComponent.SetBool("IsOnHorizontalMovement", true);
                this.SetPlayerCollider(PlayerColliderType.Flying);
            }
            else
            {
                this.animatorComponent.SetBool("IsOnHorizontalMovement", false);
                this.SetPlayerCollider(PlayerColliderType.Normal);
            }
        }

        #endregion
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "SpaceMan")
            GameObject.Find("Level").GetComponent<LevelController>().PlayerWon = true;
    }

    #region Jetpack control

    private void ActiveJetPack()
    {
        this.animatorComponent.SetBool("IsGrounded", false);
        this.animatorComponent.SetBool("IsOnHorizontalMovement", this.horizontalMoving);
        
        this.isGrounded = false;

        if (this.horizontalMoving)
            this.SetPlayerCollider(PlayerColliderType.Flying);
        else
            this.SetPlayerCollider(PlayerColliderType.Normal);

        this.CancelDeactive();
        this.currentActiveTimeJetPack += Time.deltaTime;
        this.rigidBodyComponent.AddForce(new Vector3(0, this.jetPackForce, 0));
    }

    private void CancelDeactive()
    {
        this.currentDeactivationTimeJetPack = 0;
        this.rigidBodyComponent.useGravity = true;
    }

    private void StartDeactiveJetPack()
    {
        if (this.currentActiveTimeJetPack > 1.0f)
        {
            if (!this.deactivatingJetPack)
            {
                this.deactivatingJetPack = true;
                this.currentDeactivationTimeJetPack = this.deactivateTimeJetPack;

                this.rigidBodyComponent.useGravity = false;
                this.rigidBodyComponent.velocity = new Vector3(this.rigidBodyComponent.velocity.x, 0, this.rigidBodyComponent.velocity.z);
            }
        }

        this.currentActiveTimeJetPack = 0f;
    }

    private void SetArmor(ArmorType armorType)
    {
        this.CurrentArmorType = armorType;

        Renderer[] renderes = GetComponentsInChildren<Renderer>();
        this.animatorComponent.SetBool("NormalArmor", armorType == ArmorType.Normal);

        foreach (Renderer renderer in renderes)
        {
            if (renderer.gameObject.name.ToLower().StartsWith("space_helmet"))
                renderer.material = armorType == ArmorType.Normal ? this.NormalHelmetMaterial : this.ExplosiveHelmetMaterial;
            else if(renderer.gameObject.name.ToLower().Contains("space"))
                renderer.material = armorType == ArmorType.Normal ? this.NormalArmorMaterial : this.ExplosiveArmorMaterial;
        }
    }

    private void SetPlayerCollider(PlayerColliderType colliderType)
    {
        switch (colliderType)
        {
            case PlayerColliderType.Normal:
                this.colliderComponente.center = new Vector3(this.PlayerColliderX, 0.95f, 0f);
                break;
            case PlayerColliderType.Running:
                this.colliderComponente.center = new Vector3(this.PlayerColliderXRunning, 0.95f, 0f);
                break;
            case PlayerColliderType.Flying:
                this.colliderComponente.center = new Vector3(this.PlayerColliderXFlying, 0.95f, 0f);
                break;
            default:
                break;
        }
    }

    #endregion

    public void ChangeArmor()
    {
        if (this.CurrentArmorType == ArmorType.Normal)
            this.SetArmor(ArmorType.Explosive);
        else
            this.SetArmor(ArmorType.Normal);
    }
}

#region Enum & Class

public enum ArmorType
{
    Normal,
    Explosive
}

public enum TouchSide
{
    Left,
    Right
}

public enum PlayerColliderType
{
    Normal,
    Running,
    Flying
}

public struct TouchData
{
    public int TouchId;
    public float EndTime;
    public float StartTime;
    public TouchSide TouchSide;
    public Vector2 StartPosition;
    public Vector2 EndPosition;

    public TouchData(int touchId, Vector2 startPosition, float startTime, TouchSide touchSide)
    {
        this.EndTime = 0;
        this.TouchId = touchId;
        this.StartPosition = startPosition;
        this.StartTime = startTime;
        this.EndPosition = Vector2.zero;
        this.TouchSide = touchSide;
    }
}

#endregion