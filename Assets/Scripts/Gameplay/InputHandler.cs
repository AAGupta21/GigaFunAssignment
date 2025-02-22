using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector3 startPosition;
    public Vector3 targetPosition;
    
    public Action SelectionAction;
    public Action<bool> DropAction;
    
    private bool _isDragging;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
    }

    private void Update()
    {
        if (_isDragging)
        {
            transform.position = Input.mousePosition;
        }
    }
}
