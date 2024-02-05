using Michsky.UI.ModernUIPack;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using RootMotion;
using SEE.Controls;
using SEE.Controls.Actions;
using SEE.GO;
using SEE.UI.Window;
using SEE.UI.Window.CodeWindow;
using SEE.UI.Window.ConsoleWindow;
using SEE.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Encoding = System.Text.Encoding;
using StackFrame = Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages.StackFrame;

namespace SEE.UI.DebugAdapterProtocol
{
    public class DebugAdapterProtocolSession : MonoBehaviour
    {
        private const string DebugControlsPrefab = "Prefabs/UI/DebugAdapterProtocolControls";

        public DebugAdapter.DebugAdapter Adapter;

        private ConsoleWindow console;
        private GameObject controls;
        private Process adapterProcess;
        private DebugProtocolHost adapterHost;
        private InitializeResponse capabilities;
        private Tooltip.Tooltip tooltip;

        private Queue<Action> actions = new();

        private bool isRunning;
        private List<int> threads = new();
        private int? threadId => threads.Count > 0 ? threads.First() : null;

        protected void Start()
        {
            tooltip = gameObject.AddComponent<Tooltip.Tooltip>();
            OpenConsole();
            SetupControls();

            if (Adapter == null)
            {
                LogError(new("Debug adapter not set."));
                Destroyer.Destroy(this);
                return;
            }
            console.AddMessage($"Start debugging session: {Adapter.Name} - {Adapter.AdapterFileName} - {Adapter.AdapterArguments} - {Adapter.AdapterWorkingDirectory}\n");

            if (!CreateAdapterProcess())
            {
                LogError(new("Couldn't create the debug adapter process."));
                Destroyer.Destroy(this);
                return;
            }
            else
            {
                console.AddMessage("Created the debug adapter process.\n", "Adapter", "Log");
            }
            if (!CreateAdapterHost())
            {
                LogError(new("Couldn't create the debug adapter host."));
                Destroyer.Destroy(this);
                return;
            }
            else
            {
                console.AddMessage("Created the debug adapter host.\n", "Adapter", "Log");
            }

            try
            {
                capabilities = adapterHost.SendRequestSync(new InitializeRequest()
                {
                    PathFormat = InitializeArguments.PathFormatValue.Path,
                    ClientID = "vscode",
                    ClientName = "Visual Studio Code",
                    AdapterID = Adapter.Name,
                    Locale = "en",
                    LinesStartAt1 = true,
                    ColumnsStartAt1 = true,
                });
            } catch (Exception e)
            {
                LogError(e);
                Destroyer.Destroy(this);
            }
        }

        private void SetupControls()
        {
            controls = PrefabInstantiator.InstantiatePrefab(DebugControlsPrefab, transform, false);
            controls.transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.9f, 0);

            actions.Enqueue(() =>
            {
                Dictionary<string, (bool, Action, string)> listeners = new Dictionary<string, (bool, Action, string)>
                {
                    {"Continue", (true, Continue, "Continue")},
                    {"Pause", (true, Pause, "Pause")},
                    {"Reverse", (capabilities.SupportsStepBack == true, Reverse, "Reverse")},
                    {"Next", (true, Next, "Next")},
                    {"StepBack", (capabilities.SupportsStepBack == true, StepBack, "Step Back")},
                    {"StepIn", (true, StepIn, "Step In")},
                    {"StepOut", (true, StepOut, "Step Out")},
                    {"Restart", (capabilities.SupportsRestartRequest == true, Restart, "Restart")},
                    {"Stop", (true, Stop , "Stop")},
                    {"Terminal", (true, OpenConsole, "Open the Terminal")},
                };
                foreach (var (name, (active,action, description)) in listeners)
                {
                    GameObject button = controls.transform.Find(name).gameObject;
                    button.SetActive(active);
                    button.MustGetComponent<Button>().onClick.AddListener(() => actions.Enqueue(action));
                    if (button.TryGetComponentOrLog(out PointerHelper pointerHelper))
                    {
                        pointerHelper.EnterEvent.AddListener(_ => tooltip.Show(description));
                        pointerHelper.ExitEvent.AddListener(_ => tooltip.Hide());
                    }
                }
            });
            return;

            void Continue()
            {
                if (threadId is null || isRunning) return;
                adapterHost.SendRequest(new ContinueRequest { ThreadId = (int)threadId }, _ => isRunning = true);
            }
            void Pause()
            {
                if (threadId is null || !isRunning) return;
                adapterHost.SendRequest(new ContinueRequest { ThreadId = (int)threadId }, _ => isRunning = false);
            }
            void Reverse()
            {
                if (threadId is null || isRunning) return;
                adapterHost.SendRequest(new ReverseContinueRequest { ThreadId = (int)threadId }, _ => isRunning = true);
            }
            void Next()
            {
                if (threadId is null || isRunning) return;
                adapterHost.SendRequest(new NextRequest { ThreadId = (int)threadId }, _ => isRunning = true);
            }
            void StepBack()
            {
                if (threadId is null || isRunning) return;
                adapterHost.SendRequest(new StepBackRequest { ThreadId = (int)threadId }, _ => isRunning = true);
            }
            void StepIn()
            {
                if (threadId is null || isRunning) return;
                adapterHost.SendRequest(new StepInRequest { ThreadId = (int)threadId }, _ => isRunning = true);
            }
            void StepOut()
            {
                if (threadId is null || isRunning) return;
                adapterHost.SendRequest(new StepOutRequest { ThreadId = (int)threadId }, _ => isRunning = true);
            }
            void Restart()
            {
                adapterHost.SendRequest(new RestartRequest { Arguments = Adapter.GetLaunchRequest(capabilities) }, _ => isRunning = true);
            }
            void Stop()
            {
                adapterHost.SendRequest(new DisconnectRequest(), _ => { });
            }
        }

        private void OpenConsole()
        {
            WindowSpace manager = WindowSpaceManager.ManagerInstance[WindowSpaceManager.LocalPlayer];
            if (console == null)
            {
                console = manager.Windows.OfType<ConsoleWindow>().FirstOrDefault() ?? gameObject.AddComponent<ConsoleWindow>();
                foreach ((string channel, char icon) in new[] { ("Adapter", '\uf188'), ("Debugee", '\uf135') })
                {
                    console.AddChannel(channel, icon);
                    foreach ((string level, Color color) in new[] { ("Log", Color.gray), ("Warning", Color.yellow.Darker()), ("Error", Color.red) })
                    {
                        console.AddChannelLevel(channel, level, color);
                    }
                }
                console.SetChannelLevelEnabled("Adapter", "Log", false);
                console.OnInputSubmit += OnConsoleInput;
                manager.AddWindow(console);
            }
            manager.ActiveWindow = console;
        }

        private void OnConsoleInput(string text)
        {
            Debug.Log($"On Console Input\t{text}");
        }

        private bool CreateAdapterProcess()
        {
            adapterProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Adapter.AdapterFileName,
                    Arguments = Adapter.AdapterArguments,
                    WorkingDirectory = Adapter.AdapterWorkingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    // message headers are ASCII, message bodies are UTF-8
                    StandardInputEncoding = Encoding.ASCII,
                    StandardOutputEncoding = Encoding.ASCII,
                    StandardErrorEncoding = Encoding.ASCII,
                }
            };
            adapterProcess.EnableRaisingEvents = true;
            adapterProcess.Exited += (_, args) => console.AddMessage($"Process: Exited! ({(!adapterProcess.HasExited ? adapterProcess.ProcessName : null)})");
            adapterProcess.Disposed += (_, args) => console.AddMessage($"Process: Exited! ({(!adapterProcess.HasExited ? adapterProcess.ProcessName : null)})");
            adapterProcess.OutputDataReceived += (_, args) => console.AddMessage($"Process: OutputDataReceived! ({adapterProcess.ProcessName})\n\t{args.Data}");
            adapterProcess.ErrorDataReceived += (_, args) => LogError(new($"Process: ErrorDataReceived! ({adapterProcess.ProcessName})\n\t{args.Data}"));

            string currentDirectory = Directory.GetCurrentDirectory();
            try
            {
                // working directory needs to be set manually so that executables can be found at relative paths
                Directory.SetCurrentDirectory(Adapter.AdapterWorkingDirectory);
                if (!adapterProcess.Start())
                {
                    adapterProcess = null;
                }
            }
            catch (Exception e)
            {
                LogError(e);
                adapterProcess = null;
            }
            // working directory needs to be reset (otherwise unity crashes)
            Directory.SetCurrentDirectory(currentDirectory);

            return adapterProcess != null && !adapterProcess.HasExited;
        }

        private void Update()
        {
            if (actions.Count > 0 && capabilities != null)
            {
                try
                {
                    actions.Dequeue()();
                }
                catch (Exception e)
                {
                    LogError(e);
                }
            }
        }

        private void OnEventReceived(object sender, EventReceivedEventArgs e)
        {
            if (e.Body is InitializedEvent)
            {
                actions.Enqueue(() =>
                {
                    List<Action> launchActions = Adapter.GetLaunchActions(adapterHost, capabilities);
                    Action last = launchActions.Last();
                    launchActions[launchActions.Count - 1] = () => { last(); isRunning = true; };
                    launchActions.ForEach(actions.Enqueue);
                });
            }
            else if (e.Body is OutputEvent outputEvent)
            {
                string channel = outputEvent.Category switch
                {
                    OutputEvent.CategoryValue.Console => "Adapter",
                    OutputEvent.CategoryValue.Stdout => "Debugee",
                    OutputEvent.CategoryValue.Stderr => "Debugee",
                    OutputEvent.CategoryValue.Telemetry => "Adapter",
                    OutputEvent.CategoryValue.MessageBox => "Adapter",
                    OutputEvent.CategoryValue.Exception => "Adapter",
                    OutputEvent.CategoryValue.Important => "Adapter",
                    OutputEvent.CategoryValue.Unknown => "Adapter",
                    null => "Adapter",
                    _ => "",
                };
                string? level = outputEvent.Category switch
                {
                    OutputEvent.CategoryValue.Console => "Log",
                    OutputEvent.CategoryValue.Stdout => "Log",
                    OutputEvent.CategoryValue.Stderr => "Error",
                    OutputEvent.CategoryValue.Telemetry => null,
                    OutputEvent.CategoryValue.MessageBox => "Warning",
                    OutputEvent.CategoryValue.Exception => "Error",
                    OutputEvent.CategoryValue.Important => "Warning",
                    OutputEvent.CategoryValue.Unknown => "Log",
                    null => "Log",
                    _ => "Log",
                };
                if (level is not null)
                {
                    if (level == "Error")
                    {
                        Debug.LogWarning(outputEvent.Output);
                    }
                    // FIXME: Why does it require a cast?
                    console.AddMessage(outputEvent.Output, channel, level);
                }
            }
            else if (e.Body is TerminatedEvent terminatedEvent)
            {
                // TODO: Let user restart the program.
                console.AddMessage("Terminated\n");
                actions.Enqueue(UpdateCodePosition);
                actions.Enqueue(() => Destroyer.Destroy(this));
            }
            else if (e.Body is ExitedEvent exitedEvent)
            {
                isRunning = false;

                console.AddMessage($"Exited with exit code {exitedEvent.ExitCode}\n", "Debugee", "Log");
                actions.Enqueue(() => Destroyer.Destroy(this));
            }
            else if (e.Body is StoppedEvent stoppedEvent)
            {
                isRunning = false;
                actions.Enqueue(UpdateCodePosition);
            }
            else if (e.Body is ThreadEvent threadEvent)
            {
                if (threadEvent.Reason == ThreadEvent.ReasonValue.Started)
                {
                    threads.Add(threadEvent.ThreadId);
                } else if (threadEvent.Reason == ThreadEvent.ReasonValue.Exited)
                {
                    threads.Remove(threadEvent.ThreadId);
                }
            } else if (e.Body is ContinuedEvent)
            {
                isRunning = true;
            }
        }

        private void UpdateCodePosition()
        {
            if (threadId == null) return;
            StackFrame stackFrame = adapterHost.SendRequestSync(new StackTraceRequest()
            {
                ThreadId = (int)threadId,
                Levels = 1
            }).StackFrames[0];
            string path = stackFrame.Source.Path;
            int line = stackFrame.Line;
            string title = Path.GetFileName(path);

            WindowSpace manager = WindowSpaceManager.ManagerInstance[WindowSpaceManager.LocalPlayer];
            CodeWindow codeWindow = manager.Windows.OfType<CodeWindow>().FirstOrDefault(window => window.Title == title);
            if (codeWindow == null)
            {
                codeWindow = gameObject.AddComponent<CodeWindow>();
                codeWindow.Title = title;
                codeWindow.EnterFromFile(path);
                manager.AddWindow(codeWindow);
                codeWindow.OnComponentInitialized += () =>
                {
                    codeWindow.ScrolledVisibleLine = line;
                };
            } else
            {
                codeWindow.ScrolledVisibleLine = line;
            }
            manager.ActiveWindow = codeWindow;
        }

        private bool CreateAdapterHost()
        {
            adapterHost = new DebugProtocolHost(adapterProcess.StandardInput.BaseStream, adapterProcess.StandardOutput.BaseStream);
            adapterHost.DispatcherError += (sender, args) => LogError(new($"DispatcherError - {args.Exception}"));
            adapterHost.ResponseTimeThresholdExceeded += (_, args) => console.AddMessage($"ResponseTimeThresholdExceeded - \t{args.Command}\t{args.SequenceId}\t{args.Threshold}\n", "Adapter", "Warning");
            adapterHost.EventReceived += OnEventReceived;
            adapterHost.Run();

            return adapterHost.IsRunning;
        }

        private void OnDestroy()
        {
            console.AddMessage("Debug session finished.\n");
            actions.Clear();
            if (console)
            {
                console.OnInputSubmit -= OnConsoleInput;
            }
            if (controls)
            {
                Destroyer.Destroy(controls);
            }
            if (tooltip)
            {
                Destroyer.Destroy(tooltip);
            }
            if (adapterHost != null && adapterHost.IsRunning)
            {
                adapterHost.Stop();
            }
            if (adapterProcess != null && !adapterProcess.HasExited)
            {
                adapterProcess.Close();
            }
        }

        private void LogError(Exception e)
        {
            console.AddMessage(e.ToString() + "\n", "Adapter", "Error");
            throw e;
        }
    }
}