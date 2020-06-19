using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingTexture : MonoBehaviour
{
    public float speed;
    public bool x;
    public bool y;
    public RawImage image;

    private Rect rect;

    private void Start()
    {
        rect = image.uvRect;
    }

    void Update()
    {
        rect.x += x ? Time.deltaTime * speed : 0;
        rect.y += y ? Time.deltaTime * speed : 0;

        image.uvRect = rect;
    }
}
