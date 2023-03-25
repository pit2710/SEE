﻿//Copyright 2020 Florian Garbade

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

using System.Collections.Generic;

namespace SEE.DataModel.DG
{
    /// <summary>
    /// Compares two nodes by Node.ID for equality.
    /// </summary>
    public class NodeEqualityComparer : IEqualityComparer<Node>
    {
        /// <summary>
        /// True if <paramref name="x"/> and <paramref name="y"/> have the same ID.
        /// </summary>
        /// <param name="x">node to be compared to <paramref name="y"/></param>
        /// <param name="y">node to be compared to <paramref name="x"/></param>
        /// <returns>True if <paramref name="x"/> and <paramref name="y"/> have the same ID.</returns>
        public bool Equals(Node x, Node y)
        {
            return x.ID.Equals(y?.ID);
        }

        /// <summary>
        /// Hash code for <paramref name="node"/> based on its ID.
        /// </summary>
        /// <param name="node">node whose hash code is requested</param>
        /// <returns>hash code for <paramref name="node"/></returns>
        public int GetHashCode(Node node)
        {
            return node.ID.GetHashCode();
        }
    }
}
