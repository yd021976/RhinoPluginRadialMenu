using System.Runtime.CompilerServices;

namespace customControls
{
    public struct SectorArcRadialControlEventArgs
    {
        public SectorArcButton button;
        public ButtonModel model;
        public bool isDragging = false;
        public SectorArcRadialControlEventArgs(SectorArcButton button, ButtonModel model, bool isDragging)
        {
            this.button = button;
            this.model = model;
            this.isDragging = isDragging;
        }
    }
    public partial class SectorArcRadialControl
    {

        #region Events
        /// <summary>
        /// Button click event
        /// </summary>
        public event clickEvent onClickEvent;
        /// <summary>
        /// Event for button click
        /// </summary>
        /// <param name="sender"></param>
        public delegate void clickEvent(SectorArcRadialControl sender, SectorArcRadialControlEventArgs args);

        /// <summary>
        /// event to ensure main window has focus
        /// </summary>
        public event focusRequestedEvent onFocusRequested;
        /// <summary>
        /// delegate for event to ensure main window has focus
        /// </summary>
        /// <param name="sender"></param>
        public delegate void focusRequestedEvent(SectorArcRadialControl sender, SectorArcRadialControlEventArgs args);

        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event newIconAdded onNewIconAdded;
        /// <summary>
        /// delegate for Event to notify an icon has been added to button
        /// </summary>
        /// <param name="sender"></param>
        public delegate void newIconAdded(SectorArcRadialControl sender, SectorArcRadialControlEventArgs args);
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event mouseOverButton onMouseOverButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="button"></param>
        public delegate void mouseOverButton(SectorArcRadialControl sender, SectorArcRadialControlEventArgs args);
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event mouseLeaveButton onMouseLeaveButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="sender"></param>
        public delegate void mouseLeaveButton(SectorArcRadialControl sender, SectorArcRadialControlEventArgs args);
        #endregion
    }
}