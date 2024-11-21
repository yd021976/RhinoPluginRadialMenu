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
    public partial class SectorArcRadialControl : PixelLayout
    {
        private static int sectorsNumber = 8;
        public RadialMenuLevel level;

        protected NSView nsView { get => getNSView(); }

        protected Dictionary<SectorArcButton, ButtonModel> buttons = new Dictionary<SectorArcButton, ButtonModel>();

        /// <summary>
        /// The current selected button ID
        /// </summary>
        public string selectedButtonID;

        /// <summary>
        /// Override Visible property to animate fade out when control becomes NOT visible
        /// </summary>
        public override bool Visible
        {
            set
            {
                animateShowHideEffect(value);   //NOTE: Visible will be set to "true" at end of animation
            }
        }


        public SectorArcRadialControl(RadialMenuLevel level, int startAngle = 0) : base()
        {
            this.level = level;
            Size = new Size((level.innerRadius + level.thickness) * 2, (level.innerRadius + level.thickness) * 2);

            // Init empty sectorArcButtons
            for (var i = 0; i < sectorsNumber; i++)
            {
                // Create button and model objects
                var btn = new SectorArcButton();
                var model = new ButtonModel();
                btn.onButtonClickEvent += onButtonClicked;
                btn.onButtonMouseOverButton += onButtonMouseOver;

                buttons.Add(btn, model); // Update button dictionary
                btn.ButtonModelBinding.Bind(buttons, r => r[btn]); // Bind button model
                Add(btn, 0, 0); // Add button to layout
            }
        }

        /// <summary>
        /// When a button is selected, disable all buttons except the one specified
        /// Also set the button status to "disabled"
        /// If "buttonID" is empty string, enable all buttons
        /// </summary>
        /// <param name="buttonID"></param>
        public void selectButtonID(string buttonID = "")
        {
            if (buttonID == "")
            {
                enableButtons();
            }
            else
            {
                disableButtonsExcept(buttonID);
            }
            selectedButtonID = buttonID;
        }


        /// <summary>
        /// Display list of buttons for the specified ID (i.e. ID is the ID of the previous level button that trigger opening the menu)
        /// </summary>
        /// <param name="forButtonID"></param>
        public void setMenuForButtonID(string forButtonID, int startAngle = 0)
        {
            var w = Stopwatch.StartNew();

            level.sectorData = buildSectors(sectorsNumber, startAngle);
            setupLayout(level.sectorData); // Update layout with buttons

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
        private void enableButtons()
        {
            foreach (var btn in buttons.Keys)
            {
                btn.Enabled = true;
            }
        }
        private void setupLayout(List<SectorData> sectorData)
        {
            // var w = Stopwatch.StartNew();

            // Iterate on each sector data to update button
            foreach (var sectorDataIterator in sectorData.Select((value, i) => new { i, value }))
            {
                SectorArcButton sectorBtn = buttons.ElementAt(sectorDataIterator.i).Key; //WARN: There should be a button at sector data Index
                buttons[sectorBtn].sectorData = sectorDataIterator.value; // Update button model
                buttons[sectorBtn].buttonID = "L:" + level.level + "-A:" + sectorDataIterator.value.startAngle + "-Number:" + sectorDataIterator.i.ToString(); // Update button ID
                // Update button position in layout
                Move(sectorBtn, (int)sectorDataIterator.value.bounds.Left, (int)sectorDataIterator.value.bounds.Top);
            }
            // w.Stop();
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine("setupLayout takes:" + w.ElapsedMilliseconds);
        }

        /// <summary>
        /// Build new sectors data
        /// </summary>
        /// <param name="sectorsNumber"></param>
        /// <param name="startAngle"></param>
        /// <returns></returns>
        private List<SectorData> buildSectors(int sectorsNumber, int startAngle = 0)
        {
            var w = Stopwatch.StartNew();

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
            w.Stop();
            Console.SetOut(RhinoApp.CommandLineOut);
            Console.WriteLine("buildSectors takes :" + w.ElapsedMilliseconds);
            return sectors;
        }

        /// <summary>
        /// Override to show fade in animation when control becomes visible
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            // animateShowHideEffect(true);
            // base.OnShown(e);
        }

        /// <summary>
        /// Animate show and hide effect
        /// </summary>
        /// <param name="show"></param>
        private void animateShowHideEffect(bool show)
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = 0.2;
                    context.AllowsImplicitAnimation = true;
                    nsView.AlphaValue = show == true ? 1 : 0;
                });
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
        ~SectorArcRadialControl()
        {
            //TODO: Dispose all graphics and images
        }

        private void onButtonClicked(SectorArcButton button)
        {
            onClickEvent?.Invoke(this, buildEventArgs(button));
        }
        private void onButtonMouseOver(SectorArcButton button)
        {
            onMouseOverButton?.Invoke(this, buildEventArgs(button));
        }
        private void onButtonMouseLeave(SectorArcButton button)
        {
            onMouseLeaveButton?.Invoke(this, buildEventArgs(button));
        }
        private void onButtonFocusRequested(SectorArcButton button)
        {
            onFocusRequested?.Invoke(this, buildEventArgs(button));
        }
        private void onButtonNewIconAdded(SectorArcButton button)
        {
            onNewIconAdded?.Invoke(this, buildEventArgs(button));
        }

        private SectorArcRadialControlEventArgs buildEventArgs(SectorArcButton button)
        {
            return new SectorArcRadialControlEventArgs(button, buttons[button], button.isDraggingIcon);
        }
    }
}