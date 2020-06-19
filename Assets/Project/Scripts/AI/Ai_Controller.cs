using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai_Controller : ResettableComponent
{
    public static Ai_Controller Instance;

    [Header("References:")]
    public Transform controller;
    public Transform spawnPoint;
    public SpriteRenderer decal;
    public ParticleSystem pieceParticleSystem;
    public CameraController cameraController;
    public Ai_PieceAnalyzer pieceAnalyzer;

    private bool inMovementCooldown, inRotationCooldown, run;
    private float controllerCooldownInMS;
    private float movementAmount;
    private float acceleration = 1f;
    private Orientation targetOrientation;
    private float targetHorizontalPos;
    private float initialX;
    private float accelerationChargeCount = 0f;

    public Piece CurrentPiece { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        Instance = this;

        initialX = controller.position.x;
    }

    void Start()
    {
        controllerCooldownInMS = PlayerController.Instance.controllesCooldownInMS;
        movementAmount = PlayerController.Instance.movementAmount;

        InvokeRepeating("Analyze", 0, 0.5f);
    }

    void Update()
    {
        if (!run || GameManager.State != GameState.Playing || CurrentPiece == null) return;

        ProcessHorizontalControlls();
        ProcessPieceRotationControll();

        if (CurrentPiece != null && !cameraController.IsMoving)
            CurrentPiece.UpdateFallMovement(acceleration);
    }

    public void SetDirective(float targetPos, Orientation orientation)
    {
        targetHorizontalPos = targetPos;
        targetOrientation = orientation;
        run = true;
    }

    #region <--- PIECE METHODS --->

    public void GetNewPiece()
    {
        if (CurrentPiece != null || GameManager.State != GameState.Playing) return;

        acceleration = 1f;

        var curPos = controller.position;
        curPos.x = initialX;
        controller.position = curPos;

        CurrentPiece = PieceSpawner.SpawnPieceForAI();
        decal.size = new Vector2((CurrentPiece.transform.rotation.eulerAngles.z == 90 || CurrentPiece.transform.rotation.eulerAngles.z == 270) ? CurrentPiece.size.y : CurrentPiece.size.x, decal.size.y);

        CurrentPiece.MyRigidBody.mass = 1;
        CurrentPiece.transform.SetParent(controller, false);
        CurrentPiece.transform.position = spawnPoint.position;

        CurrentPiece.onCollisionCallback.AddListener(ClearCurrentPiece);
        CurrentPiece.onCollisionCallback.AddListener(GetNewPiece);

        Analyze();
    }

    private void Analyze()
    {
        if (CurrentPiece == null || GameManager.State != GameState.Playing) return;

        var idealPlacement = pieceAnalyzer.Analyze(CurrentPiece);
        if (idealPlacement != null)
        {
            var dif =
                idealPlacement.orientation == Orientation.Top ||
                idealPlacement.orientation == Orientation.Down ?
                CurrentPiece.size.x * 0.5f : CurrentPiece.size.y * 0.5f;

            dif += idealPlacement.initialPosition.x - 0.5f;

            if (!IsEqualTo(targetHorizontalPos, dif) || targetOrientation != idealPlacement.orientation)
            {
                accelerationChargeCount = 0f;
                acceleration = 1f;
                SetDirective(dif, idealPlacement.orientation);
            }
            else
            {
                accelerationChargeCount += Time.deltaTime;
                acceleration = Mathf.Lerp(1, GameManager.MaxAccelerationModifier, accelerationChargeCount / GameManager.MaxAccelerationDuration);
            }
        }
    }

    private static bool IsEqualTo(float a, float b)
    {
        return Mathf.Abs(a - b) <= 0.1f;
    }

    public void GetNewPiece(Vector3 collisionPosition)
    {
        GetNewPiece();
    }

    public void ClearCurrentPiece()
    {
        if (CurrentPiece == null) return;

        CurrentPiece.onCollisionCallback.RemoveAllListeners();
        CurrentPiece = null;
    }

    public void ClearCurrentPiece(Vector3 collisionPosition)
    {
        if (CurrentPiece == null) return;

        pieceParticleSystem.transform.position = collisionPosition;
        pieceParticleSystem.Play();

        ClearCurrentPiece();
    }

    public void DespawnCurrentPiece()
    {
        if (CurrentPiece == null) return;
        CurrentPiece.onCollisionCallback.RemoveAllListeners();
        CurrentPiece.Despawn();
        CurrentPiece = null;
    }

    #endregion <--- PIECE METHODS --->

    #region <--- HORIZONTAL MOVEMENT METHODS --->

    private void ProcessHorizontalControlls()
    {
        if (controller.position.x != targetHorizontalPos)
            ProcessMovement(controller.position.x > targetHorizontalPos ? -1 : 1);
    }

    private void ProcessMovement(int direction)
    {
        if (inMovementCooldown) return;

        var curPos = controller.position;
        curPos.x += direction * movementAmount;
        controller.position = curPos;

        StartMovementCooldown();
    }

    private void StartMovementCooldown()
    {
        inMovementCooldown = true;
        Invoke("FinishMovementCooldown", controllerCooldownInMS / 1000f);
    }

    private void FinishMovementCooldown()
    {
        inMovementCooldown = false;
    }

    #endregion <--- HORIZONTAL MOVEMENT METHODS --->

    #region <--- ROTATION MOVEMENT METHODS --->

    private void ProcessPieceRotationControll()
    {
        if (inRotationCooldown || CurrentPiece == null || CurrentPiece.transform.rotation.z == (float)targetOrientation) return;

        CurrentPiece.transform.rotation = Quaternion.Euler(0, 0, (float)targetOrientation);
        decal.size = new Vector2((CurrentPiece.transform.rotation.eulerAngles.z == 90 || CurrentPiece.transform.rotation.eulerAngles.z == 270) ? CurrentPiece.size.y : CurrentPiece.size.x, decal.size.y);

        StartRotationCooldown();
    }

    private void StartRotationCooldown()
    {
        inRotationCooldown = true;
        Invoke("FinishRotationCooldown", controllerCooldownInMS / 100f);
    }

    private void FinishRotationCooldown()
    {
        inRotationCooldown = false;
    }

    #endregion <--- ROTATION MOVEMENT METHODS --->

    #region <--- RESET METHODS --->

    public override void ResetComponent()
    {
        acceleration = 1f;
        accelerationChargeCount = 0f;

        var curPos = controller.position;
        curPos.x = initialX;
        controller.position = curPos;
    }

    #endregion <--- RESET METHODS --->
}
