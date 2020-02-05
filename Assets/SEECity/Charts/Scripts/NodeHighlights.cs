﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SEECity.Charts.Scripts
{
    public class NodeHighlights : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private ChartManager _chartManager;

        private void Awake()
        {
            _chartManager = GameObject.FindGameObjectWithTag("ChartManager").GetComponent<ChartManager>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).gameObject.name.Equals(gameObject.name + "(Clone)"))
                    {
                        _chartManager.Accentuate(gameObject);
                        return;
                    }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.name.Equals(gameObject.name + "(Clone)"))
                {
                    _chartManager.Accentuate(gameObject);
                    return;
                }

            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _chartManager.HighlightObject(gameObject);
            StartCoroutine(Accentuate());
        }

        private IEnumerator Accentuate()
        {
            yield return new WaitForEndOfFrame();
            _chartManager.Accentuate(gameObject);

        }
    }
}