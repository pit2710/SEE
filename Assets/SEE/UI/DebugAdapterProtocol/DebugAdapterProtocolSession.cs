using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using SEE.Controls;
using SEE.GO;
using SEE.UI.Window;
using SEE.UI.Window.ConsoleWindow;
using SEE.Utils;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Encoding = System.Text.Encoding;

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
        private bool isInitialized;


        protected void Start()
        {
            OpenConsole();
            SetupControls();

            if (Adapter == null)
            {
                Debug.LogError("Debug adapter not set.");
                Destroyer.Destroy(this);
                return;
            }
            console.AddMessage($"Start debugging session: {Adapter.Name} - {Adapter.AdapterFileName} - {Adapter.AdapterArguments} - {Adapter.AdapterWorkingDirectory}");

            if (!CreateAdapterProcess())
            {
                console.AddMessage("Couldn't create the debug adapter process.", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Error);
                Destroyer.Destroy(this);
                return;
            } else
            {
                console.AddMessage("Created the debug adapter process.", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Log);
            }
            if (!CreateAdapterHost())
            {
                console.AddMessage("Couldn't create the debug adapter host.", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Error);
                Destroyer.Destroy(this);
                return;
            } else
            {
                console.AddMessage("Created the debug adapter host.", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Log);
            }

            // FIXME: Sometimes the "initialized" event occurs before the "initialize" response
            // normal order: initialize request -> initialize response, intialized event -> launch request
            capabilities = adapterHost.SendRequestSync(new InitializeRequest()
            {
                ClientID = "SEE",
                ClientName = "Software Engineering Experience",
                AdapterID = Adapter.Name,
                PathFormat = InitializeArguments.PathFormatValue.Path,
            });
            console.AddMessage("Capabilities\t" + capabilities);
            Launch();
        }

        private void SetupControls()
        {
            controls = PrefabInstantiator.InstantiatePrefab(DebugControlsPrefab, transform, false);
            controls.transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.9f, 0);

            Button terminalButton = controls.transform.Find("Terminal").gameObject.MustGetComponent<Button>();
            terminalButton.onClick.AddListener(OpenConsole);

            Button stopButton = controls.transform.Find("Stop").gameObject.MustGetComponent<Button>();
            stopButton.onClick.AddListener(() => Destroyer.Destroy(this));
        }

        private void OpenConsole()
        {
            WindowSpace manager = WindowSpaceManager.ManagerInstance[WindowSpaceManager.LocalPlayer];
            if (console == null)
            {
                console = gameObject.AddComponent<ConsoleWindow>();
                console.AddMessage("Console created");
                manager.AddWindow(console);
            }
            console.AddMessage("Console opened");
            manager.ActiveWindow = console;
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
            adapterProcess.Exited += (_, args) => console.AddMessage($"Process: Exited! ({adapterProcess.ProcessName}", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Error);
            adapterProcess.Disposed += (_, args) => console.AddMessage($"Process: Exited! ({adapterProcess.ProcessName}", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Error);
            adapterProcess.ErrorDataReceived += (_, args) => console.AddMessage($"Process: ErrorDataReceived! ({adapterProcess.ProcessName}\t{args.Data}", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Error);
            adapterProcess.OutputDataReceived += (_, args) => console.AddMessage($"Process: OutputDataReceived! ({adapterProcess.ProcessName}\t{args.Data}", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Error);


            string currentDirectory = Directory.GetCurrentDirectory();
            // working directory needs to be set manually so that executables can be found
            Directory.SetCurrentDirectory(Adapter.AdapterWorkingDirectory);
            try
            {
                if (!adapterProcess.Start())
                {
                    adapterProcess = null;
                }
            } catch (Exception e) {
                console.AddMessage(e.ToString(), ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Error);
                adapterProcess = null;
            }
            // working directory needs to be reset (otherwise unity crashes)
            Directory.SetCurrentDirectory(currentDirectory);

            return adapterProcess != null && !adapterProcess.HasExited;
        }

        private void Launch()
        {
            // safe guard to prevent launching the debugee before knowing its capabilities
            if (capabilities == null || !isInitialized) return;

            console.AddMessage("Launch");
            adapterHost.SendRequest(Adapter.GetLaunchRequest(capabilities), _ => { });
        }

        private void OnEventReceived(object sender, EventReceivedEventArgs e)
        {
            if (e.Body is InitializedEvent) {
                isInitialized = true;
                Launch();
            } else if (e.Body is OutputEvent outputEvent) {
                console.AddMessage(outputEvent.Output, ConsoleWindow.MessageSource.Debugee, ConsoleWindow.MessageLevel.Log);
            }
        }

        private bool CreateAdapterHost()
        {
            adapterHost = new DebugProtocolHost(adapterProcess.StandardInput.BaseStream, adapterProcess.StandardOutput.BaseStream);
            adapterHost.LogMessage += (sender, args) => console.AddMessage($"LogMessage - {args.Category} - {args.Message}", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Log);
            adapterHost.DispatcherError += (sender, args) => console.AddMessage($"DispatcherError - {args.Exception}", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Error);
            adapterHost.ResponseTimeThresholdExceeded += (_, args) => console.AddMessage($"ResponseTimeThresholdExceeded - \t{args.Command}\t{args.SequenceId}\t{args.Threshold}", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Warning);
            adapterHost.EventReceived += (_, args) => console.AddMessage($"EventReceived - {args.EventType}", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Log);
            adapterHost.RequestReceived += (_, args) => console.AddMessage($"RequestReceived - {args.Command}", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Log);
            adapterHost.RequestCompleted += (_, args) => console.AddMessage($"RequestCompleted - {args.Command} - {args.SequenceId} - {args.ElapsedTime}", ConsoleWindow.MessageSource.Adapter, ConsoleWindow.MessageLevel.Log);

            adapterHost.EventReceived += OnEventReceived;
            
            adapterHost.Run();

            return adapterHost.IsRunning;
        }



        private void OnDestroy()
        {
            console.AddMessage("Debug session finished.");
            if (controls)
            {
                Destroyer.Destroy(controls);
            }
            if (adapterProcess != null && !adapterProcess.HasExited)
            {
                adapterProcess.Kill();
            }
            if (adapterHost != null && adapterHost.IsRunning)
            {
                adapterHost.Stop();
            }
        }

    }
}