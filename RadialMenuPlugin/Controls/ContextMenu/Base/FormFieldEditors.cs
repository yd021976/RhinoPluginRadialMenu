using System;
using Eto.Drawing;
using Eto.Forms;

namespace RadialMenuPlugin.Controls.ContextMenu.Base.Editors
{
    public class StackField : StackLayout
    {
        protected StackLayoutItem _Label;
        protected Control _FieldEditor;

        public StackField(string label, Control editorCtrl) : base()
        {
            Orientation = Orientation.Horizontal;
            VerticalContentAlignment = VerticalAlignment.Center;
            Padding = new Padding(0, 0);
            Spacing = 32;

            // Add the label field
            var labelCtrl = new Label();
            labelCtrl.Text = label;
            _Label = new StackLayoutItem(labelCtrl);
            Items.Add(_Label);

            _FieldEditor = editorCtrl;
            Items.Add(new StackLayoutItem(_FieldEditor));
        }
    }
}