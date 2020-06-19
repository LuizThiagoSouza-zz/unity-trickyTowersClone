using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HeightMeter : ResettableComponent
{
    [Header("Properties:")]
    public LayerMask piecesLayer;
    public bool isAI;
    [Header("References:")]
    public Transform LeftBound;
    public Transform RightBound;
    public Transform Ground;
    public TMP_Text heightLabel;

    private int heightInPass, totalPassAmount;
    private Vector3 currentPass;

    private const float PASS_SIZE = 0.25f;

    public int Height { get; private set; }

    #region  <--- MONOBEHAVIOURS --->

    private void Awake()
    {
        if (LeftBound == null || RightBound == null)
        {
            Debug.LogError("Bounds not found, disabling...");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        var distance = Vector3.Distance(LeftBound.position, RightBound.position);
        totalPassAmount = (int)(distance / PASS_SIZE);

        InvokeRepeating("CalculateHeight", 0, 0.5f);
    }

    #endregion <--- MONOBEHAVIOURS --->

    #region  <--- PRIVATE METHODS --->

    private void CalculateHeight()
    {
        if (GameManager.State != GameState.Playing) return;

        heightInPass = 0;
        currentPass = RightBound.position;

        for (int i = 0; i <= totalPassAmount; i++)
        {
            var ray = Physics2D.Raycast(currentPass, Vector2.down, 200, piecesLayer);
            if (ray.rigidbody != null)
            {
                int point = Mathf.Abs((int)(Ground.position.y - ray.point.y));
                heightInPass = point <= heightInPass ? heightInPass : point;
            } 

            currentPass.x += PASS_SIZE;
        }

        Height = heightInPass;
        heightLabel.SetText(Height + "m");

        if (Height >= GameManager.Instance.heightToWin)
        {
            if (isAI) GameManager.GameWin(); else GameManager.GameOver();
        }
    }

    #endregion  <--- PUBLIC METHODS --->

    public override void ResetComponent()
    {
        ResetHeight();
    }

    private void ResetHeight()
    {
        Height = 0;
        heightLabel.SetText(Height + "m");
    }
}