//Copyright 2022 Daniel Steinhauer

//Permission is hereby granted, free of charge, to any person obtaining a
//copy of this software and associated documentation files (the "Software"),
//to deal in the Software without restriction, including without limitation
//the rights to use, copy, modify, merge, publish, distribute, sublicense,
//and/or sell copies of the Software, and to permit persons to whom the Software
//is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
//USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FlyingDeveloperCaption : MonoBehaviour
{
    /// <summary>
    /// The offset from where to show the name label.
    /// </summary>
    public Vector3 offset;

    /// <summary>
    /// The text label which is supposed to display the developer's name.
    /// </summary>
    public TMP_Text textMeshPro;

    /// <summary>
    /// A reference to the parent parent flying saucer which contains the developer's name.
    /// </summary>
    protected FlyingDeveloper devReference;

    // Start is called before the first frame update
    void Start()
    {
        GameObject flyingdev_go = transform.parent.gameObject.transform.parent.gameObject;
        this.devReference = flyingdev_go.GetComponent<FlyingDeveloper> ();

        textMeshPro.text = devReference.AuthorName;
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(devReference.transform.position + offset);
        }
    }
}
