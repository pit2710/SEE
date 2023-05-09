using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using JetBrains.Annotations;
using SEE.Game.UI.LiveDocumantation;

namespace SEE.Utils.LiveDocumentation
{
    public class CSharpExtractor : Extractor
    {
        private CommonTokenStream _tokens;
        private CommonTokenStream _commentTokens;
        private CSharpParser _parser;

        public CSharpExtractor(string fileName)
        {
            var input = File.ReadAllText(fileName);
            var lexer = new CSharpFullLexer(new AntlrInputStream(input));
            _tokens = new CommonTokenStream(lexer);
            _tokens.Fill();
            _parser = new CSharpParser(_tokens);

            var lexer1 = new CSharpFullLexer(new AntlrInputStream(input));


            _commentTokens = new CommonTokenStream(lexer1, 2);
            _commentTokens.Fill();
        }


        /// <summary>
        /// Uses the <see cref="CSharpParser"/> to parse a source file to find a speciffic class.
        /// </summary>
        /// <param name="parser">The parses which should be used for that</param>
        /// <param name="className">The name of the class which should be found</param>
        /// <returns>The declaration_context of the class and the upper bound of line numbers. If the class cant by found under that name null is returned.
        /// (Context, LineNumberBound)
        /// </returns>
        [CanBeNull]
        private (CSharpParser.Type_declarationContext, int) GetClassByName(string className)
        {
            _parser.Reset();
            var namespaces = _parser
                .compilation_unit()
                .namespace_member_declarations()
                .namespace_member_declaration();
            foreach (var namspace in namespaces)
            {
                var namespaceDefLine = namspace.namespace_declaration().Start.Line;
                var types = namspace
                    .namespace_declaration()
                    .namespace_body()
                    .namespace_member_declarations()
                    .namespace_member_declaration()
                    .ToList();
                foreach (var type in types)
                {
                    // Extract the name of the class or struct and compare it with the passed className
                    if ((type.type_declaration().class_definition() is { } c &&
                         c.identifier().GetText().Equals(className)) ||
                        (type.type_declaration().struct_definition() is { } s &&
                         s.identifier().GetText().Equals(className)))
                    {
                        var classIndex = types.IndexOf(type);
                        var classDocMinStartLine = namespaceDefLine;
                        // Get The end line of the last class
                        if (classIndex > 0)
                        {
                            classDocMinStartLine = types[classIndex - 1].Stop.Line;
                        }

                        return (type.type_declaration(), classDocMinStartLine);
                    }
                }
            }

            return (null, -1);
        }


        /// <summary>
        /// This method will extract the documentation of an given C# Class in a given file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public LiveDocumentationBuffer ExtractClassComments(string fileName, string className)
        {
            _parser.Reset();
            LiveDocumentationBuffer buffer = new();


            var (classContext, upperLineBound) = GetClassByName(className);

            if (classContext != null)
            {
                // Combining the Type 2 Tokens (C# Class Documentation) between the start of the class and the
                // start of the namespace, if the class is the first in the file, or between the start of the class and the
                // end of the last type declaration
                var commentTokens = String.Join(Environment.NewLine, _commentTokens.GetTokens()
                    .Where(x => x.Type == 2 && x.Line < classContext.Start.Line && x.Line > upperLineBound)
                    .Select(x => x.Text).ToList());

                // Parsing of the class documentation
                var lexer = new CSharpCommentsGrammarLexer(new AntlrInputStream(commentTokens));
                var tokens = new CommonTokenStream(lexer);
                tokens.Fill();
                if (tokens.GetTokens().Count > 1)
                {
                    var parser = new CSharpCommentsGrammarParser(tokens);
                    // var t = parser.start().children;
                    ProcessSummary(buffer, parser.docs().summary());
                    // Parse C# Doc line by line and write it in the Buffer
                }
            }

            return buffer;
        }

        private void ProcessMethodParameters(LiveDocumentationBuffer bufff,
            CSharpParser.Formal_parameter_listContext parameterList)
        {
            bufff.Add(new LiveDocumentationBufferText("("));
            if (parameterList != null)
            {
                var parameters = parameterList
                    .fixed_parameters()
                    .fixed_parameter();
                foreach (var parameter in parameters)
                {
                    var pType = parameter.arg_declaration().type_().GetText() + " ";
                    bufff.Add(new LiveDocumentationLink(pType, pType));
                    //signature = signature + parameter.arg_declaration().type_().GetText() + " ";
                    //  signature = signature + parameter.arg_declaration().identifier().GetText();
                    bufff.Add(
                        new LiveDocumentationBufferText(parameter.arg_declaration().identifier()
                            .GetText()));
                    if (!parameter.Equals(parameters.Last()))
                    {
                        bufff.Add(new LiveDocumentationBufferText(", "));
                    }
                }
                //   signature = signature + "(" + i.common_member_declaration().method_declaration()
                //     .formal_parameter_list().GetText() + ")";
            }

            bufff.Add(new LiveDocumentationBufferText(")"));
        }

        private void ProcessSummary(LiveDocumentationBuffer buffer,
            CSharpCommentsGrammarParser.SummaryContext summaryContext)
        {
            foreach (var commentLine in summaryContext.comments())
            {
                ProcessComment(buffer, commentLine);

                buffer.Add(new LiveDocumentationBufferText("\n"));
            }
        }

        private void ProcessComment(LiveDocumentationBuffer buffer,
            CSharpCommentsGrammarParser.CommentsContext commentsContext)
        {
            foreach (var i in commentsContext.children)
            {
                if (i is CSharpCommentsGrammarParser.SomeTextContext someTextContext)
                {
                    buffer.Add(new LiveDocumentationBufferText(someTextContext.TEXT()
                        .Aggregate("", (text, word) => text += ((word.GetText()) + " ")).TrimEnd()));
                }
                else if (i is CSharpCommentsGrammarParser.ClassLinkContext classLinkContext)
                {
                    buffer.Add(new LiveDocumentationLink(classLinkContext.linkID.Text,
                        classLinkContext.linkID.Text));
                }
            }
        }

        private void ProcessTagContent(LiveDocumentationBuffer buffer,
            CSharpCommentsGrammarParser.TagContentContext tagContentContext)
        {
            foreach (var i in tagContentContext.children)
            {
                if (i is CSharpCommentsGrammarParser.SomeTextContext someTextContext)
                {
                    buffer.Add(new LiveDocumentationBufferText(someTextContext.TEXT()
                        .Aggregate("", (text, word) => text += ((word.GetText()) + " ")).TrimEnd()));
                }
                else if (i is CSharpCommentsGrammarParser.CommentsContext commentsContext)
                {
                    ProcessComment(buffer, commentsContext);
                }
            }
        }

        private List<LiveDocumentationBuffer> ProcessParamaters(
            CSharpParser.Formal_parameter_listContext parameterListContext, CSharpCommentsGrammarParser parser)
        {
            List<LiveDocumentationBuffer> bufferList = new List<LiveDocumentationBuffer>();

            foreach (var parameter in parameterListContext.fixed_parameters().fixed_parameter())
            {
                parser.Reset();
                LiveDocumentationBuffer buffer = new LiveDocumentationBuffer();
                var parameterName = parameter.arg_declaration().identifier().GetText();
                buffer.Add(new LiveDocumentationBufferText(parameterName + " : "));
                var parameterType = parameter.arg_declaration().type_().GetText();
                buffer.Add(new LiveDocumentationLink(parameterType, parameterType));
                ProcessTagContent(buffer, parser.docs().parameters()
                    .parameter().First(x => x.paramName.Text == parameterName).parameterDescription);
                //buffer.Add(new LiveDocumentationBufferText(parser.docs().parameters()
                 //   .parameter().First(x => x.paramName.Text == parameterName).parameterDescription.GetText()));
                bufferList.Add(buffer);
            }

            return bufferList;
        }

        public List<LiveDocumentationClassMemberBuffer> ExtractMethods(string fileName, string className)
        {
            List<LiveDocumentationClassMemberBuffer> buffers = new();
            _parser.Reset();

            var (classContext, upperLineBound) = GetClassByName(className);
            // Parse all namespaces and iterate over them

            int methodUpperLineBound = upperLineBound;

            foreach (var i in classContext.class_definition().class_body().class_member_declarations()
                         .class_member_declaration())
            {
                i.GetText();
                LiveDocumentationClassMemberBuffer methodBuffer = new LiveDocumentationClassMemberBuffer();
                methodBuffer.LineNumber = i.common_member_declaration().Start.Line;
                // Concatenate the Method signature together.
                // Starting by the modifiers of the method
                var signature = i.all_member_modifiers().GetText() + " ";
                // If the current class member is a method

                // If the class member is a method
                if (i.common_member_declaration().method_declaration() is { } methodDeclarationContext)
                {
                    var commentTokens = String.Join(Environment.NewLine, _commentTokens.GetTokens()
                        .Where(x => x.Type == 2 && x.Line < i.Start.Line && x.Line > methodUpperLineBound)
                        .Select(x => x.Text).ToList());

                    var lexer = new CSharpCommentsGrammarLexer(new AntlrInputStream(commentTokens));
                    var tokens = new CommonTokenStream(lexer);
                    tokens.Fill();
                    var parser = new CSharpCommentsGrammarParser(tokens);
                    LiveDocumentationBuffer methodDoc = new LiveDocumentationBuffer();
                    if (tokens.GetTokens().Count > 1)
                    {
                        ProcessSummary(methodDoc, parser.docs().summary());
                        methodBuffer.Documentation = methodDoc;
                        parser.Reset();
                        List<LiveDocumentationBuffer> methodsDocumentation =
                            new List<LiveDocumentationBuffer>();
                        foreach (var methodParameter in parser.docs().parameters().parameter())
                        {
                            LiveDocumentationBuffer methodParameterBuffer = new LiveDocumentationBuffer();

                            methodParameterBuffer.Add(
                                new LiveDocumentationBufferText(methodParameter.paramName.Text + " "));
                            ProcessTagContent(methodParameterBuffer, methodParameter.parameterDescription);
                        }
                    }

                    methodBuffer.Parameters =
                        ProcessParamaters(methodDeclarationContext.formal_parameter_list(), parser);

                    // Add the name of the method to the buffer
                    signature +=
                        methodDeclarationContext
                            .method_member_name().GetText();
                    methodBuffer.Add(new LiveDocumentationBufferText(signature));

                    ProcessMethodParameters(methodBuffer,
                        i.common_member_declaration().method_declaration().formal_parameter_list());
                }
                // If the class member is a constructor
                else if (i.common_member_declaration().constructor_declaration() is
                         { } constructorDeclarationContext)
                {
                    var commentTokens = String.Join(Environment.NewLine, _commentTokens.GetTokens()
                        .Where(x => x.Type == 2 && x.Line < i.Start.Line && x.Line > methodUpperLineBound)
                        .Select(x => x.Text).ToList());

                    var lexer = new CSharpCommentsGrammarLexer(new AntlrInputStream(commentTokens));
                    var tokens = new CommonTokenStream(lexer);
                    tokens.Fill();
                    var parser = new CSharpCommentsGrammarParser(tokens);
                    LiveDocumentationBuffer methodDoc = new LiveDocumentationBuffer();
                    if (tokens.GetTokens().Count > 1)
                    {
                        ProcessSummary(methodDoc, parser.docs().summary());
                        methodBuffer.Documentation = methodDoc;
                        parser.Reset();
                        List<LiveDocumentationBuffer> methodsDocumentation =
                            new List<LiveDocumentationBuffer>();
                        foreach (var methodParameter in parser.docs().parameters().parameter())
                        {
                            LiveDocumentationBuffer methodParameterBuffer = new LiveDocumentationBuffer();

                            methodParameterBuffer.Add(
                                new LiveDocumentationBufferText(methodParameter.paramName.Text + " "));
                            ProcessTagContent(methodParameterBuffer, methodParameter.parameterDescription);
                        }
                    }
                    parser.Reset();
                    methodBuffer.Parameters =
                        ProcessParamaters(constructorDeclarationContext.formal_parameter_list(), parser);

                    signature += constructorDeclarationContext.identifier().GetText();
                    methodBuffer.Add(new LiveDocumentationBufferText(signature));

                    ProcessMethodParameters(methodBuffer,
                        i.common_member_declaration().constructor_declaration().formal_parameter_list());
                }
                else
                {
                    methodUpperLineBound = i.Stop.Line;
                    continue;
                }


                methodUpperLineBound = i.Stop.Line;
                buffers.Add(methodBuffer);
            }


            return buffers;
        }


        /// <summary>
        /// Extracts all using directives from a given C# file specified in <paramref name="fileName"/>.
        ///
        /// It will accumulate all imported type names in an list and return it.
        /// If the file doesn't have any using directives an empty list is returned.
        /// </summary>
        /// <param name="fileName">The name of the file in which the using directives should be extracted</param>
        /// <returns>A <see cref="List"/> of imported type names</returns>
        public List<string> ExtractImportedNamespaces(string fileName)
        {
            var input = File.ReadAllText(fileName);
            var lexer = new CSharpFullLexer(new AntlrInputStream(input));
            var tokens = new CommonTokenStream(lexer);
            tokens.Fill();
            var parser = new CSharpParser(tokens);
            List<string> ret = new();
            // If the file doesn't have any using directives an empty list is returned.
            if (parser.compilation_unit().using_directives() is { } usingDirectivesContext)
            {
                foreach (var usingDirective in usingDirectivesContext.using_directive())
                {
                    if (usingDirective is CSharpParser.UsingNamespaceDirectiveContext namespaceDirectiveContext)
                    {
                        ret.Add(namespaceDirectiveContext.namespace_or_type_name().GetText());
                    }
                }
            }

            return ret;
        }
    }
}