﻿using SEE.Game;
using SEE.Game.Drawable;
using SEE.Game.Drawable.ActionHelpers;
using SEE.Game.Drawable.Configurations;
using SEE.UI.Notification;
using SEE.Net.Actions.Drawable;
using SEE.UI.Menu.Drawable;
using SEE.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SEE.Utils.History;

namespace SEE.Controls.Actions.Drawable
{
    /// <summary>
    /// Allows the user to draw a shape.
    /// </summary>
    public class DrawShapesAction : DrawableAction
    {
        /// <summary>
        /// The object holding the line renderer.
        /// </summary>
        private GameObject shape;

        /// <summary>
        /// The drawable where the shape is displayed.
        /// </summary>
        private GameObject drawable;

        /// <summary>
        /// The positions of the line in local space.
        /// </summary>
        private Vector3[] positions = new Vector3[1];

        /// <summary>
        /// Saves all the information needed to revert or repeat this action.
        /// </summary>
        private Memento memento;

        /// <summary>
        /// This struct can store all the information needed to revert or repeat a <see cref="DrawShapesAction"/>.
        /// </summary>
        private readonly struct Memento
        {
            /// <summary>
            /// The drawable where the shape is displayed.
            /// </summary>
            public readonly DrawableConfig Drawable;
            /// <summary>
            /// The configuration of the shape.
            /// </summary>
            public readonly LineConf Shape;

            /// <summary>
            /// The constructor, which simply assigns its parameters to the fields in this class.
            /// </summary>
            /// <param name="drawable">The drawable where the shape is displayed.</param>
            /// <param name="shape">The configuration of the shape.</param>
            public Memento(GameObject drawable, LineConf shape)
            {
                Drawable = DrawableConfigManager.GetDrawableConfig(drawable);
                Shape = shape;
            }
        }

        /// <summary>
        /// True if the action is drawing.
        /// Also necessary to identify whether the line shape was successfully drawn.
        /// </summary>
        private bool drawing = false;

        /// <summary>
        /// True if the user finished the line shape drawing via menu.
        /// </summary>
        private bool finishDrawingViaButton = false;

        /// <summary>
        /// Enables the shape menu.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            ShapeMenu.Enable();
            ShapeMenu.AssignFinishButton(() =>
            {
                if (drawing && positions.Length > 1 &&
                    ShapeMenu.GetSelectedShape() == ShapePointsCalculator.Shape.Line)
                {
                    finishDrawingViaButton = true;
                }
            });
        }


        /// <summary>
        /// Stops the action. It disable the shape menu and
        /// destroys the line shape if it is not successfully completed.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            ShapeMenu.Disable();
            if (drawing && shape != null)
            {
                Destroyer.Destroy(shape);
            }
        }

        /// <summary>
        /// This method manages the player's interaction with the mode <see cref="ActionStateType.DrawShapes"/>.
        /// Specifically: Allows the user to draw a shape.
        /// For all shapes except Line, a single click on the drawable is sufficient to draw the desired shape.
        /// Simply enter the desired values in the Shape Menu.
        /// For the Line shape type, multiple clicks (one for each point) are required.
        /// </summary>
        /// <returns>Whether this action is finished</returns>
        public override bool Update()
        {
            if (!Raycasting.IsMouseOverGUI())
            {
                /// Block for initiating shape drawing.
                /// All shapes, except for straight lines, are also completed within this block.
                if (Input.GetMouseButtonDown(0) &&
                    Raycasting.RaycastAnything(out RaycastHit raycastHit) &&
                    (raycastHit.collider.gameObject.CompareTag(Tags.Drawable) ||
                    GameFinder.HasDrawable(raycastHit.collider.gameObject))
                    && !drawing)
                {
                    return ShapeDrawing(raycastHit);
                }

                /// This block provides a line preview to select the desired position of the next line point.
                LineShapePreview();

                /// With this block, the user can add a new point to the line.
                AddLineShapePoint();

                /// With left shift key can the loop option of the shape menu be toggled.
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    ShapeMenu.GetLoopManager().isOn = !ShapeMenu.GetLoopManager().isOn;
                    ShapeMenu.GetLoopManager().UpdateUI();
                }

                /// Block for successfully completing the line.
                /// It adds a final point to the line.
                /// It requires a left-click with the left Ctrl key held down.
                if (Input.GetMouseButtonUp(0) && Input.GetKey(KeyCode.LeftControl)
                    && drawing && positions.Length > 0
                    && ShapeMenu.GetSelectedShape() == ShapePointsCalculator.Shape.Line
                    && Raycasting.RaycastAnything(out RaycastHit hit)
                    && (hit.collider.gameObject.CompareTag(Tags.Drawable) ||
                        GameFinder.HasDrawable(hit.collider.gameObject)))
                {
                    Vector3 newPosition = shape.transform.InverseTransformPoint(hit.point) - ValueHolder.DistanceToDrawable;
                    if (newPosition != positions.Last())
                    {
                        Vector3[] newPositions = new Vector3[positions.Length + 1];
                        Array.Copy(sourceArray: positions, destinationArray: newPositions, length: positions.Length);
                        newPositions[newPositions.Length - 1] = newPosition;
                        positions = newPositions;

                        GameDrawer.Drawing(shape, positions);
                        new DrawNetAction(drawable.name, GameFinder.GetDrawableParentName(drawable),
                            LineConf.GetLine(shape)).Execute();
                    }
                    FinishDrawing();
                    return true;
                }

                /// Block for successfully completing the line without adding a new point.
                /// It requires a wheel-click.
                if (Input.GetMouseButtonUp(2)
                    && drawing && positions.Length > 1
                    && ShapeMenu.GetSelectedShape() == ShapePointsCalculator.Shape.Line)
                {
                    FinishDrawing();
                    return true;
                }
            }
            /// This block is outside the !Raycasting.IsMouseOverGUI check to allow
            /// the immediate detection of a click on
            /// the Finish button of the menu,
            /// even if the mouse cursor is still over the GUI.
            if (finishDrawingViaButton)
            {
                FinishDrawing();
                return true;
            }

            /// Block for canceling the drawing of a line shape.
            CancelDrawing();

            /// Block for removing the last point during the drawing of a line shape.
            RemoveLastPoint();

            return false;
        }

        /// <summary>
        /// Provides the option to cancel drawing a line shape with the escape button.
        /// </summary>
        private void CancelDrawing()
        {
            if (drawing && SEEInput.Cancel())
            {
                ShowNotification.Info("Line-Shape drawing canceled.", "The drawing of the shape art line has been canceled.");
                new EraseNetAction(drawable.name, GameFinder.GetDrawableParentName(drawable), shape.name).Execute();
                Destroyer.Destroy(shape);
                positions = new Vector3[1];
                drawing = false;
                shape = null;
            }
        }

        /// <summary>
        /// Provides the option to remove the last added point.
        /// Press the Tab key for this action.
        /// If the line does not have enough points to remove, it will be deleted.
        /// </summary>
        private void RemoveLastPoint()
        {
            if (drawing && Input.GetKeyDown(KeyCode.Tab))
            {
                if (shape.GetComponent<LineRenderer>().positionCount >= 3)
                {
                    ShowNotification.Info("Last point removed.", "The last placed point of the line has been removed.");
                    LineRenderer renderer = shape.GetComponent<LineRenderer>();
                    renderer.positionCount -= 2;
                    positions = positions.ToList().GetRange(0, positions.Length - 1).ToArray();
                    renderer.SetPositions(positions);
                    new DrawNetAction(drawable.name, GameFinder.GetDrawableParentName(drawable), LineConf.GetLine(shape)).Execute();
                }
                else
                {
                    ShowNotification.Info("Line-Shape drawing canceled.", "The drawing of the shape art line has been canceled.");
                    new EraseNetAction(drawable.name, GameFinder.GetDrawableParentName(drawable), shape.name).Execute();
                    Destroyer.Destroy(shape);
                    positions = new Vector3[1];
                    drawing = false;
                    shape = null;
                }
            }
        }

        /// <summary>
        /// Performs the drawing of shapes.
        /// However, for straight lines, only the drawing is initialized.
        /// To do this, the <see cref="GetSelectedShapePosition(Vector3, Vector3)"/> method is
        /// first called to determine the positions.
        /// Subsequently, for the selected shape (if it is not a line), the <see cref="DrawShape(Vector3)"/> method is called.
        /// </summary>
        /// <param name="raycastHit">The raycast hit of the selection.</param>
        /// <returns>Whatever the shape creation is completed.</returns>
        private bool ShapeDrawing(RaycastHit raycastHit)
        {
            drawable = raycastHit.collider.gameObject.CompareTag(Tags.Drawable) ?
                        raycastHit.collider.gameObject : GameFinder.GetDrawable(raycastHit.collider.gameObject);
            drawing = true;
            Vector3 convertedHitPoint = GameDrawer.GetConvertedPosition(drawable, raycastHit.point);

            GetSelectedShapePosition(convertedHitPoint, raycastHit.point);

            /// This block draws and completes the action for all shapes except lines.
            if (ShapeMenu.GetSelectedShape() != ShapePointsCalculator.Shape.Line)
            {
                return DrawShape(convertedHitPoint);
            }
            return false;
        }

        /// <summary>
        /// Calculates the points for the selected shape based
        /// on the chosen values in the <see cref="ShapeMenu"/>.
        /// For the Line shape, only the first point is set,
        /// as the others cannot be calculated and must be chosen by the user.
        /// </summary>
        /// <param name="convertedHitPoint">The hit point in local space, depending on the chosen drawable.</param>
        /// <param name="hitpoint">The hit point of the raycast hit.</param>
        private void GetSelectedShapePosition(Vector3 convertedHitPoint, Vector3 hitpoint)
        {
            switch (ShapeMenu.GetSelectedShape())
            {
                case ShapePointsCalculator.Shape.Line:

                    positions[0] = hitpoint;
                    shape = GameDrawer.StartDrawing(drawable, positions, ValueHolder.CurrentColorKind,
                        ValueHolder.CurrentPrimaryColor, ValueHolder.CurrentSecondaryColor,
                        ValueHolder.CurrentThickness, ValueHolder.CurrentLineKind,
                        ValueHolder.CurrentTiling);
                    positions[0] = shape.transform.InverseTransformPoint(positions[0]) - ValueHolder.DistanceToDrawable;
                    break;
                case ShapePointsCalculator.Shape.Square:
                    positions = ShapePointsCalculator.Square(convertedHitPoint, ShapeMenu.GetValue1());
                    break;
                case ShapePointsCalculator.Shape.Rectangle:
                    positions = ShapePointsCalculator.Rectanlge(convertedHitPoint, ShapeMenu.GetValue1(),
                        ShapeMenu.GetValue2());
                    break;
                case ShapePointsCalculator.Shape.Rhombus:
                    positions = ShapePointsCalculator.Rhombus(convertedHitPoint, ShapeMenu.GetValue1(),
                        ShapeMenu.GetValue2());
                    break;
                case ShapePointsCalculator.Shape.Kite:
                    positions = ShapePointsCalculator.Kite(convertedHitPoint, ShapeMenu.GetValue1(),
                        ShapeMenu.GetValue2(), ShapeMenu.GetValue3());
                    break;
                case ShapePointsCalculator.Shape.Triangle:
                    positions = ShapePointsCalculator.Triangle(convertedHitPoint, ShapeMenu.GetValue1(),
                        ShapeMenu.GetValue2());
                    break;
                case ShapePointsCalculator.Shape.Circle:
                    positions = ShapePointsCalculator.Circle(convertedHitPoint, ShapeMenu.GetValue1());
                    break;
                case ShapePointsCalculator.Shape.Ellipse:
                    positions = ShapePointsCalculator.Ellipse(convertedHitPoint, ShapeMenu.GetValue1(),
                        ShapeMenu.GetValue2());
                    break;
                case ShapePointsCalculator.Shape.Parallelogram:
                    positions = ShapePointsCalculator.Parallelogram(convertedHitPoint, ShapeMenu.GetValue1(),
                        ShapeMenu.GetValue2(), ShapeMenu.GetValue4());
                    break;
                case ShapePointsCalculator.Shape.Trapezoid:
                    positions = ShapePointsCalculator.Trapezoid(convertedHitPoint, ShapeMenu.GetValue1(),
                        ShapeMenu.GetValue2(), ShapeMenu.GetValue3());
                    break;
                case ShapePointsCalculator.Shape.Polygon:
                    positions = ShapePointsCalculator.Polygon(convertedHitPoint, ShapeMenu.GetValue1(),
                        ShapeMenu.GetVertices());
                    break;
            }
        }

        /// <summary>
        /// Creates the calculated shape if it has at least three different positions.
        /// This ensures that the Mesh Collider can be created.
        /// Subsequently, the pivot point of the shape is set,
        /// and the action is completed by creating a Memento and setting the progress state to Completed.
        /// If the shape cannot provide three different points, the action is reset.
        /// </summary>
        /// <param name="convertedHitPoint">The hit point in local space, depending on the chosen drawable.</param>
        /// <returns>Whatever the state of the shape creation is completed.</returns>
        private bool DrawShape(Vector3 convertedHitPoint)
        {
            if (GameDrawer.DifferentPositionCounter(positions) > 1)
            {
                shape = GameDrawer.DrawLine(drawable, "", positions, ValueHolder.CurrentColorKind,
                    ValueHolder.CurrentPrimaryColor, ValueHolder.CurrentSecondaryColor, ValueHolder.CurrentThickness, false,
                    ValueHolder.CurrentLineKind, ValueHolder.CurrentTiling);
                shape.GetComponent<LineRenderer>().loop = false;
                shape = GameDrawer.SetPivotShape(shape, convertedHitPoint);
                LineConf currentShape = LineConf.GetLine(shape);
                memento = new Memento(drawable, currentShape);
                new DrawNetAction(memento.Drawable.ID, memento.Drawable.ParentID, currentShape).Execute();
                CurrentState = IReversibleAction.Progress.Completed;
                drawing = false;
                return true;
            }
            else
            {
                positions = new Vector3[1];
                drawing = false;
                shape = null;
                return false;
            }
        }

        /// <summary>
        /// This method provides a line preview for the user
        /// to select the desired position of the next line point.
        /// </summary>
        private void LineShapePreview()
        {
            if (drawing && !Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) &&
                    Raycasting.RaycastAnything(out RaycastHit rh) &&
                    (rh.collider.gameObject.CompareTag(Tags.Drawable) ||
                    GameFinder.HasDrawable(rh.collider.gameObject)) &&
                    ShapeMenu.GetSelectedShape() == ShapePointsCalculator.Shape.Line
                    && (drawable == null || drawable != null && GameFinder.GetDrawable(rh.collider.gameObject).Equals(drawable)))
            {
                Vector3 newPosition = shape.transform.InverseTransformPoint(rh.point) - ValueHolder.DistanceToDrawable;
                Vector3[] newPositions = new Vector3[positions.Length + 1];
                Array.Copy(sourceArray: positions, destinationArray: newPositions, length: positions.Length);
                newPositions[^1] = newPosition;
                GameDrawer.Drawing(shape, newPositions);
                new DrawNetAction(drawable.name, GameFinder.GetDrawableParentName(drawable), LineConf.GetLine(shape)).Execute();
            }
        }

        /// <summary>
        /// Provides the function to add a new point in the Line shape.
        /// However, the new point must be different from the previous one.
        /// This requires a left mouse click, with neither the left Shift nor the left Ctrl key pressed.
        /// </summary>
        private void AddLineShapePoint()
        {
            if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift) &&
                Raycasting.RaycastAnything(out RaycastHit hit) &&
                (hit.collider.gameObject.CompareTag(Tags.Drawable) ||
                GameFinder.HasDrawable(hit.collider.gameObject))
                && drawing && ShapeMenu.GetSelectedShape() == ShapePointsCalculator.Shape.Line
                && (drawable == null || drawable != null && GameFinder.GetDrawable(hit.collider.gameObject).Equals(drawable)))
            {
                Vector3 newPosition = shape.transform.InverseTransformPoint(hit.point) - ValueHolder.DistanceToDrawable;
                if (newPosition != positions.Last())
                {
                    Vector3[] newPositions = new Vector3[positions.Length + 1];
                    Array.Copy(sourceArray: positions, destinationArray: newPositions, length: positions.Length);
                    newPositions[newPositions.Length - 1] = newPosition;
                    positions = newPositions;

                    GameDrawer.Drawing(shape, positions);
                    new DrawNetAction(drawable.name, GameFinder.GetDrawableParentName(drawable), LineConf.GetLine(shape)).Execute();
                }
            }
        }

        /// <summary>
        /// Finish the drawing of the line shape.
        /// It must be a separate method as it can be called from two different points.
        /// </summary>
        private void FinishDrawing()
        {
            GameDrawer.Drawing(shape, positions);
            shape.GetComponent<LineRenderer>().loop = ShapeMenu.GetLoopManager().isOn;
            shape = GameDrawer.SetPivot(shape);
            LineConf currentShape = LineConf.GetLine(shape);
            memento = new Memento(drawable, currentShape);
            new DrawNetAction(memento.Drawable.ID, memento.Drawable.ParentID, currentShape).Execute();
            CurrentState = IReversibleAction.Progress.Completed;
            drawing = false;
        }

        /// <summary>
        /// Reverts this action, i.e., deletes the drawn shape.
        /// </summary>
        public override void Undo()
        {
            base.Undo();
            if (shape == null)
            {
                shape = GameFinder.FindChild(memento.Drawable.GetDrawable(), memento.Shape.Id);
            }
            if (shape != null)
            {
                new EraseNetAction(memento.Drawable.ID, memento.Drawable.ParentID, memento.Shape.Id).Execute();
                Destroyer.Destroy(shape);
            }
        }

        /// <summary>
        /// Repeats this action, i.e., redraws the shape.
        /// </summary>
        public override void Redo()
        {
            base.Redo();
            shape = GameDrawer.ReDrawLine(memento.Drawable.GetDrawable(), memento.Shape);
            if (shape != null)
            {
                new DrawNetAction(memento.Drawable.ID, memento.Drawable.ParentID, LineConf.GetLine(shape)).Execute();
            }
        }

        /// <summary>
        /// A new instance of <see cref="DrawShapesAction"/>.
        /// See <see cref="ReversibleAction.CreateReversibleAction"/>.
        /// </summary>
        /// <returns>new instance of <see cref="DrawShapesAction"/></returns>
        public static IReversibleAction CreateReversibleAction()
        {
            return new DrawShapesAction();
        }

        /// <summary>
        /// A new instance of <see cref="DrawShapesAction"/>.
        /// See <see cref="ReversibleAction.NewInstance"/>.
        /// </summary>
        /// <returns>new instance of <see cref="DrawShapesAction"/></returns>
        public override IReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }

        /// <summary>
        /// Returns the <see cref="ActionStateType"/> of this action.
        /// </summary>
        /// <returns><see cref="ActionStateType.DrawShapes"/></returns>
        public override ActionStateType GetActionStateType()
        {
            return ActionStateTypes.DrawShapes;
        }

        /// <summary>
        /// The set of IDs of all gameObjects changed by this action.
        /// <see cref="ReversibleAction.GetActionStateType"/>
        /// </summary>
        /// <returns>The id of the created shape</returns>
        public override HashSet<string> GetChangedObjects()
        {
            if (memento.Drawable == null)
            {
                return new HashSet<string>();
            }
            else
            {
                return new HashSet<string>
                {
                    memento.Shape.Id
                };
            }
        }
    }
}