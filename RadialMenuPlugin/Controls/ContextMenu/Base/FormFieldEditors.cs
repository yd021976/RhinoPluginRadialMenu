using Eto.Forms;

namespace RadialMenuPlugin.Controls.ContextMenu.Base.Editors
{
    public class StackField : StackLayoutItem
    {
        protected StackLayout _layout;
        protected StackLayoutItem _Label;
        protected StackLayoutItem _FieldEditor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="editorCtrl"></param>
        public StackField(string label, Control editorCtrl, int labelWidth = 140) : base()
        {
            // Main Layout
            _layout = new StackLayout();
            _layout.Orientation = Orientation.Horizontal;
            _layout.VerticalContentAlignment = Eto.Forms.VerticalAlignment.Center;
            _layout.Spacing = 16;


            // Add the label field
            var labelCtrl = new Label();
            // labelCtrl.BackgroundColor = Colors.DarkOrange;
            labelCtrl.TextAlignment = TextAlignment.Left; labelCtrl.Width = labelWidth;
            labelCtrl.Text = label;
            _Label = new StackLayoutItem(labelCtrl, false);
            _Label.HorizontalAlignment = Eto.Forms.HorizontalAlignment.Left;

            // Add field editor control
            _FieldEditor = new StackLayoutItem(editorCtrl, true);
            _FieldEditor.HorizontalAlignment = Eto.Forms.HorizontalAlignment.Right;

            _layout.Items.Add(_Label);
            _layout.Items.Add(_FieldEditor);

            // set item contents
            Control = _layout;
        }
    }
}