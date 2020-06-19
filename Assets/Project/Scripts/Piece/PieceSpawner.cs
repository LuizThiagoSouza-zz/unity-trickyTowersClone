using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceSpawner : ResettableComponent
{
    public static PieceSpawner Instance;

    [Header("References:")]
    public GameObject piecePoolsContainer;
    public Transform inGamePiecesContainer;
    public Image nextPiecePreview;

    private List<Pool> piecePools;

    public List<Piece> InGamePieces { get; private set; }
    public Piece NextPiece { get; private set; }

    #region <--- MONOBEHAVIOURS --->

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);

        Instance = this;

        piecePools = new List<Pool>(piecePoolsContainer.GetComponentsInChildren<Pool>());
        InGamePieces = new List<Piece>();
    }

    #endregion

    #region <--- SPAWN PIECES METHODS --->

    public static Piece SpawnRandomPiece()
    {
        if (Instance.NextPiece == null)
        {
            Instance.GetNextRandomPiece();
            return Instance.piecePools[Random.Range(0, Instance.piecePools.Count)].GetItem() as Piece;
        }

        var pieceToReturn = Instance.NextPiece;
        pieceToReturn.SetActive(true);
        Instance.GetNextRandomPiece();
        return pieceToReturn;
    }

    public static Piece SpawnPieceForAI()
    {
        var pieceToReturn = Instance.piecePools[Random.Range(0, Instance.piecePools.Count)].GetItem() as Piece;
        pieceToReturn.SetActive(true);
        return pieceToReturn;
    }

    private void GetNextRandomPiece()
    {
        NextPiece = piecePools[Random.Range(0, Instance.piecePools.Count)].GetItem(false) as Piece;
        nextPiecePreview.sprite = NextPiece.sprite;
    }

    #endregion <--- SPAWN PIECES METHODS --->

    #region <--- PUBLIC METHODS --->

    public void FreezeAllPieces()
    {
        foreach (var piece in InGamePieces)
        {
            piece.MyRigidBody.isKinematic = true;
        }
    }

    #endregion <--- PUBLIC METHODS --->

    #region <--- RESET METHODS -->

    public override void ResetComponent()
    {
        ResetNextPiece();
        RemoveAllCurrentPiece();
    }

    private void ResetNextPiece()
    {
        NextPiece = null;
        nextPiecePreview.sprite = null;
    }

    private void RemoveAllCurrentPiece()
    {
        var toRemoveList = new List<Piece>(InGamePieces);
        foreach (var piece in toRemoveList)
        {
            piece.MyRigidBody.isKinematic = false;
            piece.Despawn();
        }

        InGamePieces.Clear();
    }    

    #endregion <--- RESET METHODS -->
}
