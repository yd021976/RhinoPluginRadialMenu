using System.ComponentModel;
using Eto.Forms;
using Eto.Drawing;
using RadialMenuPlugin.Data;

namespace RadialMenuPlugin.Controls.ContextMenu
{
    /// <summary>
    /// 
    /// </summary>
    public class ButtonSettingEditorForm : Base.ContextMenuForm<ButtonSettingEditorContents, Model>
    {
        public ButtonSettingEditorForm(Model data) : base()
        {
            _Contents = new ButtonSettingEditorContents();
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
            var txtCtrl = new Label().Text = "Keyboard shortcut";
            _Layout.Items.Add(new StackLayoutItem(txtCtrl));
        }
        protected override void _ModelChangedHandler(object sender, PropertyChangedEventArgs e)
        {

        }
    }
}