using Eto.Drawing;
using Eto.Forms;
using RadialMenuPlugin.Data;
using System;

namespace RadialMenuPlugin.Controls.ContextMenu.MenuButton
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    public class ButtonSettingEditorForm : Base.ContextMenuForm<ButtonSettingEditorContents, Model>
    // public class ButtonSettingEditorForm : Form
    {
        protected ButtonSettingEditorContents _SettingsEditorContents;
        public event EventHandler<TextChangingEventArgs> TriggerTextChanging;
        public ButtonSettingEditorForm() : base()
        {
            Size = new Size(450, 250);
            _SettingsEditorContents = new ButtonSettingEditorContents();
            _SettingsEditorContents.TextChanging += (s, e) =>
            {
                TriggerTextChanging?.Invoke(this, e);
            };
            Content = _SettingsEditorContents;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public ButtonSettingEditorForm(Model data) : this()
        {
            Model = data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        public void Show(Point location)
        {
            Location = location;
            Show();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            Focus();
            base.OnShown(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool _ShouldClose()
        {
            return !_SettingsEditorContents.FileSelectorOpened;
        }
    }
}