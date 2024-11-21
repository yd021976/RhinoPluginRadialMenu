namespace customControls
{
    /// <summary>
    /// Declaration of button events
    /// </summary>
    public partial class SectorArcButton
    {

        /// <summary>
        /// Button click event
        /// </summary>
        public event buttonClickEvent onButtonClickEvent;
        /// <summary>
        /// Delegate for event for button click
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonClickEvent(SectorArcButton sender);

        /// <summary>
        /// Event for requesting radial menu to get focus
        /// </summary>
        public event buttonRequestFocus onButtonRequestFocusEvent;
        /// <summary>
        /// Delegate for event for requesting radial menu to get focus
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonRequestFocus(SectorArcButton sender);

        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event buttonNewIconAdded onButtonNewIconAdded;
        /// <summary>
        /// Delegate for Event to notify an icon has been added to button
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonNewIconAdded(SectorArcButton sender);

        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event buttonMouseOver onButtonMouseOverButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonMouseOver(SectorArcButton sender);
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event buttonMouseLeaveButton onbuttonMouseLeaveButton;
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonMouseLeaveButton(SectorArcButton sender);
    }
}