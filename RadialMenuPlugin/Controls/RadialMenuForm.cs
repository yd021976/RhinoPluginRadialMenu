using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.PlugIns;
using NLog;
using RadialMenuPlugin.Data;
using RadialMenuPlugin.Controls.Buttons.MenuButton;
using RadialMenuPlugin.Controls.ContextMenu.MenuButton;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base;
using RadialMenuPlugin.Controls.Buttons.Shaped.Form.Center;

namespace RadialMenuPlugin.Controls
{
    public class RadialMenuForm : TransparentForm
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Public properties
        #endregion

        #region Protected/Private properties
        protected static int s_defaultInnerRadius = 60;
        protected static int s_defaultThickness = 65;
        protected static int s_maxLevels = 3;
        protected static Size s_menuSize = new Size(1000, 1000);
        protected PixelLayout _Layout = new PixelLayout();
        protected List<RadialMenuLevel> _Levels = new List<RadialMenuLevel>() {
                new RadialMenuLevel(1,s_defaultInnerRadius,s_defaultThickness),
                new RadialMenuLevel(2,s_defaultInnerRadius+s_defaultThickness+8,s_defaultThickness,360/8/2), // For this one, start angle at 45° to alternate buttons
                new RadialMenuLevel(3,s_defaultInnerRadius+s_defaultThickness+8+s_defaultThickness+8,s_defaultThickness),
            };
        /// <summary>
        /// Keep track of drag source Control to register/unregister "dragEnd" event
        /// </summary>
        protected Control _Dragsource;
        /// <summary>
        /// Context menu
        /// </summary>
        protected ButtonSettingEditorForm _ContextMenuForm;
        /// <summary>
        /// Current model of mouse over button
        /// <para>
        /// REMARK:
        /// </para>
        /// <para>
        /// We use this because mouse enter/leave of button do not occurs sequentially, and sometime leave event occurs after enter event.
        /// </para>
        /// <para>
        /// That lead in no "center button" tooltip displayed. So we used this property in <see cref="_UpdateTooltipBinding"/> to check if we should update binding for center button tooltip
        /// </para>
        /// </summary>
        protected Model _CurrentButtonModel;

        /// <summary>
        /// Current radial menu mode : true=>Edit mode, false=>normal mode 
        /// </summary>
        protected bool _EditMode = false;
        /// <summary>
        /// Associate a menu level to a Control of type <SectorArcRadialControl>
        /// </summary>
        protected Dictionary<RadialMenuLevel, RadialMenuControl> _Controls = new Dictionary<RadialMenuLevel, RadialMenuControl>();
        protected FormCenterButton _CenterMenuButton;
        protected readonly Macro MouseHoverLeftTooltip = new Macro("", "Close");
        protected readonly Macro MouseHoverRightTooltip = new Macro("", "Edit radial menu");
        /// <summary>
        /// Ref to Model object when button is dragged
        /// </summary>
        #endregion

        #region Public methods


        /// <summary>
        /// 
        /// </summary>
        /// <param name="plugin"></param>
        public RadialMenuForm(PlugIn plugin) : base(plugin)
        {
            Size = new Size(s_menuSize.Width, s_menuSize.Height);
            _InitLevels(); // Create "blank" radial menu controls (i.e. no button IDs, blank models)
            KeyDown += (s, e) =>
            {
                // React to key press only when no context menu is shown and if the key is not "escape"
                // We will manage here key press for button trigger by keyboard
                // REMARK:"escape" keyup event is managed in "_OnEscapePressed" handler and is also fired by the RadialMenuCommand class
                if (e.Key == Keys.Escape) return;
                if (_ContextMenuForm.Visible == false)
                {
                    _OnKeyPressed(s, e);
                }
            };
            // Init the button context menu Form
            _ContextMenuForm = new ButtonSettingEditorForm();
            _ContextMenuForm.TriggerTextChanging += _OnContextMenuTriggerChanging; // Check trigger key is not already used in radial menu level

            // Ensure when form is shown that we display only first menu level
            Shown += (o, e) =>
            {
                foreach (var ctrl in _Controls.Values)
                {
                    ctrl.Show(false); // Hide and reset radial control enable and selection
                    ctrl.SwitchEditMode(false); // ensure disable edit mode
                }
                var radialControl = _Controls.First(obj => obj.Key.Level == 1).Value;
                radialControl.SwitchEditMode(false);
                _EditMode = false;
                radialControl.Show(true);
            };

            // Create close button
            _InitCenterButton();
            _Layout.Add(_CenterMenuButton, (Size.Width / 2) - _CenterMenuButton.Size.Width / 2, (Size.Height / 2) - (_CenterMenuButton.Size.Height / 2));

            // Create and add RadialMenu Control for 1st level
            var ctrl = _Controls.First(level => level.Key.Level == 1).Value;
            ctrl.SetMenuForButtonID(null); // Init buttons and model for 1st menu level

            // Add layout to content of form
            Content = _Layout;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="owner"></param>
        public RadialMenuForm(PlugIn plugin, Window owner) : this(plugin)
        {
            Owner = owner;
        }

        #endregion

        #region Protected/Private methods
        /// <summary>
        /// Init menu level controls
        /// </summary>
        protected void _InitLevels()
        {
            for (int i = 0; i < s_maxLevels; i++)
            {
                var ctrl = new RadialMenuControl(_Levels[i]);

                // Mouse events
                ctrl.MouseEnterButton += _RadialControlMouseEnterButtonHandler;
                ctrl.MouseMoveButton += _RadialControlMouseMoveButtonHandler;
                ctrl.MouseLeaveButton += _RadialControlMouseLeaveButtonHandler;
                ctrl.MouseClickButton += _RadialControlMouseClickHandler;
                ctrl.ButtonContextMenu += _RadialControlContextMenu;

                // Drag Drop events
                ctrl.DragDropEnterButton += _RadialControlDragEnterHandler;
                ctrl.DragDropOverButton += _RadialControlDragOverHandler;
                ctrl.DragDropLeaveButton += _RadialControlDragLeaveHandler;
                ctrl.DragDropButton += _RadialControlDragDropHandler;
                ctrl.RemoveButton += _RadialControlRemoveButton;
                _Controls.Add(_Levels[i], ctrl);
            }
            //
            //REMARK Order is VERY important because arc button are rectangles. So if submenu level 2 is not "top most" (i.e. last position), drag over doesn't work and sometimes
            // drag over don't detect level "2" buttons
            //
            _Layout.Add(_Controls.ElementAt(0).Value, (Size.Width / 2) - (_Controls.ElementAt(0).Value.Size.Width / 2), Size.Height / 2 - (_Controls.ElementAt(0).Value.Size.Height / 2));
            _Layout.Add(_Controls.ElementAt(2).Value, (Size.Width / 2) - (_Controls.ElementAt(2).Value.Size.Width / 2), Size.Height / 2 - (_Controls.ElementAt(2).Value.Size.Height / 2));
            _Layout.Add(_Controls.ElementAt(1).Value, (Size.Width / 2) - (_Controls.ElementAt(1).Value.Size.Width / 2), Size.Height / 2 - (_Controls.ElementAt(1).Value.Size.Height / 2));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        protected bool _AcceptDragDrop(ButtonDragDropEventArgs eventArgs)
        {
            var sourceType = Utilities.DragDropUtilities.dragSourceType(eventArgs.DragEventSourceArgs.Source);
            switch (sourceType)
            {
                case DragSourceType.radialMenuItem:
                    var guid = eventArgs.DragEventSourceArgs.Data.GetString("MODEL_GUID");
                    var sourceDragModel = ModelController.Instance.Find(guid);
                    var ret = false;
                    if (sourceDragModel != null)
                    {
                        if (sourceDragModel.Data.Properties.IsFolder)
                        {
                            if (sourceDragModel == eventArgs.TargetModel) // If source is dragged onto initial place, accept drop
                            {
                                eventArgs.DragEventSourceArgs.Effects = DragEffects.All;
                                ret = true;
                            }
                            else
                            {
                                eventArgs.DragEventSourceArgs.Effects = DragEffects.None;
                            }
                        }
                        else
                        {
                            eventArgs.DragEventSourceArgs.Effects = DragEffects.All;
                            ret = true;
                        }
                    }
                    return ret;
                case DragSourceType.rhinoItem:
                    eventArgs.DragEventSourceArgs.Effects = DragEffects.All;
                    return true;
                case DragSourceType.unknown:
                default:
                    eventArgs.DragEventSourceArgs.Effects = DragEffects.None; // Reject drop if source is unknown
                    return false;
            }
        }
        /// <summary>
        /// Create DragEnd event handler for the source control that initiate a drag/drop. Will remove any existing DragEnd handler if exists
        /// </summary>
        /// <param name="sourceObject"></param>
        protected void _RegisterDragEndHandler(Control sourceObject)
        {
            // if (!(sourceObject is MenuButton)) // Don't register drag end for MenuButton, it will raise its own event <DoDragEnd>
            // {
            if (_Dragsource != null)
            {
                _Dragsource.DragEnd -= _RadialControlDragEndHandler; // Remove event handler for previous Drag source Control
            }
            _Dragsource = sourceObject;
            _Dragsource.DragEnd += _RadialControlDragEndHandler;
            // }
        }
        /// <summary>
        /// Update models after a drag/drop is done
        /// </summary>
        /// <param name="updatedProperties"></param>
        /// <param name="targetModel"></param>
        /// <param name="targetLevel"></param>
        protected void _UpdateDropItem(ButtonProperties updatedProperties, Model targetModel, int targetLevel)
        {
            targetModel.Data.Properties.Icon = updatedProperties.Icon;
            targetModel.Data.Properties.IsActive = true;
            targetModel.Data.Properties.IsFolder = false;
            targetModel.Data.Properties.LeftMacro = updatedProperties.LeftMacro;
            targetModel.Data.Properties.RightMacro = updatedProperties.RightMacro;
            targetModel.Data.Properties.CommandGUID = updatedProperties.CommandGUID;

            if (targetLevel > 1) // If icon is drop on a sub menu -> Update parents menu models
            {
                var parentModel = targetModel.Parent;
                do
                {
                    // Keep parent icon untouched if already set
                    if (parentModel.Data.Properties.Icon == null)
                    {
                        parentModel.Data.Properties.Icon = updatedProperties.Icon;
                    }
                    // Keep parent "isFolder" untouched if already set
                    if (parentModel.Data.Properties.IsFolder == false)
                    {
                        parentModel.Data.Properties.IsFolder = true;
                    }
                    parentModel.Data.Properties.IsActive = true; // Ensure icon is active
                    parentModel.Data.Properties.CommandGUID = updatedProperties.CommandGUID;
                    parentModel = parentModel.Parent;
                } while (parentModel != null);
            }
        }
        /// <summary>
        /// Find and return first control object that matches <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">function to find object</param>
        /// <returns>Control instance if found, null if not found</returns>
        protected RadialMenuControl _GetControl(Func<RadialMenuControl, bool> predicate)
        {
            RadialMenuControl ctrl = null;
            foreach (RadialMenuControl menuControl in _Controls.Values)
            {
                if (predicate(menuControl))
                {
                    ctrl = menuControl;
                    break;
                }
            }
            return ctrl;
        }
        protected void _RunRhinoCommand(string command)
        {
            _OnCloseClickEvent(this); // close radial menu
            RhinoApp.SetFocusToMainWindow();
            RhinoApp.RunScript(command, false); // Run Rhino command
        }
        /// <summary>
        /// Open a submenu from an opened menu control and given a button ID
        /// </summary>
        /// <param name="radialMenuControl">Currently opened submenu. Provided to compute next submenu level</param>
        /// <param name="model">Model contining the current button ID to open submenu for</param>
        /// <returns>the new <see cref="RadialMenuControl"/> opened submenu or null if no new submenu was opened</returns>
        protected RadialMenuControl _ShowSubmenu(RadialMenuControl radialMenuControl, Model model)
        {
            var nextLevel = radialMenuControl.Level.Level + 1; // Compute next level number
            var ctrl = _GetControl(element => element.Level.Level == nextLevel);
            if (ctrl != null)
            {
                ctrl.SetMenuForButtonID(model); // Update radial control buttons
                ctrl.Show(true); // Ensure control is visible
            }
            return ctrl;
        }

        /// <summary>
        /// Hide all submenus from last/max to level of <paramref name="radialMenuControl"/>
        /// <para>If no <paramref name="radialMenuControl"/> id provided, close all submenu except root submenu</para>
        /// </summary>
        /// <param name="radialMenuControl">Menu control to keep</param>
        private void _HideSubmenu(RadialMenuControl radialMenuControl = null)
        {
            var levelToClose = radialMenuControl == null ? 1 : radialMenuControl.Level.Level;
            foreach (var control in _Controls.Values)
            {
                // Hide menu control if higher than the one provided (or higher than "root" control menu)
                if (control.Level.Level > levelToClose)
                {
                    control.Show(false);
                    control.DisableButtonsExceptSelection(); // Enable all buttons
                    control.SelectedButtonID = ""; // Reset selection
                }
            }
        }
        /// <summary>
        /// Do some cleanup of visual control before exit (Hide) the plugin Form object
        /// </summary>
        /// <param name="sender"></param>
        protected override void _OnCloseClickEvent(object sender)
        {
            foreach (var control in _Controls.Values)
            {
                control.Show(false);
                control.DisableButtonsExceptSelection();
            }
            base._OnCloseClickEvent(sender);
        }
        /// <summary>
        /// Update binding for the center menu button
        /// <para>
        /// REMARK: model can be null to clear the displayed text
        /// </para>
        /// </summary>
        /// <param name="model"></param>
        protected void _UpdateTooltipBinding(Model model = null)
        {
            if (model == null)
            {
                _CenterMenuButton.SetTooltip();
            }
            else
            {
                // Show left/right macro tooltip, except on folder items
                if (!model.Data.Properties.IsFolder) _CenterMenuButton.SetTooltip(model.Data.Properties.LeftMacro, model.Data.Properties.RightMacro);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool _TriggerMenuItem(char key)
        {
#nullable enable
            RadialMenuControl? openedControl = null; // Current highest opened menu
#nullable disable
            // Get the current shown level and the parent model from the selected button of parent model
            foreach (var control in _Controls)
            {
                if (control.Value.IsVisible)
                {
                    if (control.Value.Level.Level > (openedControl?.Level.Level ?? 0))
                    {
                        openedControl = control.Value;
                    }
                }
            }
            if (openedControl != null)
            {
                foreach (var model in openedControl.GetModels())
                {
                    if (model.Data.Properties.Trigger.ToUpper() == key.ToString().ToUpper())
                    {
                        if (model.Data.Properties.IsFolder)
                        {
                            _ShowSubmenu(openedControl, model);
                            return true;
                        }
                        else
                        {
                            if (model.Data.Properties.LeftMacro.Script != "")
                            {
                                _RunRhinoCommand(model.Data.Properties.LeftMacro.Script);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Each ESC keypress closes sub menu until level 1 submenu is reached => Close radial menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void _OnEscapePressed(object sender, KeyEventArgs e)
        {
            // First check if contextmenu is opened. If so, close it
            if (_ContextMenuForm.Visible)
            {
                _ContextMenuForm.Close();
                return;
            }
            if (Visible) // Ensure radial menu is currently running
            {
                RadialMenuLevel topMostOpenedLevel = null;
                foreach (var control in _Controls)
                {
                    if (control.Value.IsVisible && (control.Key.Level > (topMostOpenedLevel == null ? 0 : topMostOpenedLevel.Level)))
                    {
                        topMostOpenedLevel = control.Key;
                    }
                }
                // Hide top most sub menu if we found any
                if (topMostOpenedLevel != null)
                {
                    if (topMostOpenedLevel.Level == 1)
                    {
                        // Close the radial menu
                        _OnCloseClickEvent(this);
                    }
                    else
                    {
                        _Controls[topMostOpenedLevel].Show(false);
                    }
                }
            }
        }
        /// <summary>
        /// Event handler for context menu Trigger text changing : Do not allow same trigger key at same level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _OnContextMenuTriggerChanging(object sender, TextChangingEventArgs e)
        {
            if (e.NewText != "" && _ContextMenuForm.Model.Data.Properties.Trigger != e.NewText)
            {
                foreach (var model in ModelController.Instance.GetSiblings(_ContextMenuForm.Model))
                {
                    if (model.Data.Properties.Trigger == e.NewText.ToUpper())
                    {
                        e.Cancel = true;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.IsChar)
            {
                e.Handled = _TriggerMenuItem(e.KeyChar);
            }
            else
            {
                //FIXME: Not great feature. Find another way to do this
                // if (e.Application || e.Control || e.Alt)
                // {
                //     if (!HasFocus)
                //     {
                //         Focus();
                //         e.Handled = true;
                //     }
                // }
            }

        }
        //
        //  Mouse enter, over and leave handlers to prevent default radial control default behavior when needed (i.e. Avoid un-selecting button)
        //  Those custom behaviors will influence the event raising of "SelectionChanged", so the logic of displaying sub menu levels is managed in "onSelectionChanged" event
        //
        /// <summary>
        /// Mouse enter a button : Manage show/hide submenus
        /// </summary>
        /// <param name="radialMenuControl"></param>
        protected void _RadialControlMouseEnterButtonHandler(RadialMenuControl radialMenuControl, ButtonMouseEventArgs e)
        {
            Logger.Debug($"Mouse enter button {e.Model.Data.ButtonID}");
            if (e.Model.Data.Properties.IsFolder)
            {
                var newSubmenu = _ShowSubmenu(radialMenuControl, e.Model);
                // Hide higher levels from new opened submenu if any
                if (newSubmenu != null)
                {
                    _HideSubmenu(newSubmenu);
                }
            }
            else // If we enter a non folder, hide higher submenus 
            {
                _HideSubmenu(radialMenuControl); // Hide any opened higher submenu
            }
            _UpdateTooltipBinding(e.Model); // Update center button tooltip
        }

        /// <summary>
        /// Mouse over a button
        /// </summary>
        /// <param name="radialMenuControl"></param>
        protected void _RadialControlMouseMoveButtonHandler(RadialMenuControl radialMenuControl, ButtonMouseEventArgs e)
        {
            if (_ContextMenuForm.Visible == false)
            {
                Focus(); // Give radial menu focus when mouse overs a button
            }
            // _UpdateTooltipBinding(e.Model); // update tooltip
        }
        protected void _RadialControlMouseClickHandler(RadialMenuControl radialMenuControl, ButtonMouseEventArgs e)
        {
            // Do nothing on "folder" click
            if (e.Model.Data.Properties.IsFolder) return;

            switch (e.MouseEventArgs.Buttons)
            {
                case MouseButtons.Primary:
                    if (e.Model.Data.Properties.LeftMacro.Script != "")
                    {
                        _RunRhinoCommand(e.Model.Data.Properties.LeftMacro.Script);
                    }
                    break;
                case MouseButtons.Alternate:
                    if (e.Model.Data.Properties.RightMacro.Script != "")
                    {
                        _RunRhinoCommand(e.Model.Data.Properties.RightMacro.Script);
                    }
                    break;
            }
        }
        /// <summary>
        /// Mouse leave a button
        /// </summary>
        /// <param name="radialMenuControl"></param>
        protected void _RadialControlMouseLeaveButtonHandler(RadialMenuControl radialMenuControl, ButtonMouseEventArgs e)
        {
            Logger.Debug($"Mouse leave button {e.Model.Data.ButtonID}");

            //HACK: We can't leave a button if mouse is hover center button. This hack is because sometimes the "leave" event of a button can trigger AFTER the center button "ENTER" event
            if (_CenterMenuButton.CurrentButtonState.GetType() == typeof(HoverState)) { }
            else
            {
                _UpdateTooltipBinding(); // Update tooltip
            }
            if (_ContextMenuForm.Visible) return; // Don't give Rhino main window focus if we're showing context menu
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radialMenuControl"></param>
        protected void _RadialControlSelectionChangedHandler(RadialMenuControl radialMenuControl)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radialMenuControl"></param>
        protected void _RadialControlFocusRequestedHandler(RadialMenuControl radialMenuControl) { }
        /// <summary>
        /// Event when button request accept drop target. If drop target menu level is != from level of drag source AND it was a "self" drag, don't accept
        /// </summary>
        /// <param name="radialMenuControl"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlAcceptDropHandler(RadialMenuControl radialMenuControl, DropTargetArgs eventArgs)
        { }
        /// <summary>
        /// Button properties were updated, dragging mode ended
        /// </summary>
        /// <param name="radialMenuControl"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlDragEnterHandler(RadialMenuControl radialMenuControl, ButtonDragDropEventArgs eventArgs)
        {
            if (_AcceptDragDrop(eventArgs))
            {
                if (radialMenuControl.Level.Level < s_maxLevels) radialMenuControl.SelectedButtonID = eventArgs.TargetModel.Data.ButtonID; // Update selection except for last menu level (useless)
                var newMenuControl = _ShowSubmenu(radialMenuControl, eventArgs.TargetModel); // Open submenu if drop is accepted
                if (newMenuControl != null) _HideSubmenu(newMenuControl); // Hide higher opened submenus
                _RegisterDragEndHandler(eventArgs.DragEventSourceArgs.Source); // Register "DragEnd" event handler
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radialMenuControl"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlDragOverHandler(RadialMenuControl radialMenuControl, ButtonDragDropEventArgs eventArgs)
        {
            if (_AcceptDragDrop(eventArgs))
            {
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radialMenuControl"></param>
        /// <param name="eventArgs"></param>
        private void _RadialControlDragLeaveHandler(RadialMenuControl radialMenuControl, ButtonDragDropEventArgs eventArgs)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radialMenuControl"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlDragDropHandler(RadialMenuControl radialMenuControl, ButtonDragDropEventArgs eventArgs)
        {
            // Update menu button data
            if (_AcceptDragDrop(eventArgs))
            {
                switch (Utilities.DragDropUtilities.dragSourceType(eventArgs.DragEventSourceArgs.Source))
                {
                    case DragSourceType.rhinoItem:
                        ButtonProperties toolbaritem = Utilities.DragDropUtilities.getDroppedToolbarItem(eventArgs.DragEventSourceArgs);
                        _UpdateDropItem(toolbaritem, eventArgs.TargetModel, radialMenuControl.Level.Level);
                        break;
                    case DragSourceType.radialMenuItem:
                        var guid = eventArgs.DragEventSourceArgs.Data.GetString("MODEL_GUID");
                        var sourceDragModel = ModelController.Instance.Find(guid);
                        if (sourceDragModel != null)
                        {
                            if (sourceDragModel == eventArgs.TargetModel) // If source and target are the same, do nothing
                            {
                                return;
                            }
                            else
                            {
                                if (eventArgs.TargetModel.Parent == sourceDragModel.Parent) // Item is moved on same level sub menu
                                {
                                    // Update target model
                                    eventArgs.TargetModel.Data.Properties.Icon = sourceDragModel.Data.Properties.Icon;
                                    eventArgs.TargetModel.Data.Properties.IsFolder = false;
                                    eventArgs.TargetModel.Data.Properties.IsActive = true;
                                    eventArgs.TargetModel.Data.Properties.LeftMacro = sourceDragModel.Data.Properties.LeftMacro;
                                    eventArgs.TargetModel.Data.Properties.RightMacro = sourceDragModel.Data.Properties.RightMacro;
                                    eventArgs.TargetModel.Data.Properties.CommandGUID = sourceDragModel.Data.Properties.CommandGUID;
                                    // Clear source model data
                                    sourceDragModel.Data.Properties.Icon = null;
                                    sourceDragModel.Data.Properties.IsActive = false;
                                    sourceDragModel.Data.Properties.IsFolder = false;
                                    sourceDragModel.Data.Properties.LeftMacro = new Macro();
                                    sourceDragModel.Data.Properties.RightMacro = new Macro();
                                    sourceDragModel.Data.Properties.CommandGUID = Guid.Empty;
                                }
                                else // Item is moved on another level sub menu
                                {
                                    // Create new model properties from source Model
                                    ButtonProperties data = new ButtonProperties(
                                        new Macro(sourceDragModel.Data.Properties.LeftMacro.Script, sourceDragModel.Data.Properties.LeftMacro.Tooltip),
                                        new Macro(sourceDragModel.Data.Properties.RightMacro.Script, sourceDragModel.Data.Properties.RightMacro.Tooltip),
                                        new Icon(sourceDragModel.Data.Properties.Icon.Frames), true, false,
                                        sourceDragModel.Data.Properties.CommandGUID);

                                    // Clear source model data
                                    sourceDragModel.Data.Properties.CommandGUID = Guid.Empty;
                                    sourceDragModel.Data.Properties.Icon = null;
                                    sourceDragModel.Data.Properties.IsActive = false;
                                    sourceDragModel.Data.Properties.IsFolder = false;
                                    sourceDragModel.Data.Properties.LeftMacro = new Macro();
                                    sourceDragModel.Data.Properties.RightMacro = new Macro();

                                    // (Recursive) Check each source item parent submenus still contains children. If not, clear parent model
                                    var parent = sourceDragModel.Parent;
                                    while (parent != null)
                                    {
                                        var sourceModelChildren = ModelController.Instance.GetChildren(parent); var activeIcon = 0;
                                        foreach (var model in sourceModelChildren)
                                        {
                                            if (model.Data.Properties.Icon != null) activeIcon++;
                                        }
                                        if (activeIcon == 0) // Parent model has no children left -> Remove icon and isFolder flag for parent model
                                        {
                                            parent.Data.Properties.Icon = null;
                                            parent.Data.Properties.IsFolder = false;
                                            parent.Data.Properties.IsActive = false;
                                            parent.Data.Properties.CommandGUID = Guid.Empty;
                                        }
                                        parent = parent.Parent;

                                    }
                                    // Rebuidld/Update sub menu hierarchy for target drop
                                    _UpdateDropItem(data, eventArgs.TargetModel, radialMenuControl.Level.Level);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// When source control drag ends, clear all opened submenus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlDragEndHandler(object sender, DragEventArgs eventArgs)
        {
            // Close submenus except level 1 menu
            foreach (var radialMenuControl in _Controls.Values)
            {
                switch (radialMenuControl.Level.Level)
                {
                    case > 1:
                        radialMenuControl.Show(false);
                        break;
                    default:
                        radialMenuControl.SelectedButtonID = "";
                        break;
                }
            }
            // Cleanup DragEnd handler
            if (_Dragsource != null)
            {
                _Dragsource.DragEnd -= _RadialControlDragEndHandler;
                _Dragsource = null;
            }
        }
        /// <summary>
        /// Remove button is requested. Clear buttons models and all children if they exists
        /// <para>
        /// Don't really remove models objects, only clear values
        /// </para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlRemoveButton(object sender, ButtonDragDropEventArgs eventArgs)
        {
            _ClearModel(eventArgs.TargetModel);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        protected void _ClearModel(Model model)
        {
            var children = ModelController.Instance.GetChildren(model);
            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    _ClearModel(child); // Clear children if any
                }
            }
            model.Clear();

            // Update parent model "isFolder" property depending it still have at least one active children button
            var parent = model.Parent;
            if (parent != null)
            {
                children = ModelController.Instance.GetChildren(parent);
                var isFolder = false;
                foreach (var child in children)
                {
                    if (child.Data.Properties.IsActive) { isFolder = true; break; }
                }
                parent.Data.Properties.IsFolder = isFolder;
            }
        }
        /// <summary>
        /// Shows context menu event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlContextMenu(object sender, ButtonMouseEventArgs eventArgs)
        {
            _ContextMenuForm.Model = eventArgs.Model;
            var location = new Point(eventArgs.ScreenLocation);
            location.X = location.X - 8;
            location.Y = location.Y - 8;
            _ContextMenuForm.Show(location);
        }
        private void _InitCenterButton()
        {
            _CenterMenuButton = new FormCenterButton(new Size(s_defaultInnerRadius * 2, s_defaultInnerRadius * 2));
            _CenterMenuButton.OnButtonMouseEnter += (o, e) =>
            {
                Logger.Debug($"Center button mouse Enter");
                Focus(); // Get menu focus
                _CenterMenuButton.SetTooltip(MouseHoverLeftTooltip, MouseHoverRightTooltip);
            };
            _CenterMenuButton.OnButtonMouseLeave += (o, e) =>
            {
                Logger.Debug($"Center button mouse Leave");
                _CenterMenuButton.SetTooltip(); // Clear tooltip as we leave button
            };
            _CenterMenuButton.OnButtonClickEvent += (sender, args) =>
            {
                // Right click on close button toggles edit mode
                if (args.Buttons == MouseButtons.Alternate)
                {
                    _EditMode = !_EditMode;
                    foreach (var control in _Controls.Values)
                    {
                        if (control.Level.Level != 1) control.Show(false); // Hide current opened sub menu
                        control.SwitchEditMode(_EditMode);
                    }
                }
                else if (args.Buttons == MouseButtons.Primary)
                {
                    _OnCloseClickEvent(sender);
                }
            };

        }
        #endregion
    }
}
