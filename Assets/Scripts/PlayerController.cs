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

    public Button FlyingButton;
    public Button WeaponButton;
    public Button LightningButton;
    
    public bool isGrounded = true;

    #endregion

    #region Sprite das setas de movimentação

    private Sprite leftArrowSprite;
    private Sprite leftArrowActiveSprite;
    private Sprite rightArrowSprite;
    private Sprite rightArrowActiveSprite;

    #endregion

    #region Contadores

    private int diamondCount;

    private int lightningCount;

    private float elapsedShootTime;

    #endregion

    #region Status

    private bool isDying;

    private bool hasGunPower;

    private bool horizontalMoving;

    private bool hasLightningPower;

    private bool isExplosionCharged;

    private bool deactivatingJetPack;

    private float lightningCountdownTimer;

    private float currentActiveTimeJetPack;

    private float currentExplosionChargeTime;

    private float currentDeactivationTimeJetPack;

    #endregion

    #region Parâmetros

    private float shootSpeed = 500f;

    private float jetPackForce = 15.0f;

    private float timeBetweenShots = 0.5f;

    private float horizontalVelocity = 210f;

    private float deactivateTimeJetPack = 1.0f;

    private float PlayerColliderX = 0f;

    private float PlayerColliderXFlying = 0.3f;

    private PreferencesController preferences;

    #endregion

    #region GameObjects

    private Animator animatorComponent;

    private Rigidbody rigidBodyComponent;

    private GameObject shootSpawnPosition;

    private CapsuleCollider colliderComponente;

    #endregion

    private float midScreen;

    private List<TouchData> touches;

    private Vector2 movementStartTouch;

    void Start()
    {
        //Preferências
        this.preferences = GameObject.Find("PreferencesController").GetComponent<PreferencesController>();

        //Define o meio da tela
        this.midScreen = Screen.width / 2.0f;

        //Inicia a lista de toques vazia
        this.touches = new List<TouchData>();

        //Obtém os componentes do GameObject
        this.animatorComponent = this.GetComponent<Animator>();
        this.rigidBodyComponent = this.GetComponent<Rigidbody>();
        this.colliderComponente = this.GetComponent<CapsuleCollider>();

        //Obtém a posição de onde as descargas sairão
        this.shootSpawnPosition = GameObject.Find("ShootSpawnPosition");

        //Obtém os sprites dos indicadores de movimento horizontal
        this.leftArrowSprite = Resources.Load<Sprite>("left-arrow");
        this.leftArrowActiveSprite = Resources.Load<Sprite>("left-arrow-active");
        this.rightArrowSprite = Resources.Load<Sprite>("right-arrow");
        this.rightArrowActiveSprite = Resources.Load<Sprite>("right-arrow-active");

        //Ajusta a armadura inicial
        this.SetArmor(this.CurrentArmorType);
    }

    void Update()
    {
        if (this.hasLightningPower)
        {
            this.lightningCountdownTimer += Time.deltaTime;

            if (this.lightningCountdownTimer >= 60f)
            {
                this.lightningCount += 1;
                this.lightningCountdownTimer = 0;
            }
        }
        else
        {
            this.lightningCountdownTimer = 0;
        }

        //Botão sair
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("000_LevelMenu");

        if (Input.GetKey(KeyCode.Space))
            this.ActiveJetPack();

        //Touches
        if (Input.touches.Length > 0 && !this.isDying)
        {
            foreach (Touch touch in Input.touches)
            {
                #region Touch Began

                if (touch.phase == TouchPhase.Began)
                {
                    //Define o ponto central de controle da movimentação horizontal
                    if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        //Identifica o lado que o touch iniciou
                        TouchSide touchSide = touch.position.x < midScreen ? TouchSide.Left : TouchSide.Right;

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

                                if (!this.AnalogJoystick.activeInHierarchy)
                                    this.AnalogJoystick.SetActive(true);

                                this.AnalogJoystick.transform.position = this.movementStartTouch;

                                this.horizontalMoving = true;
                                this.animatorComponent.SetBool("IsOnHorizontalMovement", true);

                                //Direita
                                if (touch.position.x > this.movementStartTouch.x)
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
                                this.rigidBodyComponent.velocity = new Vector3(0, this.rigidBodyComponent.velocity.y);

                                if (!this.isGrounded)
                                    this.SetPlayerCollider(PlayerColliderType.Normal);
                            }
                        }
                        else
                        {
                            this.horizontalMoving = false;
                            this.AnalogJoystick.SetActive(false);
                            this.rigidBodyComponent.velocity = new Vector3(0, this.rigidBodyComponent.velocity.y);
                            this.animatorComponent.SetBool("IsOnHorizontalMovement", false);

                            if (!this.isGrounded)
                                this.SetPlayerCollider(PlayerColliderType.Normal);
                        }

                        #endregion
                    }
                    else if (this.touches[index].TouchSide == TouchSide.Right && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        #region Lado direito

                        //Ativa ou mantém o jetpack ativo
                        if (this.CurrentArmorType == ArmorType.FlyingArmor)
                        {
                            this.ActiveJetPack();
                            this.currentExplosionChargeTime = 0;
                            this.ParticleCharging.SetActive(false);
                        }
                        else if (this.CurrentArmorType == ArmorType.LightningArmor)
                        {
                            //Somente ativa o lightning caso o jogador esteja no chão e parado
                            if (this.isGrounded && !this.horizontalMoving && this.lightningCount > 0)
                            {
                                this.currentExplosionChargeTime += Time.deltaTime;
                                this.ParticleCharging.SetActive(true);

                                if (this.currentExplosionChargeTime >= 1.5f)
                                {
                                    //Limpa o contador de tempo
                                    this.currentExplosionChargeTime = 0;

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
                                this.currentExplosionChargeTime = 0;
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
                        this.rigidBodyComponent.velocity = new Vector3(0, this.rigidBodyComponent.velocity.y);

                        #endregion
                    }
                    else if (this.touches[index].TouchSide == TouchSide.Right && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        #region Lado direito

                        if (this.CurrentArmorType == ArmorType.FlyingArmor)
                        {
                            //Mantém o personagem flutuando por um tempo antes de mandá-lo de volta para o chão
                            this.StartDeactiveJetPack();
                        }
                        else if (this.CurrentArmorType == ArmorType.LightningArmor)
                        {
                            //Limpa o contador de tempo
                            this.currentExplosionChargeTime = 0;

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

        GameObject.Find("DiamondCount").GetComponent<Text>().text = this.diamondCount.ToString() + "/5";

        if (this.lightningCount == 0)
            GameObject.Find("LightningCount").GetComponent<Text>().text = string.Empty;
        else
            GameObject.Find("LightningCount").GetComponent<Text>().text = this.lightningCount.ToString() + "x";
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

            if (this.lightningCount <= 0)
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

        float distance = 0.1f;
        Vector3 direction = Vector3.down;
        RaycastHit hit1 = new RaycastHit();
        RaycastHit hit2 = new RaycastHit();

        Vector3 position1 = new Vector3(transform.position.x + 0.25f, transform.position.y + 0.1f, transform.position.z);
        Vector3 position2 = new Vector3(transform.position.x - 0.25f, transform.position.y + 0.1f, transform.position.z);

        Debug.DrawRay(position1, direction * distance, Color.yellow);
        Debug.DrawRay(position2, direction * distance, Color.yellow);

        bool rayResult1 = Physics.Raycast(position1, direction, out hit1, distance);
        bool rayResult2 = Physics.Raycast(position2, direction, out hit2, distance);

        if (rayResult1 || rayResult2)
        {
            RaycastHit currentHit = rayResult1 ? hit1 : hit2;

            if (currentHit.collider.gameObject.CompareTag("Ground"))
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
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");

                foreach (GameObject enemie in enemies)
                    if (enemie.GetComponent<EnemieController>() != null)
                        enemie.GetComponent<EnemieController>().PlayerSensorArea = other;
                break;
            case "SpaceMan":

                //Completou a fase
                int stars = 1;

                //Pegou todos os diamantes
                if (this.diamondCount == 5)
                    stars += 1;

                //Terminou a fase em menos de dois minutos
                if (GameObject.Find("Level").GetComponent<LevelController>().CountdownTimer >= 150)
                    stars += 1;

                this.preferences.Level001Stars = System.Math.Max(this.preferences.Level001Stars, stars);
                GameObject.Find("Level").GetComponent<LevelController>().PlayerWon = true;

                break;
            case "Lava":
                this.StartDie();
                break;
            case "LightningPower":
                this.lightningCount += 2;
                this.hasLightningPower = true; 
                Destroy(other.gameObject);
                break;
            case "GunPower":
                this.hasGunPower = true;
                Destroy(other.gameObject);
                GameObject.Find("Level").GetComponent<LevelController>().ShowWeaponTutorial = true;
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
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");

                foreach (GameObject enemie in enemies)
                    if (enemie.GetComponent<EnemieController>() != null)
                        enemie.GetComponent<EnemieController>().PlayerSensorArea = null;
                break;

            default:
                break;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "LavaWall")
            GameObject.Find("Level").GetComponent<LevelController>().ShowLavaWallTutorial = true;
    }

    #region Jetpack control

    private void ActiveJetPack()
    {
        this.ParticleSetArmor.SetActive(false);
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

        switch (armorType)
        {
            case ArmorType.FlyingArmor:
                this.FlyingButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("flying-icon");
                this.WeaponButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("gun-icon-disabled");
                this.LightningButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("lightning-icon-disabled");
                break;
            case ArmorType.LightningArmor:
                this.FlyingButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("flying-icon-disabled");
                this.WeaponButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("gun-icon-disabled");
                this.LightningButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("lightning-icon");
                break;
            case ArmorType.ShottingArmor:
                this.FlyingButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("flying-icon-disabled");
                this.WeaponButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("gun-icon");
                this.LightningButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("lightning-icon-disabled");
                break;
            default:
                break;
        }

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
        if (Time.time >= this.elapsedShootTime)
        {
            this.elapsedShootTime = Time.time + this.timeBetweenShots;
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
        //Exibir mensagem de derrota
        SceneManager.LoadScene("000_LevelMenu");
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
