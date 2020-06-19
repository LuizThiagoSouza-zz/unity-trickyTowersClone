using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    [SerializeField]private float minDistanceForSwipe = 20f;
    private Vector2 fingerDownPosition;
    private Vector2 fingerUpPosition;
    private TouchPhase currentTouchPhase;

    public event Action<SwipeData> OnSwipe = delegate { };
    public event Action OnSimpleTouch = delegate { };
    public event Action<TouchPhase> OnTouchPhase = delegate { };

    private void Update()
    {
        if (Input.touches.Length > 0)
        {
            foreach (var touch in Input.touches)
            {
                currentTouchPhase = touch.phase;

                if (touch.phase == TouchPhase.Began)
                {
                    fingerUpPosition = touch.position;
                    fingerDownPosition = touch.position;
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    fingerDownPosition = touch.position;
                    DetectSwipe();
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    fingerDownPosition = touch.position;
                    DetectSwipe();
                }
            }
        }
        else if (currentTouchPhase != TouchPhase.Ended)
            currentTouchPhase = TouchPhase.Ended;

        OnTouchPhase(currentTouchPhase);
    }

    private void DetectSwipe()
    {
        if (SwipeDistanceMet())
        {
            if (IsVerticalSwipe())
            {
                var dir = fingerDownPosition.y - fingerUpPosition.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                ProcessSwipe(dir);
            }
            else
            {
                var dir = fingerDownPosition.x - fingerUpPosition.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                ProcessSwipe(dir);
            }
            fingerUpPosition = fingerDownPosition;
        }
        else
        {
            OnSimpleTouch();
        }
    }

    private void ProcessSwipe(SwipeDirection dir)
    {
        var swipeData = new SwipeData()
        {
            direction = dir,
            startPosition = fingerDownPosition,
            endPosition = fingerUpPosition
        };
        OnSwipe(swipeData);
    }

    private bool IsVerticalSwipe()
    {
        return VerticalMovementDistance() > HorizontalMovementDistance();
    }

    private bool SwipeDistanceMet()
    {
        return VerticalMovementDistance() > minDistanceForSwipe || HorizontalMovementDistance() > minDistanceForSwipe;
    }

    private float VerticalMovementDistance()
    {
        return Mathf.Abs(fingerDownPosition.y - fingerUpPosition.y);
    }

    private float HorizontalMovementDistance()
    {
        return Mathf.Abs(fingerDownPosition.x - fingerUpPosition.x);
    }
}

public struct SwipeData
{
    public Vector2 startPosition;
    public Vector2 endPosition;
    public SwipeDirection direction;
}