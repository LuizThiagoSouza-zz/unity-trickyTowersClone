using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : ResettableComponent
{
    [Header("Properties:")]
    public float heightOffset = 5f;
    public float verticalMovementAmount = 5f;
    public float verticalSpeed = 3f;
    public float shakeDuration = 1.5f;
    public float shakeForce = 5f;
    [Header("References:")]
    public Transform cameraParent;
    public Transform cameraAnchor;
    public HeightMeter heightMeter;

    private Vector3 targetPosition;
    private Vector3 initialPosition;
    private Vector3 velocity = Vector3.zero;
    private bool isShaking;
    private float currentDistance;

    public bool IsMoving { get; private set; }

    #region <--- MONOBEHAVIOURS --->

    void Awake()
    {
        targetPosition = cameraParent.position;
        initialPosition = cameraParent.position;
    }

    private void Start()
    {
        if (heightMeter == null)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (GameManager.State != GameState.Playing) return;

        currentDistance = Mathf.Abs(targetPosition.y - cameraParent.position.y);

        if (currentDistance == 0)
        {
            if (IsMoving) IsMoving = false;
            return;
        }

        if (!IsMoving) IsMoving = true;
        cameraParent.position = currentDistance >= 0.25f ?
                Vector3.SmoothDamp(cameraParent.position, targetPosition, ref velocity, Time.smoothDeltaTime * verticalSpeed) :
                targetPosition;
    }

    private void FixedUpdate()
    {
        if (GameManager.State != GameState.Playing) return;

        CalculateHeight();
    }

    #endregion <--- MONOBEHAVIOURS --->

    public void CalculateHeight()
    {
        if (cameraParent.position.y - heightMeter.Height <= heightOffset)
            targetPosition.y = heightMeter.Height + verticalMovementAmount;
        else if (cameraParent.position.y - heightMeter.Height >= heightMeter.Height + heightOffset)
            targetPosition.y = heightMeter.Height - verticalMovementAmount <= initialPosition.y ? initialPosition.y : heightMeter.Height - verticalMovementAmount;
    }

    #region <--- SHAKE METHODS --->

    public void Shake()
    {
        if (isShaking) return;

        StartCoroutine(ProcessShake());
    }

    private IEnumerator ProcessShake()
    {
        isShaking = true;

        var origin = cameraAnchor.localPosition;
        var t = 0f;
        var dir = 1;

        while (t <= shakeDuration)
        {
            cameraAnchor.localPosition = new Vector3(origin.x, origin.y + (Random.Range(0, 1f) * dir * shakeForce), origin.z);
            t += Time.deltaTime;
            dir *= -1;
            yield return null;
        }

        cameraAnchor.localPosition = origin;

        isShaking = false;
    }

    #endregion <--- SHAKE METHODS --->

    #region <--- RESET METHODS --->

    public override void ResetComponent()
    {
        ReturnToInitialPosition();
    }

    private void ReturnToInitialPosition()
    {
        cameraParent.position = initialPosition;

        targetPosition = initialPosition;
    }

    #endregion <--- RESET METHODS --->
}
