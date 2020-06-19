using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RadiusChecker : MonoBehaviour
{
    private List<Rigidbody> piecesAround;

    public Piece MyPiece { get; set; }

    private void Awake()
    {
        piecesAround = new List<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (MyPiece.State != PieceState.Controlling || other.gameObject.layer != 12) return;

        Debug.Log(other.name);

        var otherRb = other.GetComponent<Rigidbody>();
        if (otherRb == null) return;

        otherRb.isKinematic = true;
        piecesAround.Add(otherRb);
    }

    private void OnTriggerExit(Collider other)
    {
        if (MyPiece.State != PieceState.Controlling || other.gameObject.layer != 12) return;

        var otherRb = other.GetComponent<Rigidbody>();
        if (otherRb == null) return;

        otherRb.isKinematic = false;
        if (piecesAround.Contains(otherRb))
            piecesAround.Remove(otherRb);
    }

    public void Clear()
    {
        foreach (var rb in piecesAround)
            rb.isKinematic = false;

        piecesAround.Clear();
    }
}
