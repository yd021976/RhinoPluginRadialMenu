using System;
using System.Collections.Generic;
using System.Linq;
using customControls;
using Eto.Drawing;
using Eto.Forms;
using Rhino;

namespace Testing
{
    public class TestForm : Form
    {
        public Dictionary<CRadialControl, SectorData> layout = new Dictionary<CRadialControl, SectorData>();  // test dervived pixellayout class
        public PixelLayout formLayout = new PixelLayout();

        public TestForm() : base()
        {
            Size = new Eto.Drawing.Size(1200, 1200);
            formLayout.Size = Size;
            for (var i = 0; i < 3; i++)
            {
                var ctrl = new CRadialControl(80 + i * 150, 20, i);
                layout[ctrl] = new SectorData();
                formLayout.Add(ctrl, Size.Width / 2 - (ctrl.Size.Width / 2), Size.Height / 2 - (ctrl.Size.Height / 2));

                ctrl.onMouseOverButton += (sender) =>
                {
                    if (sender.shouldcloseSubmenu)
                    {
                        sender.shouldcloseSubmenu = false;
                        for (var i = 2; i > sender.level; i--)
                        {
                            var ctrl = layout.ElementAt(i).Key;
                            ctrl.hide();
                        }
                        sender.clearSelection();
                    }
                    else
                    {
                        if (sender.level < 2)
                        {
                            sender.disableBtnExcept(sender.selectedButtonID);
                            // layout.ElementAt(sender.level + 1).Key.show(sender.currentData.startAngle + (sender.currentData.sweepAngle / 2));
                            layout.ElementAt(sender.level + 1).Key.show();
                        }
                    }
                };
            }
            layout.ElementAt(0).Key.show(); // show 1st level

            Content = formLayout;
        }
    }
    public class CRadialControl : PixelLayout
    {
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event mouseOverButton onMouseOverButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="button"></param>
        public delegate void mouseOverButton(CRadialControl sender);
        /// <summary>
        /// Event to notify mouse leave
        /// </summary>
        public event mouseLeaveButton onMouseLeaveButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="button"></param>
        public delegate void mouseLeaveButton(CRadialControl sender);
        public Dictionary<Arcbutton, SectorData> layoutBtns = new Dictionary<Arcbutton, SectorData>();  // List of buttons
        int radius = 80;
        int thickness = 15;

        public int level = 0;
        public PixelLayout layout = new PixelLayout();
        public string selectedButtonID = "";
        public bool shouldcloseSubmenu = false;
        public SectorData currentData;

        public CRadialControl(int radius = 80, int thickness = 15, int level = 0) : base()
        {
            this.radius = radius;
            this.thickness = thickness;
            this.level = level;
            Size = new Size(1200, 1200);
            layout.Size = Size;
            for (var i = 0; i < 8; i++)
            {
                var ctrl = new Arcbutton();
                ctrl.ID = i.ToString();
                ctrl.onButtonMouseOverButton += (sender) =>
                {
                    // If we over a button that opened a submenu, tag it to be closed
                    if (selectedButtonID == sender.ID)
                    {
                        shouldcloseSubmenu = true;
                    }
                    if (level < 2)
                    {
                        sender.isSelected = true; // Only select button if not last level
                    }
                    selectedButtonID = sender.ID;
                    currentData = sender.data;
                    onMouseOverButton?.Invoke(this);
                };
                ctrl.onbuttonMouseLeaveButton += (sender) =>
                {
                    if (level > 1)
                    {
                        selectedButtonID = "";
                        currentData = null;
                    }
                };


                layoutBtns[ctrl] = new SectorData();
                Add(ctrl, 0, 0);
            }
            // Content = layout;
        }
        public void show(int startAngle = 0)
        {
            Visible = true;
            var currX = 50; // Start of random X

            for (var i = 0; i < 8; i++)
            {
                var _graphicsPath = new GraphicsPath();
                var sectorDrawer = new ArcSectorDrawer();

                // Will compute size and location of controls
                // var data = sectorDrawer.drawSector(_graphicsPath, Size.Width / 2, Size.Height / 2, new RadialMenuLevel(1, radius, thickness), startAngle, 360 / 8);
                var size = new Size(new Random().Next(50, 90), new Random().Next(50, 90));
                var location = new Point(new Random().Next(currX, currX+90), (level + 1) * 200);
                currX += 100+size.Width;
                // Get the control by Index in dictionary property
                var ctrl = layoutBtns.ElementAt(i).Key;

                // Udate control data
                // layoutBtns[ctrl] = data;

                // Move and resize control in pixel layout
                // Move(ctrl, (int)data.bounds.Left, (int)data.bounds.Top);
                Move(ctrl, location.X, location.Y);
                ctrl.Size = new Size(-1, -1); // Autoresize
                ctrl.Size = size; // change size

                startAngle += 360 / 8;
                if (startAngle >= 360) startAngle -= 360;
            }
        }
        public void hide()
        {
            Visible = false;
            clearSelection();
        }
        public void clearSelection()
        {
            // unselect button
            try
            {

                Arcbutton s = layoutBtns.First((btn) => btn.Key.isSelected == true).Key;
                if (s != null) s.isSelected = false;
            }
            catch { }

            // clear state
            currentData = null;
            selectedButtonID = "";
            enableBtns();
        }
        public void disableBtnExcept(string btnID)
        {
            // foreach (var btn in layoutBtns.Keys)
            // {
            //     if (btn.ID != btnID)
            //     {
            //         btn.Enabled = false;
            //     }
            // }
        }
        public void enableBtns()
        {
            // foreach (var btn in layoutBtns.Keys)
            // {
            //     btn.Enabled = true;
            // }
        }
    }


    public class Arcbutton : Button
    {
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event buttonMouseOver onButtonMouseOverButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonMouseOver(Arcbutton sender);
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event buttonMouseLeaveButton onbuttonMouseLeaveButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonMouseLeaveButton(Arcbutton sender);

        public ImageButton btn = new ImageButton();
        public SectorData data;
        public bool isHovering = false;
        protected bool _isSelected = false;
        public bool isSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                BackgroundColor = Colors.Blue;
                // btn.setImage(data.images.sectorMask);
            }
        }


        public Arcbutton() : base()
        {
            Size = new Size(0, 0);
            BackgroundColor = Colors.LightGrey;
            MouseMove += (s, e) =>
            {
                Console.SetOut(RhinoApp.CommandLineOut);
                Console.WriteLine($"Over button ${ID}");

                // if (data != null)
                // {
                // if (data.isPointInShape(e.Location) != isHovering)
                if (!isHovering)
                {
                    isHovering = !isHovering;
                    BackgroundColor = Colors.Red;
                    if (isHovering)
                    {
                        // btn.setImage(data.images.overStateImage);
                        BackgroundColor = Colors.Red;
                        onButtonMouseOverButton?.Invoke(this);

                    }
                    else
                    {
                        if (!isSelected)
                        {
                            btn.setImage(data.images.normalStateImage);
                        }
                        BackgroundColor = Colors.LightGrey;
                        onbuttonMouseLeaveButton?.Invoke(this);
                    }
                    Invalidate();
                }
                // }
            };
            MouseLeave += (s, e) =>
                {
                    BackgroundColor = Colors.LightGrey;
                    isHovering = false;
                    Invalidate();
                };
            // Add(btn, 0, 0);
        }

        public void resizeIt(SectorData data)
        {
            this.data = data;
            Size = data.size;
            Invalidate();
            // btn.setImage(data.images.normalStateImage, data.size);
        }
    }
}