using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : ResettableComponent
{
    public static PlayerController Instance;

    [Header("Properties:")]
    public float controllesCooldownInMS = 200;
    public float movementAmount = 0.5f;
    public bool runTouchControlsOnEditor = false;
    [Header("References:")]
    public Transform controller;
    public Transform spawnPoint;
    public SpriteRenderer decal;
    public ParticleSystem pieceParticleSystem;
    public CameraController cameraController;
    public AudioSource horizontalAudioSource;
    public AudioSource rotationAudioSource;
    public Tutorial tutorial;
    public SwipeDetector swipeDetector;

    private bool inCooldown;
    private float horizontalAxis;
    private float accelerationChargeCount = 0f;
    private float acceleration = 1f;
    private TouchPhase currentTouchPhase;
    
    public Piece CurrentPiece { get; private set; }

    #region <--- MONOBEHAVIOURS --->

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log(Instance.gameObject.name);
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        swipeDetector.OnSwipe += CheckFingerSwipe;
        swipeDetector.OnSimpleTouch += () => {
            if ((Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.WindowsEditor) && !runTouchControlsOnEditor) return;

            if (horizontalAxis != 0 || acceleration > 1) return;
            ProcessPieceRotationControll();
        };
        swipeDetector.OnTouchPhase += SetCurrentTouchPhase;
    }

    void Update()
    {
        if (GameManager.State == GameState.Paused) return;

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            CheckHorizontalKeyControlls();
            CheckPieceRotationKeyControll();
            CheckVerticalKeyControlls();
        }

        if (horizontalAxis > 0.01f || horizontalAxis < -0.01f)
            ProcessMovement(horizontalAxis > 0 ? 1 : -1);

        if (GameManager.State != GameState.Playing) return;

        if (CurrentPiece != null && !cameraController.IsMoving)
            CurrentPiece.UpdateFallMovement(acceleration);
    }

    #endregion <--- MONOBEHAVIOURS --->

    #region <--- MOVEMENTS METHODS --->

    private void SetCurrentTouchPhase(TouchPhase phase)
    {
        if ((Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.WindowsEditor) && !runTouchControlsOnEditor) return;

        currentTouchPhase = phase;
        if (currentTouchPhase == TouchPhase.Ended)
            OnFingerTouchEnded();
    }

    private void OnFingerTouchEnded()
    {
        if ((Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.WindowsEditor) && !runTouchControlsOnEditor) return;

        if (horizontalAxis != 0)
        {
            horizontalAxis = 0;
            ProcessHorizontalControllsUp();
        }

        if (acceleration > 1)
        {
            acceleration = 1f;
            accelerationChargeCount = 0f;
        }
    }

    private void CheckFingerSwipe(SwipeData swipeData)
    {
        if ((Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.WindowsEditor) && !runTouchControlsOnEditor) return;

        if (swipeData.direction == SwipeDirection.Down)
        {
            switch (currentTouchPhase)
            {
                case TouchPhase.Began:
                    ProcessVerticalControllDown();
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    ProcessVerticalControllHold();
                    break;

                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    ProcessVerticalControllUp();
                    break;
            }

            return;
        }

        if (swipeData.direction == SwipeDirection.Left || swipeData.direction == SwipeDirection.Right)
        {
            switch (currentTouchPhase)
            {
                case TouchPhase.Began:
                    horizontalAxis = swipeData.direction == SwipeDirection.Right ? 1 : -1;
                    ProcessHorizontalControllsDown();
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    horizontalAxis = swipeData.direction == SwipeDirection.Right ? 1 : -1;
                    ProcessHorizontalControllsHold();
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    horizontalAxis = 0;
                    ProcessHorizontalControllsUp();
                    break;
            }
        }
    }

    #region <--- VERTICAL METHODS --->

    private void CheckVerticalKeyControlls()
    {
        if (cameraController.IsMoving || Input.GetKeyDown("down") || Input.GetKeyDown(KeyCode.S) || Input.GetKeyUp("down") || Input.GetKeyUp(KeyCode.S))
        {
            ProcessVerticalControllDown();
        }
        else if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
        {
            ProcessVerticalControllHold();
        }
        else if (Input.GetKeyUp("down") || Input.GetKeyUp(KeyCode.S))
        {
            ProcessVerticalControllUp();
        }
    }

    private void ProcessVerticalControllDown()
    {
        if (GameManager.State == GameState.Tutorial)
            tutorial.CompleteTutorial(2);

        acceleration = 1f;
        accelerationChargeCount = 0f;
    }

    private void ProcessVerticalControllHold()
    {
        if (GameManager.State == GameState.Tutorial)
            tutorial.CompleteTutorial(2);

        accelerationChargeCount += Time.deltaTime;
        acceleration = Mathf.Lerp(1, GameManager.MaxAccelerationModifier, accelerationChargeCount / GameManager.MaxAccelerationDuration);
    }

    private void ProcessVerticalControllUp()
    {
        acceleration = 1f;
        accelerationChargeCount = 0f;
    }

    #endregion <---  VERTICAL METHODS --->

    #region <--- HORIZONTAL METHODS --->
    private void CheckHorizontalKeyControlls()
    {
        if (Input.GetKeyDown("left") || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown("right") || Input.GetKeyDown(KeyCode.D))
        {
            horizontalAxis = Input.GetAxis("Horizontal");
            ProcessHorizontalControllsDown();
        }
        else if (Input.GetKey("left") || Input.GetKey(KeyCode.A) || Input.GetKey("right") || Input.GetKey(KeyCode.D))
        {
            horizontalAxis = Input.GetAxis("Horizontal");
            ProcessHorizontalControllsHold();
        }
        else if (Input.GetKeyUp("left") || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp("right") || Input.GetKeyUp(KeyCode.D))
        {
            ProcessHorizontalControllsUp();
        }
    }

    private void ProcessHorizontalControllsDown()
    {
        if (GameManager.State == GameState.Tutorial)
            tutorial.CompleteTutorial(0);

        if (horizontalAxis > 0.1f || horizontalAxis < -0.1f)
            ProcessMovement(horizontalAxis > 0 ? 1 : -1);
    }

    private void ProcessHorizontalControllsHold()
    {
        if (GameManager.State == GameState.Tutorial)
            tutorial.CompleteTutorial(0);

        if (horizontalAxis > 0.1f || horizontalAxis < -0.1f)
            ProcessMovement(horizontalAxis > 0 ? 1 : -1);
    }

    private void ProcessHorizontalControllsUp()
    {
        horizontalAxis = 0;
        CancelInvoke();
        FinishCooldown();
    }

    #endregion<--- HORIZONTAL METHODS --->

    #region <--- ROTATION METHODS --->

    private void CheckPieceRotationKeyControll()
    {
        if (Input.GetKeyDown("space"))
            ProcessPieceRotationControll();
    }

    private void ProcessPieceRotationControll()
    {
        if (GameManager.State == GameState.Tutorial)
            tutorial.CompleteTutorial(1);

        if (GameManager.State != GameState.Playing || CurrentPiece == null) return;

        rotationAudioSource.Play();
        CurrentPiece.transform.rotation *= Quaternion.Euler(0, 0, 90);
        decal.size = new Vector2((CurrentPiece.transform.rotation.eulerAngles.z == 90 || CurrentPiece.transform.rotation.eulerAngles.z == 270) ? CurrentPiece.size.y : CurrentPiece.size.x, decal.size.y);
    }

    #endregion <--- ROTATION METHODS --->

    private void ProcessMovement(int direction)
    {
        if (inCooldown) return;

        if (controller == null)
        {
            controller = GameObject.FindGameObjectWithTag("Player").transform;
        }

        var curPos = controller.position;
        curPos.x += direction * movementAmount;
        controller.position = curPos;
        
        horizontalAudioSource.Play();

        StartCooldown();
    }

    private void StartCooldown()
    {
        inCooldown = true;
        Invoke("FinishCooldown", controllesCooldownInMS / 1000f);
    }

    private void FinishCooldown()
    {
        inCooldown = false;
    }

    #endregion <--- MOVEMENTS METHODS --->

    #region <--- PUBLIC METHODS --->

    public void GetNewPiece()
    {
        if (CurrentPiece != null || GameManager.State != GameState.Playing) return;

        acceleration = 1f;
        accelerationChargeCount = 0f;

        var curPos = controller.position;
        curPos.x = 0;
        controller.position = curPos;

        CurrentPiece = PieceSpawner.SpawnRandomPiece();
        CurrentPiece.SetAsPlayerPiece();
        decal.size = new Vector2((CurrentPiece.transform.rotation.eulerAngles.z == 90 || CurrentPiece.transform.rotation.eulerAngles.z == 270) ? CurrentPiece.size.y : CurrentPiece.size.x, decal.size.y);

        CurrentPiece.MyRigidBody.mass = 1;
        CurrentPiece.transform.SetParent(controller, false);
        CurrentPiece.transform.position = spawnPoint.position;

        CurrentPiece.onCollisionCallback.AddListener(ClearCurrentPiece);
        CurrentPiece.onCollisionCallback.AddListener(GetNewPiece);
    }

    public void GetNewPiece(Vector3 collisionPosition)
    {
        GetNewPiece();
    }

    public void ClearCurrentPiece()
    {
        if (CurrentPiece == null) return;

        cameraController.Shake();
        CurrentPiece.onCollisionCallback.RemoveAllListeners();
        CurrentPiece = null;
    }

    public void ClearCurrentPiece(Vector3 collisionPosition)
    {
        if (CurrentPiece == null) return;

        pieceParticleSystem.transform.position = collisionPosition;
        pieceParticleSystem.Play();
        PieceImpactAudio.PlayRandomImpactAudio();

        ClearCurrentPiece();
    }

    public void DespawnCurrentPiece()
    {
        if (CurrentPiece == null) return;
        CurrentPiece.onCollisionCallback.RemoveAllListeners();
        CurrentPiece.Despawn();
        CurrentPiece = null;
    }

    #endregion <--- PUBLIC METHDOS --->

    #region <--- RESET METHODS --->

    public override void ResetComponent()
    {
        acceleration = 1f;
        accelerationChargeCount = 0f;

        var curPos = controller.position;
        curPos.x = 0;
        controller.position = curPos;
    }

    #endregion <--- RESET METHODS --->
}
