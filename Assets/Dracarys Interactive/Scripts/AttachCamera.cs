using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachCamera : MonoBehaviour
{
    public Canvas canvas;

    private void Awake()
    {
        if (canvas)
        {
            canvas.worldCamera = Camera.main;
            Debug.Log("Attached main camera to canvas");
        }
    }

}
