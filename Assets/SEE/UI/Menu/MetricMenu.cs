using SEE.GO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SEE.UI.Menu
{
    public class MetricMenu : MonoBehaviour
    {
        public GameObject menu;
        // Start is called before the first frame update
        void Start()
        {
            TextMeshProUGUI textEinf�gen = menu.transform.Find("MetricPanelREAL/Scrollview/Content/MetricRow").gameObject.MustGetComponent<TextMeshProUGUI>();
            textEinf�gen.text = "hello world";
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
