namespace SEE.Layout.NodeLayouts.IncrementalEvostreets
{
    /// <summary>
    /// Direction in the plane containing the x-axis and z-axis
    /// </summary>
    internal enum Direction
    {
        /// <summary>
        /// Left side means decreasing x coordinate
        /// </summary>
        Left,

        /// <summary>
        /// Right side means increasing x coordinate
        /// </summary>
        Right,

        /// <summary>
        /// Upper side means decreasing z coordinate
        /// </summary>
        Upper,

        /// <summary>
        /// Lower side means increasing z coordinate
        /// </summary>
        Lower
    }
}
