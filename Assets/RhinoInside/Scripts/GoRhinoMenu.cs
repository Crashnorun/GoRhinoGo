
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
//using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using UnityEditor;
using UnityEngine;
using UnityEngine.Diagnostics;
using Rhino;
using Rhino.Runtime.InProcess;
using Rhino.Geometry;
using Rhino.PlugIns;

using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

public class MenuItems
{
    public Rhino.Runtime.InProcess.RhinoCore rhinoCore;

    [MenuItem("GoRhinoGo/Launch Grasshopper")]
    private static void NewMenuOption()
    {
        ResolveEventHandler OnRhinoCommonResolve = null;
        AppDomain.CurrentDomain.AssemblyResolve += OnRhinoCommonResolve = (sender, args) =>
        {
            const string rhinoCommonAssemblyName = "RhinoCommon";
             var assemblyName = new AssemblyName(args.Name).Name;

            if (assemblyName != rhinoCommonAssemblyName)
                return null;

            AppDomain.CurrentDomain.AssemblyResolve -= OnRhinoCommonResolve;

            string rhinoSystemDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rhino WIP", "System");
            return Assembly.LoadFrom(Path.Combine(rhinoSystemDir, rhinoCommonAssemblyName + ".dll"));
        };

        //rhinoCore = new RhinoCore(new string[] { $"/scheme=UNITY", "/nosplash", "/runscript=\"_Grasshopper\"" }, WindowStyle.Normal);
        //rhinoCore = new RhinoCore(new string[] { $"/scheme=UNITY", "/nosplash" }, WindowStyle.Normal);
    }
}
#endif

