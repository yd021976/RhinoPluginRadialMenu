using System;
using System.Collections.Generic;
using System.ComponentModel;
using Eto.Drawing;

namespace RadialMenuPlugin.Data
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ButtonPropertiesList : List<KeyValuePair<string, string>> { }
    public struct Macro
    {
        public string Script = "";
        public string Tooltip = "";
        public Macro(string script, string tooltip)
        {
            Script = script;
            Tooltip = tooltip;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ButtonProperties : INotifyPropertyChanged
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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
        private Macro _LeftMacro;
        /// <summary>
        /// Rhino script to execute
        /// </summary>
        public Macro LeftMacro
        {
            get => _LeftMacro; set
            {
                _LeftMacro = value;
                OnPropertyChanged(nameof(LeftMacro));
            }
        }
        /// <summary>
        /// Rhino script to execute
        /// </summary>
        private Macro _RightMacro;
        /// <summary>
        /// Rhino script to execute
        /// </summary>
        public Macro RightMacro
        {
            get => _RightMacro; set
            {
                _RightMacro = value;
                OnPropertyChanged(nameof(RightMacro));
            }
        }
        /// <summary>
        /// Icon of the command
        /// </summary>
        protected Icon _Icon;
        /// <summary>
        /// Icon of the command
        /// </summary>
        public Icon Icon { get => _Icon; set { _Icon = value; OnPropertyChanged(nameof(Icon)); } }
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
            LeftMacro = new Macro();
        }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="leftMacro"></param>
        /// <param name="icon"></param>
        public ButtonProperties(Macro leftMacro, Macro rightMacro, Icon icon) : this()
        {
            LeftMacro = leftMacro;
            RightMacro = rightMacro;
            Icon = icon;
        }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="script"></param>
        /// <param name="icon"></param>
        /// <param name="isActive"></param>
        public ButtonProperties(Macro leftMacro, Macro rightMacro, Icon icon, bool isActive) : this(leftMacro, rightMacro, icon)
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
        public ButtonProperties(Macro leftMacro, Macro rightMacro, Eto.Drawing.Icon icon, bool isActive, bool isFolder) : this(leftMacro, rightMacro, icon, isActive)
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
        public ButtonProperties(Macro leftMacro, Macro rightMacro, Icon icon, bool isActive, bool isFolder, Guid macroGuid) : this(leftMacro, rightMacro, icon, isActive, isFolder)
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
            try
            {
                IsFolder = rhinoSettings[nameof(IsFolder)] == true.ToString() ? true : false;
                IsActive = rhinoSettings[nameof(IsActive)] == true.ToString() ? true : false;
                LeftMacro = new Macro(rhinoSettings["LeftMacroScript"], rhinoSettings["LeftMacroTooltip"]);
                RightMacro = new Macro(rhinoSettings["RightMacroScript"], rhinoSettings["RightMacroTooltip"]);
                CommandGUID = Guid.Parse(rhinoSettings[nameof(CommandGUID)]);
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }
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
                new KeyValuePair<string, string>("LeftMacroScript",LeftMacro.Script ),
                new KeyValuePair<string, string>("LeftMacroTooltip",LeftMacro.Tooltip ), 
                new KeyValuePair<string, string>("RightMacroScript",RightMacro.Script ), 
                new KeyValuePair<string, string>("RightMacroTooltip",RightMacro.Tooltip ),
                new KeyValuePair<string, string>(nameof(IsActive), IsActive.ToString()),
                new KeyValuePair<string, string>(nameof(IsFolder), IsFolder.ToString())
            };
            return list;
        }
    }
}