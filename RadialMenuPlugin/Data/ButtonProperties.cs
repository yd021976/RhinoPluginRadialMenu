using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RadialMenuPlugin.Data
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ButtonPropertiesList : List<KeyValuePair<string, string>> { }

    /// <summary>
    /// 
    /// </summary>
    public class ButtonProperties : INotifyPropertyChanged
    {
        /// <summary>
        /// Event to notigy a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Method to raise property changed event
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// GUID of Rhino command
        /// <para>Used to define icon filename</para>
        /// </summary>
        protected Guid _CommandGUID;
        /// <summary>
        /// GUID of Rhino command
        /// <para>Used to define icon filename</para>
        /// </summary>
        public Guid CommandGUID
        {
            get => _CommandGUID;
            set
            {
                _CommandGUID = value;
                OnPropertyChanged(nameof(CommandGUID));
            }
        }
        /// <summary>
        /// Rhino script to execute
        /// </summary>
        private string _RhinoScript;
        /// <summary>
        /// Rhino script to execute
        /// </summary>
        public string RhinoScript
        {
            get => _RhinoScript; set
            {
                _RhinoScript = value;
                OnPropertyChanged(nameof(RhinoScript));
            }
        }
        /// <summary>
        /// Icon of the command
        /// </summary>
        protected Eto.Drawing.Icon _Icon;
        /// <summary>
        /// Icon of the command
        /// </summary>
        public Eto.Drawing.Icon Icon { get => _Icon; set { _Icon = value; OnPropertyChanged(nameof(Icon)); } }
        /// <summary>
        /// Define if icon/command should be presented as a folder in UI
        /// </summary>
        bool _IsFolder = false;
        /// <summary>
        /// Define if icon/command should be presented as a folder in UI
        /// </summary>
        public bool IsFolder { get => _IsFolder; set { _IsFolder = value; OnPropertyChanged(nameof(IsFolder)); } }
        /// <summary>
        /// Define if icon/command is active in the UI
        /// </summary>
        bool _IsActive = true;
        /// <summary>
        /// Define if icon/command is active in the UI
        /// </summary>
        public bool IsActive { get => _IsActive; set { _IsActive = value; OnPropertyChanged(nameof(IsActive)); } }
        /// <summary>
        /// Ctor
        /// </summary>
        public ButtonProperties() : base()
        {
            Icon = null;
            IsActive = false;
            RhinoScript = "";
        }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="script"></param>
        /// <param name="icon"></param>
        public ButtonProperties(string script, Eto.Drawing.Icon icon) : this()
        {
            RhinoScript = script;
            Icon = icon;
        }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="script"></param>
        /// <param name="icon"></param>
        /// <param name="isActive"></param>
        public ButtonProperties(string script, Eto.Drawing.Icon icon, bool isActive) : this(script, icon)
        {
            IsActive = isActive;
        }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="script"></param>
        /// <param name="icon"></param>
        /// <param name="isActive"></param>
        /// <param name="isFolder"></param>
        public ButtonProperties(string script, Eto.Drawing.Icon icon, bool isActive, bool isFolder) : this(script, icon, isActive)
        {
            IsFolder = isFolder;
        }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="script"></param>
        /// <param name="icon"></param>
        /// <param name="isActive"></param>
        /// <param name="isFolder"></param>
        public ButtonProperties(string script, Eto.Drawing.Icon icon, bool isActive, bool isFolder, Guid macroGuid) : this(script, icon, isActive, isFolder)
        {
            CommandGUID = macroGuid;
        }
        /// <summary>
        /// Create new object from rhino settings
        /// <para>
        /// REMARK: The Icon creation is up to the caller of this method, we only retrieve the icon GUID name.<br/>
        /// This is because this class shouldn't be aware of the storage location<br/>
        /// The storage location should be managed by class <see cref="SettingsHelper"/> 
        /// </para>
        /// </summary>
        /// <param name="rhinoSettings"></param>
        public ButtonProperties(Dictionary<string, string> rhinoSettings)
        {
            IsFolder = rhinoSettings[nameof(IsFolder)] == true.ToString() ? true : false;
            IsActive = rhinoSettings[nameof(IsActive)] == true.ToString() ? true : false;
            RhinoScript = rhinoSettings[nameof(RhinoScript)];
            CommandGUID = Guid.Parse(rhinoSettings[nameof(CommandGUID)]);
        }
        /// <summary>
        /// Convert class instance to list of key/value pair to integrate into Rhino settings
        /// </summary>
        /// <returns></returns>
        public ButtonPropertiesList toList()
        {
            var commandGUID = CommandGUID.ToString();
            var list = new ButtonPropertiesList
            {
                new KeyValuePair<string, string>(nameof(Icon),commandGUID ), // Sets the name of the icon
                new KeyValuePair<string, string>(nameof(CommandGUID),commandGUID ), // Sets the guid of the command
                new KeyValuePair<string, string>(nameof(RhinoScript),RhinoScript ), // Sets the command to execute
                new KeyValuePair<string, string>(nameof(IsActive), IsActive.ToString()),
                new KeyValuePair<string, string>(nameof(IsFolder), IsFolder.ToString())
            };
            return list;
        }
    }
}