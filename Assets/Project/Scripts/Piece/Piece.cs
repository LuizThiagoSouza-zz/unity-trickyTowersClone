using UnityEngine;
using UnityEngine.Events;

public class Piece : PoolItem
{
    public Vector2 size;
    public OnCollisionEvent onCollisionCallback;
    public Sprite sprite;
    public Ai_PieceGuideline aiPieceGuideline;

    private Transform myTransform;

    public PieceState State { get; private set; }
    public Rigidbody2D MyRigidBody { get; private set; }
    public bool WasPlayerPiece { get; private set; }

    #region <--- MONOBEHAVIOURS --->
    void Awake()
    {
        myTransform = GetComponent<Transform>();
        MyRigidBody = GetComponent<Rigidbody2D>();

        MyRigidBody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (GameManager.State != GameState.Playing && MyRigidBody.isKinematic)
            MyRigidBody.isKinematic = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (State != PieceState.Controlling) return;

        if (collision.gameObject.layer == 9 || collision.gameObject.layer == 10)
        {
            gameObject.layer = 9;

            PieceSpawner.Instance.InGamePieces.Add(this);

            State = PieceState.Placed;

            MyRigidBody.constraints = RigidbodyConstraints2D.None;
            MyRigidBody.mass = 100;
            MyRigidBody.drag = 2;
            MyRigidBody.gravityScale = 1;
            MyRigidBody.velocity = Vector2.zero;
            MyRigidBody.angularDrag = 15;
            MyRigidBody.angularVelocity = 0;

            if (myTransform.parent != null)
                myTransform.parent = PieceSpawner.Instance.inGamePiecesContainer;

            if (onCollisionCallback != null)
                onCollisionCallback.Invoke(collision.GetContact(0).point);
        }
    }
    #endregion <--- MONOBEHAVIOURS --->

    #region <--- PUBLIC METHODS --->

    public void SetAsPlayerPiece()
    {
        WasPlayerPiece = true;
    }

    public void UpdateFallMovement(float acceleration)
    {
        if (GameManager.State == GameState.Playing && State == PieceState.Controlling)
            myTransform.Translate(Vector3.down * GameManager.FallingSpeed * acceleration * Time.deltaTime, Space.World);
    }

    #endregion <--- PUBLIC METHODS --->

    #region <--- POOL ITEM METHODS --->

    public override void OnSpawn()
    {
        State = PieceState.Controlling;
        gameObject.layer = 11;

        MyRigidBody.mass = 1;
        MyRigidBody.drag = 1;
        MyRigidBody.gravityScale = 0;
        MyRigidBody.velocity = Vector3.zero;
        MyRigidBody.angularDrag = 1;
        MyRigidBody.angularVelocity = 0;
        MyRigidBody.isKinematic = false;
    }

    public override void OnDespawn()
    {
        MyRigidBody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        myTransform.rotation = Quaternion.identity;
        WasPlayerPiece = false;

        if (PieceSpawner.Instance.InGamePieces.Contains(this))
            PieceSpawner.Instance.InGamePieces.Remove(this);

        onCollisionCallback.RemoveAllListeners();
    }

    #endregion <--- POOL ITEM METHODS --->
}

[System.Serializable]
public class OnCollisionEvent : UnityEvent<Vector3> { }