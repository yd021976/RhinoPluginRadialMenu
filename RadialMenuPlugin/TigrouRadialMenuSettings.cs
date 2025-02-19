using NLog;
using RadialMenuPlugin.Settings;
using Rhino;
using Rhino.Commands;

namespace RadialMenuPlugin
{
    public class RadialMenuSettings : Command
    {
        protected PluginSettingsDialog SettingsDialog = new PluginSettingsDialog();
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        ///<summary>The only instance of this command.</summary>
        public static RadialMenuSettings Instance { get; private set; }

        /// <summary>
        /// Maintain list of plugin form instance associated to each opened Rhino doc (Window)
        /// </summary>
        public RadialMenuSettings()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
        

        /// <summary>
        /// Name of the command in Rhino
        /// </summary>
        /// <returns>
        /// The command name as it appears on the Rhino command line.
        /// </returns>
        public override string EnglishName => "TigrouRadialMenuSettings";

        /// <summary>
        /// Shows radial menu at mouse cursor position
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            SettingsDialog.ShowModal();
            return Result.Nothing;
        }
    }
}