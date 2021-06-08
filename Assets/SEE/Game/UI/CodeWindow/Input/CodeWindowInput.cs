﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using SEE.Game.UI.Notification;
using SEE.Net.Dashboard;
using SEE.Net.Dashboard.Model.Issues;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Game.UI.CodeWindow
{
    /// <summary>
    /// Partial class containing methods related to processing input for the code windows.
    /// </summary>
    public partial class CodeWindow
    {
        /// <summary>
        /// Populates the code window with the content of the given non-filled lexer's token stream.
        /// </summary>
        /// <param name="lexer">Lexer with which the source code file was read. Must not be filled.</param>
        /// <param name="language">Token language for the given lexer. May be <c>null</c>, in which case it is
        /// determined by the lexer name.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="lexer"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">
        /// If the given <paramref name="lexer"/> is not of a supported grammar.
        /// </exception>
        public void EnterFromTokens(IEnumerable<SEEToken> tokens)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }
            
            // Avoid multiple enumeration in case iteration over the data source is expensive.
            List<SEEToken> tokenList = tokens.ToList();
            if (!tokenList.Any())
            {
                Text = "<i>This file is empty.</i>";
                return;
            }

            TokenLanguage language = tokenList.First().Language;
            
            // We need to insert this pseudo token here so that the first line gets a line number.
            tokenList.Insert(0, new SEEToken(string.Empty, SEEToken.Type.Newline, -1, 0, language));

            // Unsurprisingly, each newline token corresponds to a new line.
            // However, we need to also add "hidden" newlines contained in other tokens, e.g. block comments.
            int assumedLines = tokenList.Count(x => x.TokenType.Equals(SEEToken.Type.Newline)) 
                               + tokenList.Where(x => !x.TokenType.Equals(SEEToken.Type.Newline)) 
                                          .Aggregate(0, (l, token) => token.Text.Count(x => x == '\n'));
            // Needed padding is the number of lines, because the line number will be at most this long.
            int neededPadding = assumedLines.ToString().Length;
            int lineNumber = 1;
            foreach (SEEToken token in tokenList)
            {
                if (token.TokenType == SEEToken.Type.Unknown)
                {
                    Debug.LogError($"Unknown token encountered for text '{token.Text}'.\n");
                }

                if (token.TokenType == SEEToken.Type.Newline)
                {
                    AppendNewline(ref lineNumber, ref Text, neededPadding);
                }
                else if (token.TokenType != SEEToken.Type.EOF) // Skip EOF token completely.
                {
                    lineNumber = HandleMultilineToken(token);
                }
            }

            // Lines are equal to number of newlines, including the initial newline.
            lines = Text.Count(x => x.Equals('\n')); // No more weird CRLF shenanigans are present at this point.
            Text = Text.TrimStart('\n'); // Remove leading newline.

            # region Local Functions
            // Appends a newline to the text, assuming we're at theLineNumber and need the given padding.
            static void AppendNewline(ref int theLineNumber, ref string text, int padding)
            {
                // First, of course, the newline.
                text += "\n";
                // Add whitespace next to line number so it's consistent.
                text += string.Join("", Enumerable.Repeat(" ", padding - $"{theLineNumber}".Length));
                // Line number will be typeset in grey to distinguish it from the rest.
                text += $"<color=#CCCCCC>{theLineNumber}</color> ";
                theLineNumber++;
            }

            // Handles a token which may contain newlines and adds its syntax-highlighted content to the code window.
            int HandleMultilineToken(SEEToken token)
            {
                bool firstRun = true;
                string[] tokenLines = token.Text.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
                foreach (string line in tokenLines)
                {
                    // Any entry after the first is on a separate line.
                    if (!firstRun)
                    {
                        AppendNewline(ref lineNumber, ref Text, neededPadding);
                    }

                    // No "else if" because we still want to display this token.
                    if (token.TokenType == SEEToken.Type.Whitespace)
                    {
                        // We just copy the whitespace verbatim, no need to even color it.
                        // Note: We have to assume that whitespace will not interfere with TMP's XML syntax.
                        Text += line.Replace("\t", new string(' ', language.TabWidth));
                    }
                    else
                    {
                        Text += $"<color=#{token.TokenType.Color}><noparse>{line.Replace("noparse", "")}</noparse></color>";
                    }

                    firstRun = false;
                }

                return lineNumber;
            }
            #endregion
        }
        
        /// <summary>
        /// Populates the code window with the contents of the given file.
        /// This will overwrite any existing text.
        /// </summary>
        /// <param name="text">An array of lines to use for the code window.</param>
        /// <exception cref="ArgumentException">If <paramref name="text"/> is empty or <c>null</c></exception>
        public void EnterFromText(string[] text)
        {
            if (text == null || text.Length <= 0)
            {
                throw new ArgumentException("Given text must not be empty or null.\n");
            }

            int neededPadding = $"{text.Length}".Length;
            Text = "";
            for (int i = 0; i < text.Length; i++)
            {
                // Add whitespace next to line number so it's consistent.
                Text += string.Join("", Enumerable.Repeat(" ", neededPadding-$"{i+1}".Length));
                // Line number will be typeset in yellow to distinguish it from the rest.
                Text += $"<color=\"yellow\">{i+1}</color> <noparse>{text[i].Replace("noparse", "")}</noparse>\n";
            }
            lines = text.Length;
        }

        /// <summary>
        /// Populates the code window with the contents of the given file.
        /// This will overwrite any existing text.
        /// </summary>
        /// <param name="filename">The filename for the file to read.</param>
        /// <param name="syntaxHighlighting">Whether syntax highlighting shall be enabled.
        /// The language will be detected by looking at the file extension.
        /// If the language is not supported, an ArgumentException will be thrown.</param>
        public void EnterFromFile(string filename, bool syntaxHighlighting = true)
        {
            FilePath = filename;
            
            // Try to read the file, otherwise display the error message.
            if (!File.Exists(filename))
            {
                ShowNotification.Error("File not found", $"Couldn't find file '{filename}'.");
                Destroy(this);
                return;
            }
            try
            {
                //TODO: Maybe disable syntax highlighting for huge files, as it would impact performance badly.
                if (syntaxHighlighting)
                {
                    try
                    {
                        EnterFromTokens(SEEToken.fromFile(filename));
                        //TODO: Other issue types too
                        MarkIssues<StyleViolationIssue>(filename).Forget(); // initiate issue search
                        return;
                    }
                    catch (ArgumentException e)
                    {
                        // In case the filetype is not supported, we render the text normally.
                        Debug.LogError($"Encountered an exception, disabling syntax highlighting: {e}");
                    }
                }

                EnterFromText(File.ReadAllLines(filename));
            }
            catch (IOException exception)
            {
                ShowNotification.Error("File access error", $"Couldn't access file {filename}: {exception}");
                Destroy(this);
            }
        }

        private async UniTaskVoid MarkIssues<T>(string path) where T : Issue, new()
        {
            const string NOPARSE = "<noparse>";
            const string NOPARSE_CLOSE = "</noparse>";
            string queryPath = Path.GetFileName(path);
            IssueTable<T> issueTable = await DashboardRetriever.Instance.GetIssues<T>(fileFilter: $"\"*{queryPath}\"");
            await UniTask.SwitchToThreadPool(); // don't interrupt main UI thread
            if (issueTable.rows.Count == 0)
            {
                return;
            }
            List<T> issues = issueTable.rows.ToList();

            // When there are different paths in the issue table, this implies that there are some files 
            // which aren't actually the one we're looking for (because we've only matched by filename so far).
            // In this case, we'll gradually refine our results until this isn't the case anymore.
            for (int skippedParts = path.Count(x => x == Path.PathSeparator)-2; DifferentPaths(issues); skippedParts--)
            {
                Assert.IsTrue(path.Contains(Path.PathSeparator));
                // Skip the first <c>skippedParts</c> parts, so that we query progressively larger parts.
                queryPath = string.Join(Path.PathSeparator.ToString(), path.Split(Path.PathSeparator).Skip(skippedParts));
                issues.RemoveAll(x => !x.Entities.Select(e => e.path).Any(p => p.EndsWith(queryPath)));
            }
            
            //await UniTask.SwitchToMainThread(); // We interfere with UI now

            foreach (T issue in issues)
            {
                // If there are multiple entities, use all those whose path matches what we have
                foreach (Issue.SourceCodeEntity entity in issue.Entities.Where(x => x.path.EndsWith(queryPath)))
                {
                    if (entity.content != null)
                    {
                        await UnderlinePart(await GetContentIndices(entity.line, entity.content), issue);
                    }
                    else
                    {
                        // Apply underline to all lines between line and endLine, if endLine is defined
                        IEnumerable<int> entityLines = entity.endLine == null ? new[] {entity.line} 
                            : Enumerable.Range(entity.line, (int) entity.endLine - entity.line);

                        foreach (int line in entityLines)
                        {
                            await UnderlinePart(GetLineIndices(line), issue);
                        }
                    }
                    // TODO: Underline the entity
                }
            }

            #region Local Methods

            async UniTask UnderlinePart((int startIndex, int endIndex, bool noparseStart, bool noparseEnd) content, T issue)
            {
                const string UNDERLINE = "<u>";
                const string UNDERLINE_CLOSE = "</u>";
                (int startIndex, int endIndex, bool noparseStart, bool noparseEnd) = content;
                await UniTask.SwitchToMainThread();  // We need to modify the displayed text on the main thread
                
                // We put the closing tag in first, otherwise our endIndex would shift.
                TextMesh.text = TextMesh.text.Insert(endIndex, UNDERLINE_CLOSE).Insert(startIndex, UNDERLINE);
                if (noparseEnd)
                {
                    // We want to "unescape" <u> and </u> in the <noparse> segments.
                    // </noparse> is shifted right by <u>.
                    TextMesh.text = TextMesh.text.Insert(endIndex + UNDERLINE.Length + UNDERLINE_CLOSE.Length, NOPARSE)
                                            .Insert(endIndex + UNDERLINE.Length, NOPARSE_CLOSE);
                }
                if (noparseStart)
                {
                    TextMesh.text = TextMesh.text.Insert(startIndex + UNDERLINE.Length, NOPARSE).Insert(startIndex, NOPARSE_CLOSE);
                }
                
                await UniTask.SwitchToThreadPool(); // And we're done with that for now

                //TODO: Tooltip with info from issue
            }

            static bool DifferentPaths(ICollection<T> issues)
            {
                //TODO: Does this also detect different paths within an entity enumerable, not just across issues?
                HashSet<string> subPaths = new HashSet<string>(issues.First().Entities.Select(e => e.path));
                return issues.Select(x => x.Entities.Select(e => e.path)).Any(x => !subPaths.SetEquals(x));
            }


            // Returns (rich start index, rich end index, whether the beginning, end is in a <noparse>)
            async UniTask<(int, int, bool, bool)> GetContentIndices(int line, string content)
            {
                // For the motivation behind what happens here, please see method GetRichIndex.
                // As a TL;DR: Our TMP contains rich tags, while the given line and content don't account for them.
                
                (string lineContent, IList<TMP_CharacterInfo> contentInfo) = await GetLineInfo(line);
                // We get the start and end index within this list, not accounting for rich tags ("clean").
                int cleanStartIndex = lineContent.IndexOf(content, StringComparison.Ordinal);
                int cleanEndIndex = cleanStartIndex + content.Length;

                bool startInNoparse = ContentInNoparse(contentInfo[cleanStartIndex].index);
                bool endInNoparse = ContentInNoparse(contentInfo[cleanEndIndex].index);
                // The "rich" start index can then be inferred using the index property.
                return (contentInfo[cleanStartIndex].index, contentInfo[cleanEndIndex].index, 
                        startInNoparse, endInNoparse);
            }
            
            // Returns (rich start index, rich end index, whether the start, end of the line contains <noparse>
            (int, int, bool, bool) GetLineIndices(int line)
            {
                List<string> splitText = TextMesh.text.Split('\n').ToList();
                string lineContent = splitText[line-1];
                // We want to count newlines as well (+ line - 1)
                int indexShift = line == 1 ? 0 : splitText.GetRange(0, line - 1).SelectMany(c => c).Count() + line - 1;
                int startIndex = indexShift + lineContent.IndexOf("</color>", StringComparison.Ordinal);
                int endIndex = indexShift + lineContent.Length;
                
                return (startIndex, endIndex, ContentInNoparse(startIndex), ContentInNoparse(endIndex));
            }

            // Returns ("clean" line as a string, line info)
            async UniTask<(string, IList<TMP_CharacterInfo>)> GetLineInfo(int line)
            {
                // In order to update the displayed text, we need to temporarily switch to the main thread
                await UniTask.SwitchToMainThread();
                // This call is necessary to recompute the textInfo property with its indices
                TextMesh.ForceMeshUpdate(true, true);
                await UniTask.SwitchToThreadPool();
                
                // We get a list of CharacterInfos from the target line
                IList<TMP_CharacterInfo> contentInfo = TextMesh.textInfo.characterInfo
                                                               .SkipWhile(x => x.lineNumber+1 != line) 
                                                               .TakeWhile(x => x.lineNumber+1 == line).ToList();
                
                string lineContent = string.Concat(contentInfo.Select(x => x.character));
                return (lineContent, contentInfo);
            }

            // Returns whether or not the content at the rich index is in a <noparse> tag (by checking the line it's in)
            bool ContentInNoparse(int richIndex)
            {
                string untilContent = "";
                // This gets the beginning of the line containing the richIndex until the richIndex
                for (int i = 0; i < richIndex; i++)
                {
                    char current = TextMesh.text[i];
                    if (current == '\n')
                    {
                        // Reset on new line
                        untilContent = "";
                    }
                    untilContent += current;
                }
                // Content is in <noparse> if the tag has been opened more times than it has been closed
                return Regex.Matches(untilContent, Regex.Escape(NOPARSE)).Count
                       > Regex.Matches(untilContent, Regex.Escape(NOPARSE_CLOSE)).Count;
            }

            #endregion
        }

        /// <summary>
        /// Get index within the "rich" text (with markup tags) from index in the "clean" text (with markup tags).
        /// </summary>
        /// <param name="cleanIndex">The index within the "clean" text.</param>
        /// <returns>The index within the "rich" text.</returns>
        /// <example>
        /// Assume we have the source line <c>&lt;color red&gt;private class&lt;/color&gt; Test {</c>.
        /// As we can see, rich tags have been inserted so that the "private class" keywords are rendered in red.
        /// This is a problem when we later want to e.g. underline "class Test". It's no longer possible to simply
        /// search the text for "class Test" and underline that part, because it's broken up by <c>&lt;/color&gt;</c>.
        /// To remedy this, you can call this method with an index in the "clean" version.
        /// In our example, this would be 9 (before "class") and 19 (after "Test"). This method will then return the
        /// corresponding indices in the text with tags present, which in our example would be 20 and 38.
        /// </example>
        private int GetRichIndex(int cleanIndex)
        {
            return TextMesh.textInfo.characterInfo[cleanIndex].index;
        }

    }
}