using System.Collections.Generic;
using SEE.GameObjects;
using SEE.GO;
using SEE.Utils;
using TMPro;
using UnityEngine;

namespace SEE.Game.UI.CodeWindow
{
    /// <summary>
    /// This part of the <see cref="CodeWindow"/> class contains the desktop specific UI code.
    /// </summary>
    public partial class CodeWindow
    {
        protected override void StartDesktop()
        {
            if (Title == null || Text == null)
            {
                Debug.LogError("Title and text must be defined when setting up CodeWindow!\n");
                return;
            }

            // We need to create a new world-space canvas instead of using the existing one.
            // Since this is done for each one, we'll group them together.
            GameObject group = GameObject.Find(CODE_WINDOWS_NAME);
            if (group == null)
            {
                group = new GameObject {name = CODE_WINDOWS_NAME};
            }

            // Create new code canvas from prefab
            Object codeCanvasPrefab = Resources.Load<GameObject>(CODE_CANVAS_PREFAB);
            CodeCanvas = Instantiate(codeCanvasPrefab, group.transform, true) as GameObject;
            if (CodeCanvas == null)
            {
                Debug.LogError("Couldn't instantiate CodeCanvas from prefab.\n");
                return;
            }

            CodeCanvas.name = $"Code Canvas '{Title}'";
            if (!CodeCanvas.TryGetComponentOrLog(out Canvas canvas) ||
                !CodeCanvas.TryGetComponentOrLog(out RectTransform rectTransform))
            {
                return;
            }

            canvas.worldCamera = Camera.main;

            // Position and scale canvas in world
            rectTransform.sizeDelta = Resolution;
            float scale = WorldWidth / Resolution.x;
            rectTransform.localScale = new Vector3(scale, scale, scale);
            if (Anchor != null)
            {
                Vector3 anchorPosition = Anchor.transform.position;
                rectTransform.position = anchorPosition + new Vector3(0, AnchorDistance, 0);
                // If an anchor is defined, we can also add a connecting line.
                Vector3 startLinePosition = anchorPosition;
                startLinePosition.y = BoundingBox.GetRoof(new List<GameObject> {Anchor});
                GameObject edge = new GameObject();
                // The line should touch the lowest point of the canvas, so we use the y-value of a bottom corner
                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);
                Vector3 endLinePosition = new Vector3(startLinePosition.x, corners[0].y, startLinePosition.z);
                LineFactory.Draw(edge, new[] {startLinePosition, endLinePosition},
                    0.01f, Materials.New(Materials.ShaderType.Opaque, Color.black)); //TODO: TransparentLine
                edge.transform.SetParent(canvas.transform);
                //TODO animations
            }
            else
            {
                Debug.LogWarning("No anchor for the CodeCanvas has been defined." +
                                 " Positioning CodeWindow at origin (with y-offset) instead.");
                rectTransform.position = new Vector3(0, AnchorDistance, 0);
            }

            canvas.transform.Find("CodeWindow/Dragger/Title").gameObject.GetComponent<TextMeshProUGUI>().text = Title;
            if (canvas.transform.Find("CodeWindow/Content/Scrollable/Code").gameObject.TryGetComponentOrLog(out TextMeshProUGUI text))
            {
                text.text = Text;
                text.fontSize = FontSize;
            }
            
            // Make canvas always face the camera
            canvas.gameObject.AddComponent<CanvasFaceCamera>();
        }
    }
}