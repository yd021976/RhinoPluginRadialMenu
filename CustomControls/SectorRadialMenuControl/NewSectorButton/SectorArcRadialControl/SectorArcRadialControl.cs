using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using AppKit;
using Eto.Drawing;
using Eto.Forms;
using Foundation;
using Rhino;

namespace customControls
{
    /// <summary>
    /// PixelLayout object that is responsible of displaying sectors button
    /// <para>
    /// WARN: DO NOT USE "VISIBLE" property -> It prevents layout control "NSTrackingArea" to be updated when controls size changes
    /// Instead, we use "AlphaValue" to show/hide this control
    /// </para>
    /// </summary>
    public class SectorArcRadialControl : PixelLayout
    {
        #region Event args definition
        public struct ButtonInfos
        {
            public SectorArcButton.ButtonStates buttonState { get; }
            public Model model { get; }

            public ButtonInfos(SectorArcButton.ButtonStates buttonState, Model model)
            {
                this.buttonState = buttonState;
                this.model = model;
            }
        }
        public class SelectionArgs : EventArgs
        {
            /// <summary>
            /// <null> when no button selected
            /// </summary>
            public ButtonInfos? buttonProps;
            /// <summary>
            /// Give any event subscriber to ask change selection 
            /// </summary>
            public bool shouldUpdateSelection { get; set; } = true; // Gives event handler to change default behavior. If true, default behavior is applied
            public SelectionArgs(SectorArcButton.ButtonStates buttonState, Model model)
            {
                buttonProps = new ButtonInfos(buttonState, model);
            }
            public SelectionArgs() { }
        }
        public class DragDropArgs : SelectionArgs
        {
            public SectorArcButton.DropTargetArgs dragEvent;
            public DragDropArgs(SectorArcButton.ButtonStates buttonState, Model model, SectorArcButton.DropTargetArgs e) : base(buttonState, model)
            {
                dragEvent = e;
            }
        }
        #endregion

        #region  Events declaration
        /// <summary>
        /// Button click event
        /// </summary>
        public event clickEvent onClickEvent;
        public delegate void clickEvent(SectorArcRadialControl sender, SelectionArgs args);

        /// <summary>
        /// event to ensure main window has focus
        /// </summary>
        public event focusRequestedEvent onFocusRequested;
        public delegate void focusRequestedEvent(SectorArcRadialControl sender, SelectionArgs args);

        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event onDragDropButton onButtonDragDrop;
        public delegate void onDragDropButton(SectorArcRadialControl sender, DragDropArgs args);
        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event onDragEnterButton onButtonDragEnter;
        public delegate void onDragEnterButton(SectorArcRadialControl sender, DragDropArgs args);
        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event onDragOverButton onButtonDragOver;
        public delegate void onDragOverButton(SectorArcRadialControl sender, DragDropArgs args);
        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event onDragLeaveButton onButtonDragLeave;
        public delegate void onDragLeaveButton(SectorArcRadialControl sender, DragDropArgs args);
        /// <summary>
        /// Event to request drop target could be accepted
        /// </summary>
        public event dropTargetAccepted onDropTargetAccepted;
        public delegate void dropTargetAccepted(SectorArcRadialControl sender, SectorArcButton.DropTargetArgs args);

        /// <summary>
        /// Event to notify a button selection has changed
        /// </summary>
        public event selectionChanged onSelectionChanged;
        public delegate void selectionChanged(SectorArcRadialControl sender, SelectionArgs args);

        /// <summary>
        /// Event to notify mouse enters a button
        /// </summary>
        public event mouseEnterButton onMouseEnterButton;
        public delegate void mouseEnterButton(SectorArcRadialControl sender, SelectionArgs args);
        /// <summary>
        /// Event to notify mouse over a button
        /// </summary>
        public event mouseOverButton onMouseOverButton;
        public delegate void mouseOverButton(SectorArcRadialControl sender, SelectionArgs args);

        /// <summary>
        /// Event to notify mouse leave a button
        /// </summary>
        public event mouseLeaveButton onMouseLeaveButton;
        public delegate void mouseLeaveButton(SectorArcRadialControl sender, SelectionArgs args);
        #endregion

        private static int sectorsNumber = 8;
        public RadialMenuLevel level;
        public bool isVisible
        {
            get => nsView.AlphaValue == 0 ? false : true;
        }
        public NSView nsView { get => getNSView(); }

        protected Dictionary<SectorArcButton, Model> buttons = new Dictionary<SectorArcButton, Model>();
        /// <summary>
        /// The current selected button ID
        /// </summary>
        protected string _selectedButtonID;
        public string selectedButtonID
        {
            get => _selectedButtonID;
            set
            {
                SectorArcButton btn = null;
                var raiseEvent = selectedButtonID != value;
                _selectedButtonID = value;

                try
                {
                    btn = buttons.First(keyvaluepair => keyvaluepair.Key.ID == _selectedButtonID).Key;
                }
                catch { }
                finally
                {
                    if (value != "")
                    {
                        if (btn != null)
                        {
                            btn.states.isSelected = true;
                            clearSelection(_selectedButtonID); // Clear selection of others buttons
                        }
                    }
                    else // if no selection, clear all button selection state
                    {
                        clearSelection();
                    }

                    // Raise "selectionChanged" if selection changed
                    if (raiseEvent)
                    {
                        SelectionArgs args = buildSelectionEventArgs(btn);
                        onSelectionChanged?.Invoke(this, args);
                    }
                }
            }
        }
        private double animationDuration = 0.3;

        private ButtonModelData bindData(Dictionary<SectorArcButton, Model> data)
        {
            return data.First(d => d.Key.ID == "").Value.data;
        }
        public SectorArcRadialControl(RadialMenuLevel level, int startAngle = 0) : base()
        {
            var w = Stopwatch.StartNew(); //DEBUG Method measure
            this.level = level;
            Size = new Size((level.innerRadius + level.thickness) * 2, (level.innerRadius + level.thickness) * 2);

            // Init empty sectorArcButtons
            for (var i = 0; i < sectorsNumber; i++)
            {
                // Create button and model objects
                var btn = new SectorArcButton();
                var model = new Model(generateButtonID(startAngle, i), null);
                btn.onButtonClickEvent += buttonClickedHandler;
                btn.onbuttonMouseEnterButton += buttonMouseEnterHandler;
                btn.onButtonMouseOverButton += buttonMouseOverHandler;
                btn.onbuttonMouseLeaveButton += buttonMouseLeaveHandler;
                btn.onButtonDragDrop += buttonDragDropHandler;
                btn.onButtonDragEnter += buttonDragEnterHandler;
                btn.onButtonDragLeave += buttonDragLeaveHandler;
                btn.onButtonDragOver += buttonDragOverHandler;
                btn.onButtonAcceptTarget += buttonDropAcceptTargetHandler;
                buttons.Add(btn, model); // Update button dictionary
                btn.ButtonModelBinding.Bind(buttons, btnCollection => btnCollection[btn].data); // Bind button model
                Add(btn, 0, 0); // Add button to layout
            }
            Visible = true;
            //DEBUG Method measure
            w.Stop();
            Console.SetOut(RhinoApp.CommandLineOut);
            Console.WriteLine("SectorArcRadialControl constructor:" + w.ElapsedMilliseconds);
        }

        public ButtonProperties getButtonProperties(string buttonID)
        {
            try
            {
                return buttons.First(entry => entry.Key.ID == buttonID).Value.data.properties;
            }
            catch
            {
                Console.SetOut(RhinoApp.CommandLineOut);
                Console.WriteLine($"SectorArcRadialControl get button {buttonID} properties error");
                return null;
            }
        }
        public void setButtonProperties(string buttonID, ButtonProperties properties)
        {
            try
            {
                buttons.First(entry => entry.Key.ID == buttonID).Value.data.properties = properties;
            }
            catch
            {
                Console.SetOut(RhinoApp.CommandLineOut);
                Console.WriteLine($"SectorArcRadialControl set button {buttonID} properties error");
            }
            finally { }
        }
        public void show(bool show = true)
        {
            if (!show) // When hidding, reset button state (selected and enabled)
            {
                clearSelection(); // when control shows, reset all button selected to false
                enableButtons(); // When control is hidden, reset selection and enable children
            }
            setChildrenVisibleState(show); // Update children sector button visible state to prevent unwanted "onMouseOver" events
            animateShowHideEffect(show); // Animate effects
        }

        /// <summary>
        /// Activate or deactivate all buttons except the one selected
        /// If no button is selected, enable all buttons
        /// </summary>
        /// <param name="buttonID"></param>
        public void disableButtonsExceptSelection()
        {
            if (selectedButtonID == "") // If no selection active, enable all buttons
            {
                enableButtons();
            }
            else // If a button is selected, disable all others buttons
            {
                disableButtonsExcept(selectedButtonID);
            }
        }

        /// <summary>
        /// Do the control contains the provided button
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        public bool hasButton(SectorArcButton btn)
        {
            return buttons.ContainsKey(btn);
        }

        /// <summary>
        /// Display list of buttons for the specified ID (i.e. ID is the ID of the previous level button that trigger opening the menu)
        /// </summary>
        /// <param name="forButtonID"></param>
        public void setMenuForButtonID(Model parent = null, int startAngle = 0)
        {
            var w = Stopwatch.StartNew();//DEBUG Method measure

            // level.sectorData = buildSectors(sectorsNumber, startAngle);
            setupLayout(parent, startAngle); // Update layout with buttons

            //DEBUG Method measure
            w.Stop();
            Console.SetOut(RhinoApp.CommandLineOut);
            Console.WriteLine("setMenuForButtonID takes:" + w.ElapsedMilliseconds);
        }

        /// <summary>
        /// Disable all buttons except the one specified
        /// <para>Use this method when a button opens a folder</para>
        /// </summary>
        /// <param name="buttonID"></param>
        private void disableButtonsExcept(string buttonID)
        {
            foreach (var btn in buttons.Keys)
            {
                if (btn.ID == buttonID) btn.Enabled = true; else btn.Enabled = false;
            }
        }

        /// <summary>
        /// Enable all control buttons
        /// </summary>
        private void enableButtons()
        {
            foreach (var btn in buttons.Keys)
            {
                btn.Enabled = true;
            }
        }

        /// <summary>
        /// Reset all selected buttons
        /// </summary>
        private void clearSelection(string ExceptButtonID = "")
        {
            foreach (var btn in buttons.Keys)
            {
                if (ExceptButtonID != "") // Check if we have a button to not clear selection
                {
                    if (buttons[btn].data.buttonID != ExceptButtonID) btn.states.isSelected = false;
                }
                else
                {
                    btn.states.isSelected = false;
                }

            }
        }

        /// <summary>
        /// Compute new controls size and location to reorder layout control display of "sectors" 
        /// </summary>
        /// <param name="startAngle"></param>
        private void setupLayout(Model parent, int startAngle = 0)
        {
            selectedButtonID = ""; // As we build new layout, unselect any button
            // Iterate on each sector data to update button
            var i = 0; var swwep_angle = 360 / sectorsNumber;
            foreach (SectorArcButton button in buttons.Keys)
            {
                button.Unbind(); // Unbind any bindings
                var model = new Model(generateButtonID(startAngle, i), parent);
                buttons[button] = model; // update model
                button.ButtonModelBinding.Bind(buttons, bntCollection => bntCollection[button].data); // Bind button model
                button.ID = model.data.buttonID; // Update button ID

                var sectordata = updateSectorData(startAngle); // Compute new sector data from level and angle to setup button visual images
                Move(button, (int)sectordata.bounds.Left, (int)sectordata.bounds.Top); // Update control position
                buttons[button].data.sectorData = sectordata; // Update button model sectordata to update control size and images
                startAngle += swwep_angle; i++;
                if (startAngle >= 360) startAngle -= 360;
            }
        }

        private SectorData updateSectorData(int startAngle = 0)
        {
            var _graphicsPath = new GraphicsPath();
            var sectorDrawer = new ArcSectorDrawer();
            return sectorDrawer.drawSector(_graphicsPath, Size.Width / 2, Size.Height / 2, level, startAngle, 360 / sectorsNumber);
        }
        /// <summary>
        /// Build new sectors data
        /// </summary>
        /// <param name="sectorsNumber"></param>
        /// <param name="startAngle"></param>
        /// <returns></returns>
        private List<SectorData> buildSectors(int sectorsNumber, int startAngle = 0)
        {
            int angleStart;
            List<SectorData> sectors = new List<SectorData>();

            var sweepAngle = 360 / sectorsNumber;

            for (int i = 0; i < sectorsNumber; i++)
            {
                // compute angle. If > 360, reset to 0
                angleStart = startAngle + (i * sweepAngle);
                if (angleStart > 360) startAngle -= 360;

                // Draw one sector
                var _graphicsPath = new GraphicsPath();
                var sectorDrawer = new ArcSectorDrawer();
                var sectorData = sectorDrawer.drawSector(_graphicsPath, Size.Width / 2, Size.Height / 2, level, angleStart, sweepAngle);

                // add sector infos to list
                sectors.Add(sectorData);

                // Release resources
                _graphicsPath.Dispose();
            }
            return sectors;
        }

        /// <summary>
        /// Animate show and hide effect
        /// </summary>
        /// <param name="show"></param>
        private void animateShowHideEffect(bool show = true)
        {
            NSAnimationContext.BeginGrouping();
            NSAnimationContext.CurrentContext.AllowsImplicitAnimation = true;
            NSAnimationContext.CurrentContext.Duration = animationDuration;
            nsView.AlphaValue = show ? 1 : 0;
            NSAnimationContext.EndGrouping();
        }
        private void setChildrenVisibleState(bool show)
        {
            foreach (var ctrl in buttons.Keys)
            {
                ctrl.states.isVisible = show;
            }
        }
        /// <summary>
        /// Return the native MacOS NSView object
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        private NSView getNSView()
        {
            var ctrlProp = Handler.GetType().GetProperty("Control");
            return (NSView)ctrlProp.GetValue(Handler, null);
        }
        #region Button event handlers
        private void buttonClickedHandler(SectorArcButton button)
        {
            onClickEvent?.Invoke(this, buildSelectionEventArgs(button));
        }


        private void buttonMouseEnterHandler(SectorArcButton button)
        {
            SelectionArgs args = buildSelectionEventArgs(button);
            onMouseEnterButton?.Invoke(this, args); // Raise event before apply default behavior below.
            if (args.shouldUpdateSelection) // Does subscriber want default behavior ?
            {
                button.states.isSelected = true;
                _selectedButtonID = button.ID;
                Console.SetOut(RhinoApp.CommandLineOut);
                Console.WriteLine($"Radial control enter button {button.ID}");
            }

            // Raise event selectionChanged
            args = buildSelectionEventArgs(button);
            onSelectionChanged?.Invoke(this, args);
        }

        private void buttonMouseOverHandler(SectorArcButton button)
        {
            SelectionArgs args = buildSelectionEventArgs(button);
            onMouseOverButton?.Invoke(this, args);
        }

        private void buttonMouseLeaveHandler(SectorArcButton button)
        {
            SelectionArgs args = buildSelectionEventArgs(button);
            onMouseLeaveButton?.Invoke(this, args);
            if (args.shouldUpdateSelection) // Does subscriber want default behavior ?
            {
                button.states.isSelected = false; // Change selection state
                _selectedButtonID = "";
                Console.SetOut(RhinoApp.CommandLineOut);
                Console.WriteLine($"Radial control leave button {button.ID}");
            }

        }
        private void onButtonFocusRequested(SectorArcButton button)
        {
            onFocusRequested?.Invoke(this, buildSelectionEventArgs(button));
        }
        private void buttonDragDropHandler(SectorArcButton button, SectorArcButton.DropTargetArgs args)
        {
            onButtonDragDrop?.Invoke(this, buildDragdropEventArgs(button, args));
        }
        private void buttonDragEnterHandler(SectorArcButton button, SectorArcButton.DropTargetArgs args)
        {
            // button.states.isSelected = true;
            // _selectedButtonID = button.ID;
            onButtonDragEnter?.Invoke(this, buildDragdropEventArgs(button, args));
        }
        private void buttonDragOverHandler(SectorArcButton button, SectorArcButton.DropTargetArgs args)
        {
            onButtonDragOver?.Invoke(this, buildDragdropEventArgs(button, args));
        }
        private void buttonDragLeaveHandler(SectorArcButton button, SectorArcButton.DropTargetArgs args)
        {
            // button.states.isSelected = false; // Change selection state
            // _selectedButtonID = "";
            onButtonDragLeave?.Invoke(this, buildDragdropEventArgs(button, args));
        }

        private void buttonDropAcceptTargetHandler(SectorArcButton button, SectorArcButton.DropTargetArgs args)
        {
            onDropTargetAccepted?.Invoke(this, args);
        }
        #endregion

        private SelectionArgs buildSelectionEventArgs(SectorArcButton button)
        {
            if (button != null)
            {
                return new SelectionArgs(button.states, buttons[button]);
            }
            else
            {
                return new SelectionArgs();
            }
        }
        private DragDropArgs buildDragdropEventArgs(SectorArcButton button, SectorArcButton.DropTargetArgs evnt)
        {
            if (button != null)
            {
                return new DragDropArgs(button.states, buttons[button], evnt);
            }
            else
            {
                throw new Exception("Button can not be null for DragDrop event args");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startAngle"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private string generateButtonID(int startAngle, int i)
        {
            return "L:" + level.level + "-A:" + startAngle + "-Number:" + i.ToString();
        }
    }
}