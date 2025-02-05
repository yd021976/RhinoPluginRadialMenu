using System;
using System.Diagnostics;
using NLog;
using NLog.Layouts;
using RadialMenuPlugin.Utilities.Settings;
using Rhino;
using Rhino.PlugIns;

namespace RadialMenuPlugin
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class RadialMenuPlugin : PlugIn
    {
        public static readonly string IsDebugEnvName = "DEBUG_PLUGIN";
        public SettingsHelper SettingsHelper;
        public RadialMenuPlugin()
        {
            Instance = this;
            SettingsHelper = new SettingsHelper(this);
        }

        ///<summary>Gets the only instance of the FirstRhinopluginPlugin plug-in.</summary>
        public static RadialMenuPlugin Instance { get; private set; }

        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and maintain plug-in wide options in a document.
        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            InitNLog();
            return LoadReturnCode.Success;
        }
        /// <summary>
        /// 
        /// </summary>
        protected void InitNLog()
        {
            // configure NLog logger
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = SettingsDirectoryAllUsers + "/logfile.txt";
            var filetarget = new NLog.Targets.FileTarget("logfile") { FileName = logfile };
            var n = System.Reflection.Assembly.GetExecutingAssembly();
            var consoleTargetDebug = new NLog.Targets.MethodCallTarget()
            {
                Name = "RhinoCommandHistory",
                ClassName = "RadialMenuPlugin.NLogClassLogger, RadialMenu, Version=1.1.1.0, Culture=neutral, PublicKeyToken=null",
                MethodName = "log",
            };
            consoleTargetDebug.Parameters.Add(new NLog.Targets.MethodCallParameter("${level}"));
            consoleTargetDebug.Parameters.Add(new NLog.Targets.MethodCallParameter("${callsite:includeNamespace=false}"));
            consoleTargetDebug.Parameters.Add(new NLog.Targets.MethodCallParameter("${message}"));
            config.AddRule(LogLevel.Info, LogLevel.Fatal, filetarget); // Log all except debug and trace level into file

            // Log trace and debug level into rhino console ONLY when in debug mode
            var isDebug = Environment.GetEnvironmentVariable(RadialMenuPlugin.IsDebugEnvName);
            if (isDebug == "1")
            {
                config.AddRule(LogLevel.Trace, LogLevel.Debug, consoleTargetDebug);
            }
            LogManager.Configuration = config;
        }
    }
    public class NLogClassLogger
    {

        public static void log(string level, string callsite, string message)
        {
            Console.SetOut(RhinoApp.CommandLineOut);
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var threadID = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            Console.WriteLine($"{level}|{timestamp}|#{threadID}|{callsite}|{message}");
        }
    }
}