using Eto.Forms;
using Eto.Drawing;
using RadialMenuPlugin.Data;
using RadialMenuPlugin.Controls.ContextMenu.Base.Editors;
using System.ComponentModel;
using System;
using RadialMenuPlugin.Utilities;

namespace RadialMenuPlugin.Controls.ContextMenu.MenuButton
{
    /// <summary>
    /// 
    /// </summary>
    public class ButtonSettingEditorContents : Base.ContextMenuContent<Model>
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected TextBox _TriggerEditor;
        protected GroupBox _LeftMacroEditor;
        protected TextBox _RhinoLeftMacroCommandEditor;
        protected TextBox _RhinoLeftMacroTooltipEditor;
        protected GroupBox _RightMacroEditor;
        protected TextBox _RhinoRightMacroCommandEditor;
        protected TextBox _RhinoRightMacroTooltipEditor;
        protected Label _MessageLabel;
        protected ImageView _CommandIconView;
        protected Bitmap _DefaultIcon;
        protected OpenFileDialog _IconFileChooser;
        public bool FileSelectorOpened = false;
        public event EventHandler<TextChangingEventArgs> TextChanging;

        public ButtonSettingEditorContents() : base()
        {
            _DefaultIcon = Bitmap.FromResource("TigrouRadialMenu.Bitmaps.question-mark-circle-outline-icon.png");
            _IconFileChooser = new OpenFileDialog();
            var filter = new FileFilter { Name = "Images", Extensions = new string[] { ".jpeg", ".png", ".png", ".bmp" } };
            _IconFileChooser.Filters.Add(filter);

            Orientation = Orientation.Horizontal;
            Padding = new Padding(16);
            VerticalContentAlignment = VerticalAlignment.Stretch;

            var _containerLayout = new StackLayout(); _containerLayout.Orientation = Orientation.Vertical; _containerLayout.HorizontalContentAlignment = HorizontalAlignment.Stretch; _containerLayout.Spacing = 8;
            _InitTriggerEditor();
            _InitRhinoMacrosEditor();
            _InitIconEditor();
            _InitLabel();
            var triggerStackField = new StackField("Trigger", _TriggerEditor);
            var rhinoLeftMacroEditor = new StackField("Rhino left command", _LeftMacroEditor);
            var rhinoRightMacroEditor = new StackField("Rhino right command", _RightMacroEditor);
            var commandIcon = new StackField("Icon", _CommandIconView);
            var messageLabel = new StackField("", _MessageLabel);
            _containerLayout.Items.Add(triggerStackField);
            _containerLayout.Items.Add(rhinoLeftMacroEditor);
            _containerLayout.Items.Add(rhinoRightMacroEditor);
            _containerLayout.Items.Add(commandIcon);
            _containerLayout.Items.Add(messageLabel);

            var mainContent = new StackLayoutItem(_containerLayout, true);
            Items.Add(mainContent);
        }
        /// <summary>
        /// Update binding when Model object is updated
        /// </summary>
        protected override void _UpdateModelBindings()
        {
            _TriggerEditor.Unbind();
            _TriggerEditor.TextBinding.Bind(_Model.Data.Properties, nameof(_Model.Data.Properties.Trigger));

            _RhinoLeftMacroCommandEditor.Unbind();
            _RhinoLeftMacroCommandEditor.TextBinding.Bind(_Model.Data.Properties, prop => prop.LeftMacro.Script);
            _RhinoLeftMacroTooltipEditor.Unbind();
            _RhinoLeftMacroTooltipEditor.TextBinding.Bind(_Model.Data.Properties, prop => prop.LeftMacro.Tooltip);

            _RhinoRightMacroCommandEditor.Unbind();
            _RhinoRightMacroCommandEditor.TextBinding.Bind(_Model.Data.Properties, prop => prop.RightMacro.Script);
            _RhinoRightMacroTooltipEditor.Unbind();
            _RhinoRightMacroTooltipEditor.TextBinding.Bind(_Model.Data.Properties, prop => prop.RightMacro.Tooltip);

            Icon icon = _Model.Data.Properties.Icon;
            if (icon == null)
            {
                icon = _DefaultIcon.WithSize(DragDropUtilities.IconSize);
            }
            _CommandIconView.Image = icon;
        }
        protected override void _ModelChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_Model.Data.Properties.Icon):
                    _CommandIconView.Image = _Model.Data.Properties.Icon;
                    break;
                default:
                    break;
            }
        }
        protected override void OnShown(EventArgs e)
        {
            _MessageLabel.Text = "";
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
            _TriggerEditor.Size = new Size(-1, -1);
            _TriggerEditor.TextChanging += (s, e) =>
            {
                // Give chance to cancel text changing to subscribers
                TextChanging?.Invoke(this, e);
                if (e.Cancel)
                {
                    _MessageLabel.Text = "Trigger already assigned";
                }
                else
                {
                    _MessageLabel.Text = "";
                }
            };
            // As text changes, set letter to upper case
            _TriggerEditor.TextChanged += (s, e) =>
            {
                _TriggerEditor.Text = _TriggerEditor.Text.ToUpper();
            };
        }
        /// <summary>
        /// 
        /// </summary>
        protected void _InitRhinoMacrosEditor()
        {
            var layoutLeftMacro = new StackLayout(); layoutLeftMacro.Spacing = 4; layoutLeftMacro.Orientation = Orientation.Vertical; layoutLeftMacro.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            _LeftMacroEditor = new GroupBox();
            _LeftMacroEditor.Content = layoutLeftMacro;

            _RhinoLeftMacroCommandEditor = new TextBox(); _RhinoLeftMacroCommandEditor.ToolTip = "Enter Rhino \"Left\" command";
            _RhinoLeftMacroCommandEditor.ShowBorder = false;
            _RhinoLeftMacroTooltipEditor = new TextBox(); _RhinoLeftMacroTooltipEditor.ToolTip = "Enter tooltip here";
            _RhinoLeftMacroTooltipEditor.ShowBorder = false;
            var leftStackitem1 = new StackLayoutItem(_RhinoLeftMacroCommandEditor, true);
            var leftStackItem2 = new StackLayoutItem(_RhinoLeftMacroTooltipEditor, true);
            layoutLeftMacro.Items.Add(leftStackitem1);
            layoutLeftMacro.Items.Add(leftStackItem2);

            var layoutRightMacro = new StackLayout(); layoutRightMacro.Spacing = 4; layoutRightMacro.Orientation = Orientation.Vertical; layoutRightMacro.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            _RightMacroEditor = new GroupBox();
            _RightMacroEditor.Content = layoutRightMacro;
            _RhinoRightMacroCommandEditor = new TextBox(); _RhinoRightMacroCommandEditor.ToolTip = "Enter Rhino \"Right\" command";
            _RhinoRightMacroCommandEditor.ShowBorder = false;
            _RhinoRightMacroTooltipEditor = new TextBox(); _RhinoRightMacroTooltipEditor.ToolTip = "Enter tooltip here";
            _RhinoRightMacroTooltipEditor.ShowBorder = false;
            var rightStackitem1 = new StackLayoutItem(_RhinoRightMacroCommandEditor, true);
            var rightStackItem2 = new StackLayoutItem(_RhinoRightMacroTooltipEditor, true);
            layoutRightMacro.Items.Add(rightStackitem1);
            layoutRightMacro.Items.Add(rightStackItem2);
        }
        protected void _InitIconEditor()
        {
            _CommandIconView = new ImageView();
            // _CommandIconView.BackgroundColor = Colors.Gray;
            _CommandIconView.BackgroundColor = Colors.Transparent;
            _CommandIconView.MouseDown += (s, e) =>
            {
                FileSelectorOpened = true;

                _IconFileChooser.ShowDialog(ParentWindow); // Modal blocking dialog
                if (_IconFileChooser.FileName != null)
                {
                    try
                    {
                        var bitmap = new Bitmap(_IconFileChooser.FileName);
                        _Model.Data.Properties.Icon = bitmap.WithSize(DragDropUtilities.IconSize);
                        _Model.Data.Properties.CommandGUID = Guid.NewGuid(); // Generate a new Guid for this image/icon
                        _Model.Data.Properties.IsActive = true; // Set button as active
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                    }
                }
                FileSelectorOpened = false;
            };
        }
        protected void _InitLabel()
        {
            _MessageLabel = new Label();
            _MessageLabel.Text = "";
        }
    }
}