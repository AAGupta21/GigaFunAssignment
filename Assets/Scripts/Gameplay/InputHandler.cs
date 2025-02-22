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
    
    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}
