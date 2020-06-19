using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TopologyMeter : ResettableComponent
{
    [Header("Properties:")]
    public LayerMask piecesLayer;
    [Header("References:")]
    public Transform LeftBound;
    public Transform RightBound;
    public Transform Ground;

    private int totalPassAmount;
    private Vector3 currentPass;

    private const float PASS_SIZE = 1f;

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
    }

    #endregion <--- MONOBEHAVIOURS --->

    #region  <--- PUBLIC METHODS --->

    public List<Vector3> CalculateTopology()
    {
        currentPass = RightBound.position;

        var topology = new List<Vector3>();

        for (int i = 0; i <= totalPassAmount; i++)
        {
            var ray = Physics2D.Raycast(currentPass, Vector2.down, 100, piecesLayer);
            if (ray.rigidbody != null)
            {
                Debug.DrawRay(ray.point, Vector3.up * 2, Color.green, 0.5f);

                var point = ray.point;
                point.y = Mathf.Floor(point.y);
                topology.Add(point);
            }

            currentPass.x += PASS_SIZE;
        }

        return topology;
    }

    #endregion  <--- PUBLIC METHODS --->

    public override void ResetComponent()
    {

    }
}