using System;
using RadialMenuPlugin.Data;
using RadialMenuPlugin.Utilities.Settings;

namespace RadialMenuPlugin.Settings
{
    /// <summary>
    /// 
    /// </summary>
    public class PluginSettingsDialog : BaseDialog<PluginSettingsDialogContents, SettingsClass>
    {
        protected PluginSettingsDialogContents _PluginEditorContents;
        public PluginSettingsDialog() : base()
        {

            _PluginEditorContents = new PluginSettingsDialogContents();
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (SettingsHelper.Instance.GetSettingsRoot(SettingsDomain.GeneralSettings, out var settings, false))
            { 
                
            }
        }
    }
}