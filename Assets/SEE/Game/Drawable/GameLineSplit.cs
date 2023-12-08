﻿using SEE.Net.Actions.Drawable;
using RTG;
using SEE.Game;
using SEE.Game.UI.Notification;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SEE.Game.Drawable.Configurations;
using SEE.Game.Drawable;

namespace Assets.SEE.Game.Drawable
{
    /// <summary>
    /// This class provides functionality for splitting lines. 
    /// The splitting point can either be retained (split) or deleted (pointerase)
    /// </summary>
    public static class GameLineSplit
    {
        /// <summary>
        /// Method for splitting a line.
        /// The splitting point can either be retained (split) or deleted (pointerase)
        /// </summary>
        /// <param name="drawable">The drawable on that the lines should be redrawn.</param>
        /// <param name="originLine">Configuration of the original line.</param>
        /// <param name="matchedIndices">The indices of the points to be split.</param>
        /// <param name="positions">The positions of the line.</param>
        /// <param name="lines">List that holds the new created line configurations.</param>
        /// <param name="removeMatchedIndex">Specifies whether the split points should be deleted.</param>
        public static void Split(GameObject drawable, LineConf originLine, List<int> matchedIndices, List<Vector3> positions, List<LineConf> lines, bool removeMatchedIndex)
        {
            int removeCounter = removeMatchedIndex ? 1 : 0;

            if (matchedIndices.Count > 1)
            {
                List<List<Vector3>> parts = new ();
                for (int i = 0; i < matchedIndices.Count; i++)
                {
                    int newI = removeMatchedIndex ? matchedIndices[i] : matchedIndices[i] + 1;
                    newI = newI < 1 ? 1 : newI;
                    if (i > 0)
                    {
                        int startIndex = removeMatchedIndex ? matchedIndices[i - 1] + 1 : matchedIndices[i - 1];
                        parts.Add(positions.GetRange(startIndex, newI));
                    } else
                    {
                        parts.Add(positions.GetRange(0, newI));
                    }
                }
                foreach(List<Vector3> list in parts)
                {
                    TryReDraw(drawable, originLine, list.ToArray(), lines);
                }
                if (lines.Count == 1 && !removeMatchedIndex)
                {
                    ShowNotification.Warn("Can't split", "The line can't split on start/end point.\nThe line was redrawn nonetheless.");
                }
            }
            else
            {
                if (matchedIndices.Count == 1)
                {
                    Vector3[] begin = positions.GetRange(0, matchedIndices[0] + 1 - removeCounter).ToArray();
                    if (begin.Length == 1 && !removeMatchedIndex)
                    {
                        ShowNotification.Warn("Line Split Problem: ", "You can't split the line on the start point.\nThe line was redrawn nonetheless.");
                    } else if (begin.Length == positions.Count && !removeMatchedIndex)
                    {
                        ShowNotification.Warn("Line Split Problem: ", "You can't split the line on the end point.\nThe line was redrawn nonetheless.");
                    }
                    int lastIndex = positions.Count - removeCounter - matchedIndices[0];
                    Vector3[] end = positions.GetRange(matchedIndices[0] + removeCounter, lastIndex).ToArray();

                    TryReDraw(drawable, originLine, begin, lines);
                    TryReDraw(drawable, originLine, end, lines);
                }
            }

            
        }

        /// <summary>
        /// Checks if the line can be redrawn.
        /// </summary>
        /// <param name="drawable">The drawable on that the line should be drawn.</param>
        /// <param name="originLine">The configuration of the original line.</param>
        /// <param name="positions">The positions for the new line.</param>
        /// <param name="lines">List that holds the new line configurations.</param>
        private static void TryReDraw(GameObject drawable, LineConf originLine, Vector3[] positions, List<LineConf> lines)
        {
            if (positions.Length > 1)
            {
                lines.Add(ReDraw(drawable, originLine, positions));
            }
        }

        /// <summary>
        /// Redraws the line with the new sub positions.
        /// </summary>
        /// <param name="drawable">Drawable on that the line should be redrawn.</param>
        /// <param name="originLine">Configuration of the old line.</param>
        /// <param name="positions">The new sub positions of the new line.</param>
        /// <returns>Configuration of the new sub line.</returns>
        private static LineConf ReDraw(GameObject drawable, LineConf originLine, Vector3[] positions)
        {
            LineConf lineToCreate = (LineConf)originLine.Clone();
            lineToCreate.id = "";
            lineToCreate.rendererPositions = positions;

            GameObject newLine = GameDrawer.ReDrawLine(drawable, lineToCreate);
            new DrawOnNetAction(drawable.name, GameFinder.GetDrawableParentName(drawable), LineConf.GetLine(newLine)).Execute();

            return LineConf.GetLine(newLine);
        }
    }
}