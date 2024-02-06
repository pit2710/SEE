using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using SEE.UI.PropertyDialog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SEE.UI.DebugAdapterProtocol.DebugAdapter
{
    public abstract class DebugAdapter
    {
        protected static readonly string AdapterDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Adapters"));

        public abstract string Name { get; }
        public abstract string AdapterWorkingDirectory { get; set; }
        public abstract string AdapterFileName { get; set; }
        public abstract string AdapterArguments { get; set; }


        public abstract void SetupLaunchConfig(GameObject go, PropertyGroup group);
        public abstract void SaveLaunchConfig();

        public abstract LaunchRequest GetLaunchRequest(InitializeResponse capabilities);

        public virtual void Launch(DebugProtocolHost adapterHost, InitializeResponse capabilities)
        {
            if (capabilities.SupportsConfigurationDoneRequest == true)
            {
                adapterHost.SendRequestSync(new ConfigurationDoneRequest());
            }
            adapterHost.SendRequestSync(GetLaunchRequest(capabilities));
        }
    }
}