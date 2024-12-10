using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using customControls;
using Eto.Drawing;
using Eto.Forms;
using Rhino;

namespace Testing
{
    public class TestForm2 : Form
    {
        public Dictionary<CRadialControl2, SectorData> layout = new Dictionary<CRadialControl2, SectorData>();  // test dervived pixellayout class
        public PixelLayout formLayout = new PixelLayout();
        public Button randomizeButton = new Button();

        public TestForm2() : base()
        {
            Size = new Eto.Drawing.Size(1200, 1200);
            formLayout.Size = Size;

            randomizeButton.Text = "Randomize";
            randomizeButton.Size = new Size(100, 50);
            formLayout.Add(randomizeButton, 50, 50);
            randomizeButton.Click += (s, e) =>
            {
                layout.ElementAt(0).Key.show();
                // layout.ElementAt(1).Key.show();
            };
            for (var i = 0; i < 3; i++)
            {
                var ctrl = new CRadialControl2(i);
                layout[ctrl] = new SectorData();
                formLayout.Add(ctrl, Size.Width / 2 - (ctrl.Size.Width / 2), Size.Height / 2 - (ctrl.Size.Height / 2));
                ctrl.onClickCControl += (sender) =>
                {
                    if (sender.level < 2)
                    {
                        layout.ElementAt(sender.level + 1).Key.show();
                    }
                };
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
                    }
                    else
                    {
                        if (sender.level < 2)
                        {
                            layout.ElementAt(sender.level + 1).Key.show();
                        }
                    }
                };
                // ctrl.onMouseOverButton += (sender) =>
                // {
                //     if (sender.shouldcloseSubmenu)
                //     {
                //         sender.shouldcloseSubmenu = false;
                //         for (var i = 2; i > sender.level; i--)
                //         {
                //             var ctrl = layout.ElementAt(i).Key;
                //             ctrl.hide();
                //         }
                //         sender.clearSelection();
                //     }
                //     else
                //     {
                //         if (sender.level < 2)
                //         {
                //             sender.disableBtnExcept(sender.selectedButtonID);
                //             // layout.ElementAt(sender.level + 1).Key.show(sender.currentData.startAngle + (sender.currentData.sweepAngle / 2));
                //             layout.ElementAt(sender.level + 1).Key.show();
                //         }
                //     }
                // };
            }
            layout.ElementAt(0).Key.show(); // show 1st level

            Content = formLayout;
        }
    }
    public class CRadialControl2 : PixelLayout
    {
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event mouseOverButton onMouseOverButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="button"></param>
        public delegate void mouseOverButton(CRadialControl2 sender);
        /// <summary>
        /// Event to notify mouse leave
        /// </summary>
        public event mouseLeaveButton onMouseLeaveButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="button"></param>
        public delegate void mouseLeaveButton(CRadialControl2 sender);
        /// <summary>
        /// Event to notify mouse leave
        /// </summary>
        public event clickCControl onClickCControl;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="button"></param>
        public delegate void clickCControl(CRadialControl2 sender);
        public Dictionary<Arcbutton, SectorData> layoutBtns = new Dictionary<Arcbutton, SectorData>();  // List of buttons
        public int level = 0;
        public string selectedButtonID = "";
        public bool shouldcloseSubmenu = false;
        public SectorData currentData;
        public CustomControl ctrl1 = new CustomControl();
        public CustomControl ctrl2 = new CustomControl();
        public CustomControl ctrl3 = new CustomControl();
        public Dictionary<CustomControl, SectorData> buttons = new Dictionary<CustomControl, SectorData>();

        public CRadialControl2(int level = 0) : base()
        {
            this.level = level;
            Size = new Size(1200, 1200);

            for (var i = 0; i < 5; i++)
            {
                var btn = new CustomControl();
                btn.ID = i.ToString();
                buttons.Add(btn, new SectorData());
                Add(btn, 0, 0);
                // btn.Click += (s, e) =>
                // {
                //     onClickCControl?.Invoke(this);
                // };
                btn.onButtonMouseOverButton += (s) =>
                {
                    if (s.ID == selectedButtonID && selectedButtonID != "")
                    {
                        shouldcloseSubmenu = true;
                        selectedButtonID = "";
                    }
                    else
                    {
                        selectedButtonID = s.ID;
                    }
                    onMouseOverButton?.Invoke(this);
                };
                btn.onbuttonMouseLeaveButton += (sender) =>
                {
                    if (level > 1)
                    {
                        selectedButtonID = "";
                        currentData = null;
                    }
                };
            }

            // Add(ctrl1, 0, 0);
            // Add(ctrl2, 0, 0);
            // Add(ctrl3, 0, 0);
            // for (var i = 0; i < 8; i++)
            // {
            //     var ctrl = new CustomControl();
            //     ctrl.ID = i.ToString();
            //     ctrl.onButtonMouseOverButton += (sender) =>
            //     {
            //         // If we over a button that opened a submenu, tag it to be closed
            //         if (selectedButtonID == sender.ID)
            //         {
            //             shouldcloseSubmenu = true;
            //         }
            //         if (level < 2)
            //         {
            //             sender.isSelected = true; // Only select button if not last level
            //         }
            //         selectedButtonID = sender.ID;
            //         currentData = sender.data;
            //         onMouseOverButton?.Invoke(this);
            //     };
            // ctrl.onbuttonMouseLeaveButton += (sender) =>
            // {
            //     if (level > 1)
            //     {
            //         selectedButtonID = "";
            //         currentData = null;
            //     }
            // };


            //     layoutBtns[ctrl] = new SectorData();
            //     Add(ctrl, 0, 0);
            // }
        }
        public void show(int startAngle = 0)
        {
            // Visible = true;
            getNSView().AlphaValue = 1;
            var currX = 50; // Start of random X


            // Will compute size and location of controls : WORKING

            // foreach (var ctrl in Controls)
            // {
            //     var size = new Size(new Random().Next(50, 90), new Random().Next(50, 90));
            //     var location = new Point(new Random().Next(currX, currX + 90), (level + 1) * 200);
            //     currX += 100 + size.Width;
            //     // Get the control by Index in dictionary property
            //     ctrl.Size = size;
            //     Move(ctrl, location.X, location.Y);
            // }

            // size = new Size(new Random().Next(50, 90), new Random().Next(50, 90));
            // location = new Point(new Random().Next(currX, currX + 90), (level + 1) * 200);
            // currX += 100 + size.Width;
            // // Get the control by Index in dictionary property
            // ctrl2.Size = size;
            // Move(ctrl2, location.X, location.Y);

            // size = new Size(new Random().Next(50, 90), new Random().Next(50, 90));
            // location = new Point(new Random().Next(currX, currX + 90), (level + 1) * 200);
            // currX += 100 + size.Width;
            // // Get the control by Index in dictionary property
            // ctrl3.Size = size;
            // Move(ctrl3, location.X, location.Y);


            foreach (var ctrl in buttons.Keys)
            {
                var size = new Size(new Random().Next(50, 90), new Random().Next(50, 90));
                var location = new Point(new Random().Next(currX, currX + 90), (level + 1) * 200 + new Random().Next(40, 90));
                currX += 100 + size.Width;
                // Get the control by Index in dictionary property
                ctrl.Size = size;
                Move(ctrl, location.X, location.Y);
            }
        }
        public void hide()
        {
            // Visible = false;
            getNSView().AlphaValue = 0;
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
            shouldcloseSubmenu = false;
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

        private NSView getNSView()
        {
            var ctrlProp = Handler.GetType().GetProperty("Control");
            return (NSView)ctrlProp.GetValue(Handler, null);
        }
    }


    public class CustomControl : PixelLayout
    {
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event buttonMouseOver onButtonMouseOverButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonMouseOver(CustomControl sender);
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
        public delegate void buttonMouseLeaveButton(CustomControl sender);

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


        public CustomControl() : base()
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