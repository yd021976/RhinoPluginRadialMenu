using System.ComponentModel;
using Eto.Forms;
using Eto.Drawing;
using RadialMenuPlugin.Data;
using AppKit;
using System.Collections.Generic;

namespace RadialMenuPlugin.Controls.ContextMenu
{
    /// <summary>
    /// 
    /// </summary>
    public class ButtonSettingEditorForm : Base.ContextMenuForm<ButtonSettingEditorContents, Model>
    {
        public ButtonSettingEditorForm() : base()
        {
            Size = new Size(150, 200);
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
    }

    /// <summary>
    /// 
    /// </summary>
    public class ButtonSettingEditorContents : Base.ContextMenuContent<Model>
    {
        public ButtonSettingEditorContents() : base()
        {
            Orientation = Orientation.Vertical;
            Padding = new Padding(16);

            var isFolderEditor = new StackLayout();
            isFolderEditor.Orientation = Orientation.Horizontal;
            isFolderEditor.VerticalContentAlignment = VerticalAlignment.Center;
            isFolderEditor.Padding = new Padding(16, 0);
            isFolderEditor.Spacing = 32;

            var isFolderLabel = new Label(); isFolderLabel.Text = "Is folder";
            var isFolderValue = new CheckBox(); isFolderValue.CheckedBinding.Bind(_Model.Data.Properties, obj => obj.IsFolder);
            isFolderEditor.Items.Add(new StackLayoutItem(isFolderLabel));
            isFolderEditor.Items.Add(new StackLayoutItem(isFolderValue));

            Items.Add(new StackLayoutItem(isFolderEditor));
        }
        protected override void _ModelChangedHandler(object sender, PropertyChangedEventArgs e)
        {

        }
    }
}