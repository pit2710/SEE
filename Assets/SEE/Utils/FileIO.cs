﻿using System.IO;
using System.Text;

namespace SEE.Utils
{
    /// <summary>
    /// Provides utilities for File input/output.
    /// </summary>
    public static class FileIO
    {
        /// <summary>
        /// Returns the text contained in the file with given <paramref name="fileName"/>
        /// in the range <paramref name="fromLine"/> to <paramref name="toLine"/>.
        ///
        /// If the file has less lines than <paramref name="fromLine"/>, the empty
        /// string will be returned. If the file has less lines than  <paramref name="toLine"/>
        /// only the lines from <paramref name="fromLine"/> to the last line in the file
        /// will be returned.
        ///
        /// Precondition: <paramref name="fromLine"/> must be greater than zero
        /// and must the less than or equal to <paramref name="toLine"/>.
        /// </summary>
        /// <param name="fileName">name of the file</param>
        /// <param name="fromLine">the start of the requested line range</param>
        /// <param name="toLine">the end of the requested line range</param>
        /// <returns>file content in the specified line range</returns>
        public static string Read(string fileName, int fromLine, int toLine)
        {
            UnityEngine.Assertions.Assert.IsTrue(fromLine > 0 && fromLine <= toLine);

            StringBuilder result = new();
            int lineNo = 0;

            using FileStream fileStream = new(path: fileName, mode: FileMode.Open, access: FileAccess.Read,
                                              share: FileShare.Read, bufferSize: 4096, options: FileOptions.SequentialScan);
            using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                lineNo++;
                if (lineNo > toLine)
                {
                    break;
                }
                else if (fromLine <= lineNo)
                {
                    result.AppendLine(line);
                }
            }
            return result.ToString();
        }
    }
}