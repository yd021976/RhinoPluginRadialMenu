using System.ComponentModel;
using Eto.Forms;
using Eto.Drawing;
using RadialMenuPlugin.Data;
using AppKit;
using System.Collections.Generic;
using RadialMenuPlugin.Controls.ContextMenu.Base.Editors;
using System;

namespace RadialMenuPlugin.Controls.ContextMenu
{
    /// <summary>
    /// 
    /// </summary>
    public class ButtonSettingEditorForm : Base.ContextMenuForm<ButtonSettingEditorContents, Model>
    {
        public ButtonSettingEditorForm() : base()
        {
            Size = new Size(150, 150);
            Content = new ButtonSettingEditorContents();
        }
        public ButtonSettingEditorForm(Model data) : this()
        {
            Model = data;
        }
        public void Show(Point location)
        {
            Location = location;
            Show();
        }
        protected override void OnShown(EventArgs e)
        {
            Focus();
            base.OnShown(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ButtonSettingEditorContents : Base.ContextMenuContent<Model>
    {
        protected TextBox _TriggerEditor;
        public ButtonSettingEditorContents() : base()
        {
            Orientation = Orientation.Vertical;
            Padding = new Padding(16);
            _InitTriggerEditor();

            var triggerStackField = new StackField("Trigger", _TriggerEditor);
            Items.Add(new StackLayoutItem(triggerStackField));
        }
        /// <summary>
        /// Update binding when Model object is updated
        /// </summary>
        protected override void _UpdateModelBindings()
        {
            _TriggerEditor.Unbind();
            _TriggerEditor.TextBinding.Bind(_Model.Data.Properties, nameof(_Model.Data.Properties.Trigger));
        }
        protected override void _ModelChangedHandler(object sender, PropertyChangedEventArgs e)
        { }
        protected override void OnShown(EventArgs e)
        {
            _TriggerEditor.Focus();
            base.OnShown(e);
        }
        /// <summary>
        /// 
        /// </summary>
        protected void _InitTriggerEditor()
        {
            _TriggerEditor = new TextBox();
            _TriggerEditor.MaxLength = 1;
            _TriggerEditor.ShowBorder = false;
            _TriggerEditor.Size = new Size(25, 20);
            _TriggerEditor.TextChanged += (s, e) =>
            {
                _TriggerEditor.Text = _TriggerEditor.Text.ToUpper();
            };
        }
    }
}