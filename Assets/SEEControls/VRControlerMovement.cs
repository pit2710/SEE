﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class VRControlerMovement : MonoBehaviour
{
    public GameObject Rig;
    public GameObject Controler;

    private bool leftTriggerLastFrame = false;
    private bool leftTouchButtonPressed = false;

    [SerializeField]
    [FormerlySerializedAs("OnLeftTriggerPulled")]
    private UnityEvent _OnLeftTrigger = new UnityEvent();

    [SerializeField]
    [FormerlySerializedAs("OnLeftTouchButtonPressed")]
    private UnityEvent _OnLeftTouchButtonPressed = new UnityEvent();

    [SerializeField]
    [FormerlySerializedAs("OnLeftTouchButtonReleased")]
    private UnityEvent _OnLeftTouchButtonReleased = new UnityEvent();

    void Start()
    {
        
    }

    void Update()
    {
        float movementAxis = Input.GetAxis("RightVRTriggerMovement");
        Rig.transform.Translate(Controler.transform.forward * movementAxis);

        float leftTriggerAxis = Input.GetAxis("LeftVRTrigger");
        if(leftTriggerAxis >0.5f && !leftTriggerLastFrame)
        {
            _OnLeftTrigger.Invoke();
            leftTriggerLastFrame = true;
        }
        else if(leftTriggerAxis < 0.5 && leftTriggerLastFrame)
        {
            leftTriggerLastFrame = false;
        }

        if (!leftTouchButtonPressed && Input.GetButton("LeftVRTouchButton"))
        {
            Debug.Log("open menu");
            leftTouchButtonPressed = true;
            _OnLeftTouchButtonPressed.Invoke();
        }
        else if(leftTouchButtonPressed && Input.GetButton("LeftVRTouchButton"))
        {
            leftTouchButtonPressed = false;
            _OnLeftTouchButtonReleased.Invoke();
        }
    }
}
