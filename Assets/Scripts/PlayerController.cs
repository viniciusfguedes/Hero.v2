using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    #region Public Attributes

    public ArmorType CurrentArmorType;

    public GameObject ExplosionCollider;
    public GameObject ParticleSetArmor;
    public GameObject ParticleCharging;
    public GameObject ParticleExplosion;

    public Material FlyingArmorMaterial;
    public Material FlyingHelmetMaterial;
    public Material LightningArmorMaterial;
    public Material LightningHelmetMaterial;
    public Material ShottingArmorMaterial;
    public Material ShottingHelmetMaterial;

    public GameObject LightningShoot;

    public Image LeftArrowImage;
    public Image RightArrowImage;

    public GameObject AnalogJoystick;
    public Button WeaponButton;
    public Button LightningButton;

    /// <summary>
    /// Indica se o jogador está no chão
    /// </summary>
    public bool isGrounded = true;

    #endregion

    #region Attributes

    private Sprite leftArrowSprite;
    private Sprite leftArrowActiveSprite;
    private Sprite rightArrowSprite;
    private Sprite rightArrowActiveSprite;

    private int lightningCount;

    private int diamondCount;

    private bool hasGunPower;
    
    private bool isDying;

    private float shootSpeed = 500f;

    private GameObject shootSpawnPosition;

    private float timestamp;

    private float timeBetweenShots = 0.5f;

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
    private float horizontalVelocity = 210f;

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
        GUI.Label(new Rect(10, 10, 100, 20), this.transform.position.ToString());
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

        this.shootSpawnPosition = GameObject.Find("ShootSpawnPosition");

        this.leftArrowSprite = Resources.Load<Sprite>("left-arrow");
        this.leftArrowActiveSprite = Resources.Load<Sprite>("left-arrow-active");
        this.rightArrowSprite = Resources.Load<Sprite>("right-arrow");
        this.rightArrowActiveSprite = Resources.Load<Sprite>("right-arrow-active");

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
        if (Input.touches.Length > 0 && !this.isDying)
        {
            foreach (Touch touch in Input.touches)
            {
                //Botão do HUD
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
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
                            if (this.CurrentArmorType == ArmorType.FlyingArmor)
                                this.ActiveJetPack();
                            else if (this.CurrentArmorType == ArmorType.ShottingArmor)
                                this.Shot();
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

                            //Somente permite o personagem se mover caso não esteja carregando a descarga elétrica
                            if (this.CurrentArmorType == ArmorType.FlyingArmor || this.CurrentArmorType == ArmorType.ShottingArmor || (this.CurrentArmorType == ArmorType.LightningArmor && !this.ParticleCharging.activeInHierarchy && !this.ParticleExplosion.activeInHierarchy))
                            {
                                int direction = 0;

                                //Movimento horizontal para a direita
                                if (touch.position.x > this.movementStartTouch.x)
                                    direction = 1;
                                //Movimento horizontal para a esquerda
                                else
                                    direction = -1;

                                this.rigidBodyComponent.velocity = new Vector2(direction * this.horizontalVelocity * Time.deltaTime, this.rigidBodyComponent.velocity.y);

                                //Verifica se o personagem está se movendo
                                if (Mathf.Abs(this.rigidBodyComponent.velocity.x) > 0.05f)
                                {
                                    //Vira o personagem para o lado correto
                                    this.transform.localScale = new Vector3(direction, 1, 1);

                                    //if (!this.AnalogJoystick.activeInHierarchy)
                                    //    this.AnalogJoystick.SetActive(true);

                                    //this.AnalogJoystick.transform.position = this.movementStartTouch;

                                    this.horizontalMoving = true;
                                    this.animatorComponent.SetBool("IsOnHorizontalMovement", true);

                                    //Direita
                                    if(touch.position.x > this.movementStartTouch.x)
                                    {
                                        this.LeftArrowImage.sprite = this.leftArrowSprite;
                                        this.RightArrowImage.sprite = this.rightArrowActiveSprite;
                                    }
                                    //Esquerda
                                    else
                                    {
                                        this.LeftArrowImage.sprite = this.leftArrowActiveSprite;
                                        this.RightArrowImage.sprite = this.rightArrowSprite;
                                    }

                                    if (!this.isGrounded)
                                    {
                                        this.animatorComponent.SetBool("IsOnHorizontalMovement", true);
                                        this.SetPlayerCollider(PlayerColliderType.Flying);
                                    }
                                }
                                else
                                {
                                    this.horizontalMoving = false;
                                    this.AnalogJoystick.SetActive(false);
                                    this.animatorComponent.SetBool("IsOnHorizontalMovement", false);

                                    if (!this.isGrounded)
                                        this.SetPlayerCollider(PlayerColliderType.Normal);
                                }
                            }
                            else
                            {
                                this.horizontalMoving = false;
                                this.AnalogJoystick.SetActive(false);
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
                            if (this.CurrentArmorType == ArmorType.FlyingArmor)
                            {
                                this.ActiveJetPack();
                                this.currentChargeTime = 0;
                                this.ParticleCharging.SetActive(false);
                            }
                            else if(this.CurrentArmorType == ArmorType.LightningArmor)
                            {
                                //Somente ativa o lightning caso o jogador esteja no chão e parado
                                if (this.isGrounded && !this.horizontalMoving && this.lightningCount > 0)
                                {
                                    this.currentChargeTime += Time.deltaTime;
                                    this.ParticleCharging.SetActive(true);

                                    if (this.currentChargeTime >= 1.5f)
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
                                else
                                {
                                    this.currentChargeTime = 0;
                                    this.ParticleCharging.SetActive(false);
                                }
                            }
                            else
                            {
                                this.Shot();
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
                            this.AnalogJoystick.SetActive(false);
                            this.animatorComponent.SetBool("IsOnHorizontalMovement", false);

                            #endregion
                        }
                        else if (this.touches[index].TouchSide == TouchSide.Right)
                        {
                            #region Lado direito

                            if (this.CurrentArmorType == ArmorType.FlyingArmor)
                            {
                                //Mantém o personagem flutuando por um tempo antes de mandá-lo de volta para o chão
                                this.StartDeactiveJetPack();
                            }
                            else if(this.CurrentArmorType == ArmorType.LightningArmor)
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
                else
                {
                    this.horizontalMoving = false;
                    this.AnalogJoystick.SetActive(false);
                    this.animatorComponent.SetBool("IsOnHorizontalMovement", false);
                    this.rigidBodyComponent.velocity = new Vector3(0, 0);
                }
            }

        }
        else
        {
            this.horizontalMoving = false;
            this.AnalogJoystick.SetActive(false);
            this.animatorComponent.SetBool("IsOnHorizontalMovement", false);

            this.movementStartTouch = Vector2.zero;
            this.rigidBodyComponent.velocity = new Vector3(0, this.rigidBodyComponent.velocity.y);

            this.touches = new List<TouchData>();
        }

        GameObject.Find("DiamondCount").GetComponent<Text>().text = diamondCount.ToString() + "/5";
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

        #region Lightining

        if (this.isExplosionCharged)
        {
            this.lightningCount -= 1;
            this.isExplosionCharged = false;
            this.ExplosionCollider.SetActive(true);
            this.ExplosionCollider.transform.localScale = new Vector3(7f, 0.1f, 7f);

            if(this.lightningCount <= 0)
            {
                this.SetArmor(ArmorType.FlyingArmor);
                Sprite lightningSprite = Resources.Load<Sprite>("lightning-icon-disabled");
                ((Image)LightningButton.GetComponent<Image>()).sprite = lightningSprite;
            }
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
                    this.animatorComponent.SetBool("IsOnHorizontalMovement", true);
                else
                    this.animatorComponent.SetBool("IsOnHorizontalMovement", false);

                this.SetPlayerCollider(PlayerColliderType.Normal);
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
        switch (other.tag)
        {
            case "SensorArea":
                GameObject[] repteis = GameObject.FindGameObjectsWithTag("Reptil");

                foreach (GameObject reptil in repteis)
                    if (reptil.GetComponent<ReptilController>() != null)
                        reptil.GetComponent<ReptilController>().PlayerSensorArea = other;
                break;
            case "SpaceMan":
                GameObject.Find("Level").GetComponent<LevelController>().PlayerWon = true;
                break;
            case "Lava":
                this.StartDie();
                break;
            case "LightningPower":

                this.lightningCount += 2;
                Sprite lightningSprite = Resources.Load<Sprite>("lightning-icon");
                ((Image)LightningButton.GetComponent<Image>()).sprite = lightningSprite;

                Destroy(other.gameObject);
                break;
            case "GunPower":

                this.hasGunPower = true;
                Sprite gunSprite = Resources.Load<Sprite>("gun-icon");
                ((Image)WeaponButton.GetComponent<Image>()).sprite = gunSprite;
                
                Destroy(other.gameObject);
                break;

            case "Diamond":
                this.diamondCount += 1;
                Destroy(other.gameObject);
                break;
            default:
                break;
        }            
    }

    void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case "SensorArea":
                GameObject[] repteis = GameObject.FindGameObjectsWithTag("Reptil");

                foreach (GameObject reptil in repteis)
                    if (reptil.GetComponent<ReptilController>() != null)
                        reptil.GetComponent<ReptilController>().PlayerSensorArea = null;
                break;

            default:
                break;
        }
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

        this.ParticleSetArmor.SetActive(false);
        this.ParticleSetArmor.SetActive(true);

        foreach (Renderer renderer in renderes)
        {
            if (renderer.gameObject.name.ToLower().StartsWith("space_helmet"))
                renderer.material = armorType == ArmorType.FlyingArmor ? this.FlyingHelmetMaterial : armorType == ArmorType.LightningArmor ? this.LightningHelmetMaterial : this.ShottingHelmetMaterial;
            else if (renderer.gameObject.name.ToLower().Contains("space"))
                renderer.material = armorType == ArmorType.FlyingArmor ? this.FlyingArmorMaterial : armorType == ArmorType.LightningArmor ? this.LightningArmorMaterial : this.ShottingArmorMaterial;
        }
    }

    private void SetPlayerCollider(PlayerColliderType colliderType)
    {
        switch (colliderType)
        {
            case PlayerColliderType.Normal:
                this.colliderComponente.center = new Vector3(this.PlayerColliderX, 0.95f, 0f);
                break;
            case PlayerColliderType.Flying:
                this.colliderComponente.center = new Vector3(this.PlayerColliderXFlying, 0.95f, 0f);
                break;
            default:
                break;
        }
    }

    #endregion

    void Shot()
    {
        if (Time.time >= this.timestamp)
        {
            this.timestamp = Time.time + this.timeBetweenShots;
            this.animatorComponent.SetTrigger("IsShotting");

            GameObject projectile = Instantiate(this.LightningShoot, this.shootSpawnPosition.transform.position, Quaternion.identity) as GameObject;

            if (this.transform.localScale.x < 0)
                projectile.GetComponent<Rigidbody>().AddForce(new Vector3(-1 * this.shootSpeed, 0, 0));
            else
                projectile.GetComponent<Rigidbody>().AddForce(new Vector3(1 * this.shootSpeed, 0, 0));
        }
    }

    void FinishedDie()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void SetColliderDeath()
    {
        colliderComponente.direction = 0;
    }

    public void StartDie()
    {
        this.isDying = true;
        this.rigidBodyComponent.useGravity = true;
        this.rigidBodyComponent.velocity = Vector3.zero;
        this.animatorComponent.SetTrigger("IsDying");
    }

    public void SetFlyingArmor()
    {
        if (this.isGrounded)
            this.SetArmor(ArmorType.FlyingArmor);
    }

    public void SetLightningArmor()
    {
        if (this.isGrounded && this.lightningCount > 0)
            this.SetArmor(ArmorType.LightningArmor);
    }

    public void SetWeaponArmor()
    {
        if (this.isGrounded && this.hasGunPower)
            this.SetArmor(ArmorType.ShottingArmor);
    }
}

#region Enum & Class

public enum ArmorType
{
    FlyingArmor,
    LightningArmor,
    ShottingArmor
}

public enum TouchSide
{
    Left,
    Right
}

public enum PlayerColliderType
{
    Normal,
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
