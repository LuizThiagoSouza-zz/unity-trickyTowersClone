using TMPro;
using UnityEngine;

public class PieceDespawner : MonoBehaviour
{
    [Header("References:")]
    public RectTransform skull;
    public Animator skullAnimator;
    public AudioSource loseLifeAudioSource;

    private Piece currentPiece;

    #region <--- MONOBEHAVIOURS --->

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 9 || other.gameObject.layer == 11)
        {
            var piece = other.GetComponent<Piece>();
            if (piece == null || currentPiece == piece) return;

            currentPiece = piece;
            if (piece.WasPlayerPiece)
            {
                if (GameManager.State != GameState.Playing) return;

                loseLifeAudioSource.Play();
                GameManager.AddLife(-1);
                ShowSkullIcon(piece.transform.position);

                if (PlayerController.Instance.CurrentPiece == piece)
                {
                    PlayerController.Instance.ClearCurrentPiece();
                    PlayerController.Instance.GetNewPiece();
                }
            }
            else if (Ai_Controller.Instance != null && piece == Ai_Controller.Instance.CurrentPiece)
            {
                Ai_Controller.Instance.ClearCurrentPiece();
                Ai_Controller.Instance.GetNewPiece();
            }

            DespawnPiece(piece);
        }
    }

    #endregion <--- MONOBEHAVIOURS --->

    #region <--- PRIVATE METHODS --->

    private void ShowSkullIcon(Vector3 despawnPoisiton)
    {
        var xPos = Camera.main.WorldToScreenPoint(despawnPoisiton).x;
        skull.position = new Vector3(xPos, 0);
        skullAnimator.Rebind();
        skullAnimator.SetTrigger("Show");
    }

    private static void DespawnPiece(Piece piece)
    {
        piece.Despawn();
    }

    #endregion <--- PRIVATE METHODS --->
}
