﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SEE.Controls
{
    [CreateAssetMenu(fileName = "New KeyActionMapping", menuName = "Controls/KeyActionMapping", order = 1)]
    public class KeyActionMapping : ActionMapping
    {
        [SerializeField]
        public ButtonEvent OnWKey;

        [SerializeField]
        public ButtonEvent OnAKey;

        [SerializeField]
        public ButtonEvent OnSKey;

        [SerializeField]
        public ButtonEvent OnDKey;

        public override void CheckInput()
        {
            if(Input.GetKey("W"))
            {
                OnWKey.Invoke();
            }
        }

        public override string GetTypeAsString()
        {
            return "Key Mapping";
        }
    }
}
