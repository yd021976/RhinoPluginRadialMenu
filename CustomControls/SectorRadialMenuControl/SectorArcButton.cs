// using System;
// using AppKit;
// using Eto.Drawing;
// using Eto.Forms;
// using RadialMenu;
// using Rhino;

// namespace customControls
// {
   
//     public class SectorArcButton_old : Drawable
//     {
//         protected class backdropButton : Drawable
//         {
//             protected Image _image;
//             public backdropButton(Image image, Size size) : base()
//             {
//                 _image = image;
//                 base.Size = size;
//             }
//             public void setImage(Image image)
//             {
//                 _image = image;
//                 Invalidate();
//             }
//             protected override void OnPaint(PaintEventArgs drawingContext)
//             {
//                 drawingContext.Graphics.DrawImage(_image, 0, 0);
//             }
//         }
//         /// <summary>
//         /// Used for animating button UI state changes
//         /// </summary>
//         protected backdropButton _backdropButton;

//         public SectorData sectorData;
//         protected bool isHovering = false;
//         public new string ID { get { return sectorData.buttonID; } set { base.ID = value; } }
//         protected bool isDraggingIcon = false;
//         protected ButtonProperties properties;
//         /// <summary>
//         /// Button click event
//         /// </summary>
//         public event buttonClickEvent onclickEvent;
//         /// <summary>
//         /// Delegate for event for button click
//         /// </summary>
//         /// <param name="sender"></param>
//         public delegate void buttonClickEvent(SectorArcButton sender);

//         /// <summary>
//         /// Event for requesting radial menu to get focus
//         /// </summary>
//         public event buttonRequestFocus onRequestFocusEvent;
//         /// <summary>
//         /// Delegate for event for requesting radial menu to get focus
//         /// </summary>
//         /// <param name="sender"></param>
//         public delegate void buttonRequestFocus(SectorArcButton sender);

//         /// <summary>
//         /// Event to notify an icon has been added to button
//         /// </summary>
//         public event newIconAdded onNewIconAdded;
//         /// <summary>
//         /// Delegate for Event to notify an icon has been added to button
//         /// </summary>
//         /// <param name="sender"></param>
//         public delegate void newIconAdded(SectorArcButton sender, ButtonProperties properties);

//         /// <summary>
//         /// Event to notify mouse is over button
//         /// </summary>
//         public event mouseOver onMouseOverButton;
//         /// <summary>
//         /// Delegate
//         /// </summary>
//         /// <param name="sender"></param>
//         public delegate void mouseOver(SectorArcButton sender, ButtonProperties properties);
//         /// <summary>
//         /// Event to notify mouse is over button
//         /// </summary>
//         public event mouseLeaveButton onMouseLeaveButton;
//         /// <summary>
//         /// Delegate
//         /// </summary>
//         /// <param name="sender"></param>
//         public delegate void mouseLeaveButton(SectorArcButton sender, ButtonProperties properties);


//         public SectorArcButton(SectorData sectorData) : base()
//         {
//             _backdropButton = new backdropButton(sectorData.images.sectorMask,sectorData.size);
//             this.sectorData = sectorData;
//             base.Size = this.sectorData.size;

//             // set properties from settings
//             try
//             {
//                 properties = RadialMenuPlugin.Instance.settingsHelper.settings.buttonProperties[this.sectorData.buttonID];
//             }
//             catch
//             {
//                 properties = new ButtonProperties();
//             }
//             MouseMove += mouseMoveHandler;
//             MouseLeave += mouseLeaveHandler;
//             MouseEnter += mouseEnterHandler;
//             MouseDown += mouseDownHandler;
//             EnabledChanged += enableStateChanged;

//             // DragDrop
//             AllowDrop = true;
//             DragEnter += dragEnterHandler;
//             DragOver += dragOverHandler;
//             DragLeave += dragLeaveHandler;
//             DragDrop += dragDropHandler;
//         }
//         protected override void OnPaint(PaintEventArgs e)
//         {
//             base.OnPaint(e);
//             if (!Enabled)
//             {
//                 e.Graphics.DrawImage(sectorData.images.disabledStateImage, 0, 0);
//             }
//             else
//             {
//                 // Draw arc sector image

//                 // Mouse is not hovering button
//                 if (!isHovering)
//                 {
//                     if (sectorData.images.normalStateImage != null)
//                     {

//                         e.Graphics.DrawImage(sectorData.images.normalStateImage, 0, 0);
//                     }
//                 }
//                 // Mouse if hovering button
//                 else
//                 {
//                     if (sectorData.images.overStateImage != null)
//                     {
//                         e.Graphics.DrawImage(sectorData.images.overStateImage, 0, 0);

//                         // get button native NSView
//                         var ctrl = Handler.GetType().GetProperty("Control");
//                         var nsView = (NSView)ctrl.GetValue(Handler, null);

//                         // get backdrop native NSView
//                         var ctrl2 = _backdropButton.Handler.GetType().GetProperty("Control");
//                         var nsView2 = (NSView)ctrl.GetValue(_backdropButton.Handler, null);

//                         // try to add "backdrop" as subview of "button"
//                         _backdropButton.setImage(sectorData.images.normalStateImage);
//                         nsView.AddSubview(nsView2, NSWindowOrderingMode.Above, null);
                        
//                         nsView.AlphaValue = 0; // hide button
//                         nsView2.AlphaValue = 1;

//                     }
//                 }

//                 // Draw arc sector icon
//                 if (properties.isActive && properties.icon != null)
//                 {
//                     var arcCenterWorld = sectorData.sectorCenter();
//                     var arcCenterLocal = sectorData.convertWorldToLocal(arcCenterWorld);

//                     e.Graphics.DrawImage(properties.icon, arcCenterLocal.X - (properties.icon.Size.Width / 2), arcCenterLocal.Y - (properties.icon.Size.Height / 2));
//                 }
//             }
//         }

//         private void mouseDownHandler(object sender, MouseEventArgs e)
//         {
//             if (!isDraggingIcon)
//             {
//                 if (e.Modifiers == Keys.Application && properties.isActive)
//                 {
//                     DataObject eventObj = new DataObject();
//                     DoDragDrop(eventObj, DragEffects.All, properties.icon, new PointF(10, 10));

//                 }
//                 else
//                 {
//                     if (properties.isActive)
//                     {
//                         if (properties.rhinoScript != "")
//                         {
//                             onclickEvent?.Invoke(this); // Raise onclick event to be handled by delegate when rhino command is executed
//                             Rhino.RhinoApp.RunScript(properties.rhinoScript, false);
//                         }
//                     }
//                 }
//             }
//         }

//         /// <summary>
//         /// Handler for mouse move in control. Note that we check and fire custom mouse "leave" and "over" here because
//         /// the shape is not a rectangle. So the mouse can already "enter" in rectangle but not in the control shape
//         /// </summary>
//         /// <param name="sender"></param>
//         /// <param name="e"></param>
//         private void mouseMoveHandler(object sender, MouseEventArgs e)
//         {
//             // Ensure main plugin windo has focus -> workaround for click event that doesn't work if main window has no focus
//             // If main window has no focus, the click event gives focus to main window and no click event on button occurs
//             this.onRequestFocusEvent?.Invoke(this);

//             var new_isHovering = sectorData.isPointInShape(e.Location);
//             if (new_isHovering != isHovering)
//             {
//                 isHovering = new_isHovering;
//                 if (isHovering)
//                 {
//                     onMouseOverButton.Invoke(this, properties); // Notify mouse is over the button
//                     System.Console.SetOut(RhinoApp.CommandLineOut);
//                     System.Console.WriteLine("mouse custom over control " + sectorData.buttonID);
//                 }
//                 else
//                 {
//                     onMouseLeaveButton.Invoke(this, properties);
//                     System.Console.SetOut(RhinoApp.CommandLineOut);
//                     System.Console.WriteLine("mouse custom Leave control " + sectorData.buttonID);
//                 }
//                 Invalidate();
//             }
//         }

//         private void enableStateChanged(object sender, EventArgs e)
//         {

//         }
//         private void mouseEnterHandler(object sender, MouseEventArgs e)
//         {
//             // var new_isHovering = _sectorData.isPointInShape(e.Location);
//             // if (new_isHovering != isHovering)
//             // {
//             //     isHovering = new_isHovering;
//             //     Invalidate();
//             // }

//         }
//         private void mouseLeaveHandler(object sender, MouseEventArgs e)
//         {
//             isHovering = false;
//             onMouseLeaveButton.Invoke(this, properties);
//             Invalidate();
//             System.Console.SetOut(RhinoApp.CommandLineOut);
//             System.Console.WriteLine("mouse leave control " + sectorData.buttonID);
//         }

//         private void dragEnterHandler(object sender, DragEventArgs e)
//         {
//             // Ensure mouse cursor if hovering arc sector
//             OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
//             if (isHovering)
//             {
//                 isDraggingIcon = true;
//                 if (dragSourceType(e.Source) == DragSourceTypes.self)
//                 {
//                     if (((SectorArcButton)e.Source).ID == ID)
//                     {
//                         e.Effects = DragEffects.Link;
//                     }
//                 }
//             }
//         }

//         private void dragLeaveHandler(object sender, DragEventArgs e)
//         {
//             // Ensure mouse cursor if leaving arc sector
//             OnMouseLeave(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
//             e.Effects = DragEffects.All;
//             if (!isHovering)
//             {
//                 isDraggingIcon = false;
//                 e.Effects = DragEffects.All;
//             }

//             var dSource = dragSourceType(e.Source);
//             if (dSource == DragSourceTypes.self && ((SectorArcButton)e.Source).ID == ID)
//             {
//                 properties.isActive = false;
//                 onNewIconAdded.Invoke(this, properties); // update settings
//                 Invalidate();
//             }
//         }
//         private void dragOverHandler(object sender, DragEventArgs e)
//         {
//             // Ensure mouse cursor if hovering arc sector
//             OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
//             if (!isHovering)
//             {
//                 isDraggingIcon = false;
//             }
//             else
//             {
//                 e.Effects = DragEffects.Copy;
//             }
//         }
//         private void dragDropHandler(object sender, DragEventArgs e)
//         {
//             // Ensure mouse cursor if hovering arc sector
//             OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));

//             if (isHovering)
//             {
//                 switch (dragSourceType(e.Source))
//                 {
//                     case DragSourceTypes.self:
//                         if (((SectorArcButton)e.Source).ID == ID)
//                         {
//                             // If drag drop and button had previously an icon/script, restore it
//                             properties.isActive = properties.icon != null ? true : false;
//                             Invalidate();
//                         }
//                         else
//                         {
//                             properties.icon = ((SectorArcButton)e.Source).properties.icon; //TODO: Ensure icon data are copied to new properties
//                             properties.rhinoScript = ((SectorArcButton)e.Source).properties.rhinoScript;
//                             properties.isActive = true;
//                             Invalidate();
//                         }
//                         break;
//                     case DragSourceTypes.rhinoItem:
//                         var rhinoToolbarItem = getDroppedToolbarItem(e);
//                         properties.icon = rhinoToolbarItem.icon;
//                         properties.rhinoScript = rhinoToolbarItem.script;
//                         properties.isActive = true;
//                         Invalidate();
//                         break;
//                     default:
//                         break;
//                 }
//                 onNewIconAdded.Invoke(this, properties);
//             }
//         }

//         /// <summary>
//         /// Get type of object dragged into this control
//         /// </summary>
//         /// <param name="source"></param>
//         /// <returns></returns>
//         private DragSourceTypes dragSourceType(Control source)
//         {
//             if (source.GetType() == typeof(SectorArcButton))
//             {
//                 return DragSourceTypes.self;
//             }
//             else if (source.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
//             {
//                 return DragSourceTypes.rhinoItem;
//             }
//             else
//             {
//                 return DragSourceTypes.unknown;
//             }
//         }

//         /// <summary>
//         ///  <para>When a drop event occurs (dragLeave), check if drop item is a Rhino toolbar item</para>
//         ///  <para>If so, try to get the script command macro and the associated icon</para>
//         /// </summary>
//         /// <param name="e"></param>
//         /// <returns></returns>
//         private RhinoToolBarItem getDroppedToolbarItem(DragEventArgs e)
//         {
//             var obj = e.Source;
//             if (obj != null)
//             {
//                 if (obj.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
//                 {
//                     // Get the macro of the dropped toolbar item
//                     var lMacro = obj.GetType().GetProperty("LeftMacro").GetValue(obj, null);

//                     // Seems that "CreateIcon" is a good condidate to get the Rhino toolbar item icon
//                     var iconCreateMethod = lMacro.GetType().GetMethod("CreateIcon");
//                     var icon = (Eto.Drawing.Icon)iconCreateMethod.Invoke(lMacro, new object[] { new Eto.Drawing.Size(28, 28), true });
//                     if (icon != null)
//                     {
//                         // Get the macro "script" command
//                         var macroScript = lMacro.GetType().GetProperty("Script").GetValue(lMacro, null);
//                         return new RhinoToolBarItem((string)macroScript, icon);
//                     }
//                     else
//                     {
//                         return null;
//                     }
//                 }
//                 else
//                 {
//                     return null;
//                 }
//             }
//             return null;
//         }
//     }
// }