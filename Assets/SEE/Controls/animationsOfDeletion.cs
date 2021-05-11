using SEE.Game;
using SEE.GO;
using SEE.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SEE.DataModel.DG;

/// <summary>
/// A class providing methods needed for the animations of gameobjects having been deleted by the user, for instance
/// the movement of a gamenode to the garbage can, as well as the inverse undo-mechanism.
/// </summary>
namespace SEE.Controls
{
    public static class AnimationsOfDeletion
    {
        /// <summary>
        /// The garbage can the deleted nodes will be moved to. It is the object named 
        /// <see cref="GarbageCanName"/>.
        /// </summary>
        private static GameObject garbageCan;

        /// <summary>
        /// The animation time of the animation of moving a node to the top of the garbage can.
        /// </summary>
        public const float TimeForAnimation = 1f;

        /// <summary>
        /// The waiting time of the animation for moving a node into a garbage can from over the garbage can.
        /// </summary>
        private const float TimeToWait = 1f;

        /// <summary>
        ///  A vector for an objects localScale which fits into the garbage can.
        ///  TODO: Currently set to an absolute value. Should be set abstract, e.g., half of the 
        ///  garbage can's diameter. 
        /// </summary>
        private static readonly Vector3 defaultGarbageVector = new Vector3(0.1f, 0.1f, 0.1f);

        /// <summary>
        /// A list of ratios of the current localScale and a target scale.
        /// </summary>
        private static Dictionary<GameObject, Vector3> shrinkFactors { get; set; } = new Dictionary<GameObject, Vector3>();

        // <summary>
        /// A history of the old positions of the nodes deleted by this action.
        /// </summary>
        private static Dictionary<GameObject, Vector3> oldPositions = new Dictionary<GameObject, Vector3>();

        /// <summary>
        /// Number of animations used for an object's expansion, removing it from the garbage can.
        /// </summary>
        private const float StepsOfExpansionAnimation = 10;

        /// <summary>
        /// The time (in seconds) between animations of expanding a node that is being restored
        /// from the garbage can.
        /// </summary>
        private const float TimeBetweenExpansionAnimation = 0.14f;

        /// <summary>
        /// A history of all edges and the graph where they were attached to, deleted by this action.
        /// </summary>
        private static Dictionary<GameObject, Graph> deletedEdges { get; set; } = new Dictionary<GameObject, Graph>();

        /// <summary>
        /// Moves all nodes in <paramref name="deletedNodes"/> to the garbage can
        /// using an animation. When they finally arrive there, they will be 
        /// deleted. 
        /// 
        /// Assumption: <paramref name="deletedNodes"/> contains all nodes in a subtree
        /// of the game-node hierarchy. All of them represent graph nodes.
        /// </summary>
        /// <param name="deletedNodes">the deleted nodes which will be moved to the garbage can.</param>
        /// <returns>the waiting time between moving deleted nodes over the garbage can and then into the garbage can</returns>
        public static IEnumerator MoveNodeToGarbage(IList<GameObject> deletedNodes)
        {
            garbageCan = GameObject.Find("GarbageCan");
            // We need to reset the portal of all all deletedNodes so that we can move
            // them to the garbage bin. Otherwise they will become invisible if they 
            // leave their portal.
            foreach (GameObject deletedNode in deletedNodes)
            {
                oldPositions[deletedNode] = deletedNode.transform.position;
                if (!deletedNodes.Contains(deletedNode))
                {
                    Portal.SetInfinitePortal(deletedNode);
                }
            }
            foreach (GameObject deletedNode in deletedNodes)
            {
                Tweens.Move(deletedNode, new Vector3(garbageCan.transform.position.x, garbageCan.transform.position.y + 1.4f, garbageCan.transform.position.z), TimeForAnimation);
            }
            yield return new WaitForSeconds(TimeToWait);

            foreach (GameObject deletedNode in deletedNodes)
            {
                Vector3 shrinkFactor = VectorOperations.DivideVectors(deletedNode.transform.localScale, defaultGarbageVector);
                if (!shrinkFactors.ContainsKey(deletedNode))
                {
                    shrinkFactors.Add(deletedNode, shrinkFactor);
                }
                deletedNode.transform.localScale = Vector3.Scale(deletedNode.transform.localScale, shrinkFactor);
                Tweens.Move(deletedNode, new Vector3(garbageCan.transform.position.x, garbageCan.transform.position.y, garbageCan.transform.position.z), TimeForAnimation);
            }
            yield return new WaitForSeconds(TimeToWait);
        }

        /// <summary>
        /// Removes all given nodes from the garbage can back into the city.
        /// </summary>
        /// <param name="deletedNode">The nodes to be removed from the garbage-can</param>
        /// <returns>the waiting time between moving deleted nodes from the garbage-can and then to the city</returns>
        public static IEnumerator RemoveNodeFromGarbage(IList<GameObject> deletedNodes)
        {
            // vertical movement of nodes
            foreach (GameObject deletedNode in deletedNodes)
            {
                Tweens.Move(deletedNode, new Vector3(garbageCan.transform.position.x, garbageCan.transform.position.y + 1.4f, garbageCan.transform.position.z), TimeForAnimation);
                PlayerSettings.GetPlayerSettings().StartCoroutine(WaitAndExpand(deletedNode));
            }

            yield return new WaitForSeconds(TimeToWait);

            // back to the original position
            foreach (GameObject node in deletedNodes)
            {
                Tweens.Move(node, oldPositions[node], TimeForAnimation);
            }

            yield return new WaitForSeconds(TimeToWait);
            deletedNodes.Clear();
            deletedEdges.Clear();
            InteractableObject.UnselectAll(true);
        }

        /// <summary>
        /// Coroutine that waits and expands the shrunk object which is currently being removed from the garbage can.
        /// </summary>
        /// <param name="deletedNode">The nodes to be removed from the garbage-can</param>
        /// <returns>the waiting time between moving deleted nodes from the garbage-can and then to the city</returns>
        private static IEnumerator WaitAndExpand(GameObject deletedNode)
        {
            yield return new WaitForSeconds(TimeToWait);
            Vector3 shrinkFactor = shrinkFactors[deletedNode];
            float animationsCount = StepsOfExpansionAnimation;
            float exponent = 1 / StepsOfExpansionAnimation;
            shrinkFactor = VectorOperations.ExponentOfVectorCoordinates(shrinkFactor, exponent);

            while (animationsCount > 0)
            {
                deletedNode.transform.localScale = VectorOperations.DivideVectors(shrinkFactor, deletedNode.transform.localScale);
                yield return new WaitForSeconds(TimeBetweenExpansionAnimation);
                animationsCount--;
            }
        }

        /// <summary>
        /// Delays the visibility of edges having been removed from the garbage can.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static IEnumerator DelayEdges(GameObject edge)
        {
            yield return new WaitForSeconds(TimeForAnimation + TimeToWait);
            edge.SetVisibility(true, true);
        }

        /// <summary>
        /// Hides a given param name="gameEdge"></param> having been deleted before.
        /// </summary>
        /// <param name="gameEdge"></param>
        public static void HideEdges(GameObject gameEdge)
        {
            gameEdge.SetVisibility(false, true);
            if (!deletedEdges.ContainsKey(gameEdge))
            {
                deletedEdges.Add(gameEdge, gameEdge.GetGraph());
            }
        }
    }
}