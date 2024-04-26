using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using OmniSharp.Extensions.JsonRpc.Server;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.General;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using SEE.UI;
using SEE.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SEE.Tools.LSP
{
    /// <summary>
    /// Handles the language server process.
    ///
    /// This class is responsible for starting and stopping the language server, and is intended
    /// to be the primary interface for other classes to communicate with the language server.
    /// </summary>
    public class LSPHandler : MonoBehaviour
    {
        /// <summary>
        /// The language server to be used.
        /// This property has to be set before the language server is started.
        /// </summary>
        public LSPServer Server
        {
            get
            {
                return server ??= LSPServer.GetByName(serverName);
            }
            set
            {
                serverName = value.Name;
                server = value;
            }
        }

        /// <summary>
        /// The language server to be used.
        /// This is the backing field for the <see cref="Server"/> property.
        /// Note that this field is not serialized; for serialization, the <see cref="serverName"/> field is used.
        /// </summary>
        private LSPServer server;

        /// <summary>
        /// The name of the language server to be used.
        /// This property is only used for serialization.
        /// </summary>
        [field: SerializeField, HideInInspector]
        private string serverName;

        /// <summary>
        /// The path to the project to be analyzed.
        /// </summary>
        [field: SerializeField, HideInInspector]
        public string ProjectPath { get; set; }

        /// <summary>
        /// Whether to log the communication between the language server and SEE to a temporary file.
        /// </summary>
        [field: SerializeField, HideInInspector]
        public bool LogLSP { get; set; }

        /// <summary>
        /// The language client that is used to communicate with the language server.
        /// </summary>
        private LanguageClient Client { get; set; }

        /// <summary>
        /// The process that runs the language server.
        /// </summary>
        private Process lspProcess;

        /// <summary>
        /// A semaphore to ensure that nothing interferes with the language server while it is starting or stopping.
        /// </summary>
        private readonly SemaphoreSlim semaphore = new(1, 1);

        /// <summary>
        /// Whether the language server is ready to process requests.
        /// </summary>
        private bool IsReady { get; set; }

        /// <summary>
        /// The maximum time to wait for a response from the language server.
        /// </summary>
        public TimeSpan TimeoutSpan = TimeSpan.FromSeconds(2);

        /// <summary>
        /// The capabilities of the language server.
        /// </summary>
        public IServerCapabilities ServerCapabilities => Client?.ServerSettings.Capabilities;

        private void OnEnable()
        {
            if (Server != null)
            {
                InitializeAsync().Forget();
            }
        }

        private void OnDisable()
        {
            ShutdownAsync().Forget();
        }

        /// <summary>
        /// Initializes the language server such that it is ready to process requests.
        /// </summary>
        public async UniTask InitializeAsync(string executablePath = null, CancellationToken token = default)
        {
            if (Server == null)
            {
                throw new InvalidOperationException("LSP server must be set before initializing the handler.\n");
            }
            await semaphore.WaitAsync(token);
            if (IsReady)
            {
                semaphore.Release();
                return;
            }

            HashSet<ProgressToken> initialWork = new();
            IDisposable spinner = LoadingSpinner.ShowIndeterminate("Initializing language server...");
            try
            {
                // TODO: Check for executable (at relevant locations?) first, and if not there, direct users
                //       for info on how to install it.
                ProcessStartInfo startInfo = new(fileName: Server.ServerExecutable, arguments: Server.Parameters)
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = ProjectPath
                };
                lspProcess = Process.Start(startInfo);
                if (lspProcess == null)
                {
                    throw new InvalidOperationException("Failed to start the language server.\n");
                }

                Stream outputLog = Stream.Null;
                Stream inputLog = Stream.Null;
                if (LogLSP)
                {
                    string tempDir = Path.GetTempPath();
                    string outputPath = Path.Combine(tempDir, "outputLogLsp.txt");
                    string inputPath = Path.Combine(tempDir, "inputLogLsp.txt");
                    if (File.Exists(outputPath))
                    {
                        File.Delete(outputPath);
                    }
                    if (File.Exists(inputPath))
                    {
                        File.Delete(inputPath);
                    }
                    outputLog = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                    inputLog = new FileStream(inputPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                }

                TeeStream teedInputStream = new(lspProcess.StandardOutput.BaseStream, outputLog);
                TeeStream teedOutputStream = new(lspProcess.StandardInput.BaseStream, inputLog);

                // TODO: Add other capabilities here
                DocumentSymbolCapability symbolCapabilities = new()
                {
                    HierarchicalDocumentSymbolSupport = true
                };
                Client = LanguageClient.Create(options =>
                                                   options.WithInput(teedInputStream)
                                                          .WithOutput(teedOutputStream)
                                                          .WithRootPath(ProjectPath)
                                                          .WithCapability(symbolCapabilities)
                                                          .DisableDynamicRegistration()
                                                          .DisableWorkspaceFolders()
                                                          .WithUnhandledExceptionHandler(Debug.LogException)
                                                          .WithClientInfo(new ClientInfo
                                                          {
                                                              Name = "SEE",
                                                              Version = Application.version
                                                          })
                                                          .WithMaximumRequestTimeout(TimeoutSpan)
                                                          .WithContentModifiedSupport(false)
                                                          .WithInitializationOptions(Server.InitOptions)
                                                          .DisableProgressTokens()
                                                          .WithWorkspaceFolder(ProjectPath, "Main")
                                                          .OnWorkDoneProgressCreate(HandleInitialWorkDoneProgress));
                // Starting the server might take a little while.
                await AsyncUtils.RunWithTimeoutAsync(t => Client.Initialize(t).AsUniTask(), TimeoutSpan * 4,
                                                     throwOnTimeout: true);
                do
                {
                    // We wait until the initial work is done.
                    // We detect this by checking if any work progress notifications have been sent,
                    // and then wait until the progress is done. As soon as there are 500ms without
                    // any progress, we assume the initial work is done.
                    bool doneInTimeout = await AsyncUtils.RunWithTimeoutAsync(t => UniTask.WaitUntil(() => initialWork.Count == 0,
                                                                                                     cancellationToken: t),
                                                                              // We allow more time for the initial work to be done.
                                                                              TimeoutSpan * 4, throwOnTimeout: false);
                    await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);
                    if (!doneInTimeout)
                    {
                        break;
                    }
                } while (initialWork.Count > 0);
                IsReady = true;
            }
            finally
            {
                semaphore.Release();
                spinner.Dispose();
            }
            return;

            void HandleInitialWorkDoneProgress(WorkDoneProgressCreateParams progressParams)
            {
                if (!IsReady && progressParams.Token != null)
                {
                    initialWork.Add(progressParams.Token);
                    MonitorInitialWorkDoneProgress(progressParams.Token).Forget();
                }
            }

            async UniTaskVoid MonitorInitialWorkDoneProgress(ProgressToken token)
            {
                await foreach (WorkDoneProgress _ in Client.WorkDoneManager.Monitor(token).ToUniTaskAsyncEnumerable()
                                                           .Where(x => x.Kind == WorkDoneProgressKind.End))
                {
                    initialWork.Remove(token);
                }
            }
        }

        /// <summary>
        /// Handles the ShowMessage notification with the given <paramref name="messageParams"/>
        /// by showing a notification to the user.
        /// </summary>
        /// <param name="showMessageParams">The parameters of the ShowMessage notification.</param>
        private void ShowMessage(ShowMessageParams showMessageParams)
        {
            string languageServerName = Server?.Name ?? "Language Server";
            switch (showMessageParams.Type)
            {
                case MessageType.Error:
                    ShowNotification.Error($"{languageServerName} Error", showMessageParams.Message);
                    break;
                case MessageType.Warning:
                    ShowNotification.Warn($"{languageServerName} Warning", showMessageParams.Message);
                    break;
                case MessageType.Info:
                    ShowNotification.Info($"{languageServerName} Info", showMessageParams.Message);
                    break;
                case MessageType.Log:
                default:
                    ShowNotification.Info($"{languageServerName} Log", showMessageParams.Message);
                    break;
            }
        }

        /// <summary>
        /// Handles the LogMessage notification with the given <paramref name="messageParams"/>
        /// by logging the message to the Unity console.
        /// </summary>
        /// <param name="messageParams">The parameters of the LogMessage notification.</param>
        private static void LogMessage(LogMessageParams messageParams)
        {
            switch (messageParams.Type)
            {
                case MessageType.Error:
                    Debug.LogError(messageParams.Message);
                    break;
                case MessageType.Warning:
                    Debug.LogWarning(messageParams.Message);
                    break;
                case MessageType.Info:
                case MessageType.Log:
                default:
                    Debug.Log(messageParams.Message);
                    break;
            }
        }

        /// <summary>
        /// Opens the document at the given <paramref name="path"/> in the language server.
        ///
        /// Note that the document needs to be closed manually after it is no longer needed.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        public void OpenDocument(string path)
        {
            DidOpenTextDocumentParams parameters = new()
            {
                TextDocument = new TextDocumentItem
                {
                    Uri = DocumentUri.File(path),
                    LanguageId = Server.LanguageIdFor(Path.GetExtension(path).TrimStart('.')),
                    Version = 1,
                    Text = File.ReadAllText(path)
                }
            };
            Client.DidOpenTextDocument(parameters);
        }

        /// <summary>
        /// Closes the document at the given <paramref name="path"/> in the language server.
        /// The document needs to have been opened before.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        public void CloseDocument(string path)
        {
            DidCloseTextDocumentParams parameters = new()
            {
                TextDocument = new TextDocumentIdentifier(path)
            };
            Client.DidCloseTextDocument(parameters);
        }

        /// <summary>
        /// Retrieves the symbols in the document at the given <paramref name="path"/>.
        ///
        /// See the LSP specification for <c>textDocument/documentSymbol</c> for more information.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        /// <returns>An asynchronous enumerable that emits the symbols in the document.</returns>
        public IUniTaskAsyncEnumerable<SymbolInformationOrDocumentSymbol> DocumentSymbols(string path)
        {
            DocumentSymbolParams symbolParams = new()
            {
                TextDocument = new TextDocumentIdentifier(path),
                PartialResultToken = null
            };
            return AsyncUtils.ObserveUntilTimeout(t => Client.RequestDocumentSymbol(symbolParams, t), TimeoutSpan);
        }

        /// <summary>
        /// Retrieves hover information for the document at the given <paramref name="path"/> at the given
        /// <paramref name="line"/> and <paramref name="character"/>.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        /// <param name="line">The line number in the document.</param>
        /// <param name="character">The column in the line.</param>
        /// <returns>The hover information for the document at the given position.</returns>
        public async UniTask<Hover> HoverAsync(string path, int line, int character = 0)
        {
            HoverParams hoverParams = new()
            {
                TextDocument = new TextDocumentIdentifier(path),
                Position = new Position(line, character)
            };
            return await AsyncUtils.RunWithTimeoutAsync(t => Client.RequestHover(hoverParams, t).AsUniTask(false), TimeoutSpan, throwOnTimeout: false);
        }

        /// <summary>
        /// Retrieves all references to the symbol in the document with the given <paramref name="path"/> at the given
        /// <paramref name="line"/> and <paramref name="character"/>.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        /// <param name="line">The line number in the document.</param>
        /// <param name="character">The column in the line.</param>
        /// <param name="includeDeclaration">Whether to include the declaration of the symbol in the results.</param>
        /// <returns>An asynchronous enumerable that emits the locations of the references to the symbol.</returns>
        public IUniTaskAsyncEnumerable<LocationOrLocationLink> References(string path, int line, int character = 0, bool includeDeclaration = false)
        {
            ReferenceParams parameters = new()
            {
                TextDocument = new TextDocumentIdentifier(path),
                Position = new Position(line, character),
                Context = new ReferenceContext { IncludeDeclaration = includeDeclaration },
            };
            return GetLocationsByLspFunc<ReferenceParams, Location>(path, line, character, (_, t) => Client.RequestReferences(parameters, t)).Select(x => new LocationOrLocationLink(x));
        }

        /// <summary>
        /// Retrieves the type definition belonging to the symbol in the document with the given
        /// <paramref name="path"/> at the given <paramref name="line"/> and <paramref name="character"/>.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        /// <param name="line">The line number in the document.</param>
        /// <param name="character">The column in the line.</param>
        /// <returns>An asynchronous enumerable that emits the location of the type definition of the symbol.</returns>
        public IUniTaskAsyncEnumerable<LocationOrLocationLink> TypeDefinition(string path, int line, int character = 0)
        {
            return GetLocationsByLspFunc<TypeDefinitionParams, LocationOrLocationLink>(path, line, character, (p, t) => Client.RequestTypeDefinition(p, t));
        }

        /// <summary>
        /// Retrieves the declaration of the symbol in the document with the given <paramref name="path"/> at the given
        /// <paramref name="line"/> and <paramref name="character"/>.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        /// <param name="line">The line number in the document.</param>
        /// <param name="character">The column in the line.</param>
        /// <returns>An asynchronous enumerable that emits the location of the declaration of the symbol.</returns>
        public IUniTaskAsyncEnumerable<LocationOrLocationLink> Declaration(string path, int line, int character = 0)
        {
            return GetLocationsByLspFunc<DeclarationParams, LocationOrLocationLink>(path, line, character, (p, t) => Client.RequestDeclaration(p, t));
        }

        /// <summary>
        /// Retrieves the definition of the symbol in the document with the given <paramref name="path"/> at the given
        /// <paramref name="line"/> and <paramref name="character"/>.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        /// <param name="line">The line number in the document.</param>
        /// <param name="character">The column in the line.</param>
        /// <returns>An asynchronous enumerable that emits the location of the definition of the symbol.</returns>
        public IUniTaskAsyncEnumerable<LocationOrLocationLink> Definition(string path, int line, int character = 0)
        {
            return GetLocationsByLspFunc<DefinitionParams, LocationOrLocationLink>(path, line, character, (p, t) => Client.RequestDefinition(p, t));
        }

        /// <summary>
        /// Retrieves the implementation of the function or method in the document with the given
        /// <paramref name="path"/> at the given <paramref name="line"/> and <paramref name="character"/>.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        /// <param name="line">The line number in the document.</param>
        /// <param name="character">The column in the line.</param>
        /// <returns>An asynchronous enumerable that emits the location of the implementation of the symbol.</returns>
        public IUniTaskAsyncEnumerable<LocationOrLocationLink> Implementation(string path, int line, int character = 0)
        {
            return GetLocationsByLspFunc<ImplementationParams, LocationOrLocationLink>(path, line, character, (p, t) => Client.RequestImplementation(p, t));
        }

        /// <summary>
        /// Retrieves all outgoing calls for the symbol in the document with the given <paramref name="path"/> at the given
        /// <paramref name="line"/> and <paramref name="character"/>.
        ///
        /// In case there are multiple symbols at the given position, the <paramref name="selectItems"/> function is used
        /// to select the desired symbols.
        /// </summary>
        /// <param name="selectItems">A function that should return <c>true</c> for the desired symbols to select
        /// the outgoing calls for.</param>
        /// <param name="path">The path to the document.</param>
        /// <param name="line">The line number in the document.</param>
        /// <param name="character">The column in the line.</param>
        /// <returns>An asynchronous enumerable that emits the <see cref="CallHierarchyItem"/>s of the outgoing calls.</returns>
        public IUniTaskAsyncEnumerable<CallHierarchyItem> OutgoingCalls(Func<CallHierarchyItem, bool> selectItems,
                                                                        string path, int line, int character = 0)
        {
            CallHierarchyPrepareParams prepareParams = new()
            {
                TextDocument = new TextDocumentIdentifier(path),
                Position = new Position(line, character)
            };
            IUniTaskAsyncEnumerable<CallHierarchyItem> callItems = AsyncUtils.ObserveUntilTimeout(t => Client.RequestCallHierarchyPrepare(prepareParams, t), TimeoutSpan)
                                                                             .Where(selectItems);

            return callItems.SelectMany(item =>
            {
                CallHierarchyOutgoingCallsParams outgoingParams = new()
                {
                    Item = item
                };
                return AsyncUtils.ObserveUntilTimeout(t => Client.RequestCallHierarchyOutgoing(outgoingParams, t), TimeoutSpan).Select(x => x.To);
            });
        }

        /// <summary>
        /// Retrieves all supertypes for the symbol in the document with the given <paramref name="path"/> at the given
        /// <paramref name="line"/> and <paramref name="character"/>.
        ///
        /// In case there are multiple symbols at the given position, the <paramref name="selectItems"/> function is used
        /// to select the desired symbols.
        /// </summary>
        /// <param name="selectItems">A function that should return <c>true</c> for the desired symbols to select
        /// the supertypes for.</param>
        /// <param name="path">The path to the document.</param>
        /// <param name="line">The line number in the document.</param>
        /// <param name="character">The column in the line.</param>
        /// <returns>An asynchronous enumerable that emits the <see cref="TypeHierarchyItem"/>s of the supertypes.</returns>
        public IUniTaskAsyncEnumerable<TypeHierarchyItem> Supertypes(Func<TypeHierarchyItem, bool> selectItems, string path, int line, int character = 0)
        {
            TypeHierarchyPrepareParams prepareParams = new()
            {
                TextDocument = new TextDocumentIdentifier(path),
                Position = new Position(line, character)
            };
            IUniTaskAsyncEnumerable<TypeHierarchyItem> items = AsyncUtils.ObserveUntilTimeout(t => Client.RequestTypeHierarchyPrepare(prepareParams, t), TimeoutSpan)
                                                                         .Where(selectItems);
            return items.SelectMany(item =>
            {
                TypeHierarchySupertypesParams supertypesParams = new()
                {
                    Item = item
                };
                return AsyncUtils.ObserveUntilTimeout(t => Client.RequestTypeHierarchySupertypes(supertypesParams, t), TimeoutSpan);
            });
        }

        /// <summary>
        /// Retrieves all locations of type <typeparamref name="R"/> using the given <paramref name="lspFunction"/>.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        /// <param name="line">The line number in the document.</param>
        /// <param name="character">The column in the line.</param>
        /// <param name="lspFunction">The function to retrieve the locations.</param>
        /// <typeparam name="P">The type of the parameters for the LSP function.</typeparam>
        /// <typeparam name="R">The type of the locations to retrieve.</typeparam>
        /// <returns>An asynchronous enumerable that emits the locations of type <typeparamref name="R"/>.</returns>
        private IUniTaskAsyncEnumerable<R> GetLocationsByLspFunc<P, R>(string path, int line, int character,
                                                                       Func<P, CancellationToken, IObservable<IEnumerable<R>>> lspFunction)
            where P : TextDocumentPositionParams, new()
        {
            P parameters = new()
            {
                TextDocument = new TextDocumentIdentifier(path),
                Position = new Position(line, character),
            };
            return AsyncUtils.ObserveUntilTimeout(t => lspFunction(parameters, t), TimeoutSpan);
        }

        /// <summary>
        /// Shuts down the language server and exits its process.
        ///
        /// After this method is called, the language server is no longer
        /// ready to process requests until it is initialized again.
        /// </summary>
        public async UniTask ShutdownAsync(CancellationToken token = default)
        {
            await semaphore.WaitAsync(token);
            if (!IsReady)
            {
                // LSP server is not running.
                return;
            }

            IDisposable spinner = LoadingSpinner.Show("Shutting down language server...");
            try
            {
                if (Client != null)
                {
                    await AsyncUtils.RunWithTimeoutAsync(_ => Client.Shutdown().AsUniTask(), TimeoutSpan, throwOnTimeout: false);
                }
            }
            catch (InvalidParametersException)
            {
                // Some language servers (e.g., rust-analyzer) have trouble with OmniSharp's empty map.
                // They throw an InvalidParameterException, which we can ignore for now.
            }
            finally
            {
                // In case Client.SendExit() fails, we release the semaphore and resources first to avoid a deadlock.
                IsReady = false;
                semaphore.Release();
                spinner.Dispose();

                Client?.SendExit();
            }
        }
    }
}
