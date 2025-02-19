using System;
using AppKit;
using Eto.Forms;
using RadialMenuPlugin.Controls.Base.ContextMenu;
using RadialMenuPlugin.Data;

namespace RadialMenuPlugin.Controls.ContextMenu.MenuButton
{ 
    /// <summary>
    /// Abstract class for menu contents based on data from "Model" class
    /// </summary>
    public abstract class ContextMenuContent<D> : BaseContent<D> where D : Model, new()
    {
        #region  public properties
        /// <summary>
        /// 
        /// </summary>
        // protected D _Model = new D();
        #endregion
        #region protected/private properties

        /// <summary>
        /// 
        /// </summary>
        protected new BindableBinding<ContextMenuContent<D>, D> _ModelBinding => new BindableBinding<ContextMenuContent<D>, D>(
                           this,
                           (ContextMenuContent<D> obj) => obj._Model,
                           // Update "model" property with new value and register a property changed event handler of the "model" object
                           delegate (ContextMenuContent<D> obj, D value)
                           {
                               // Remove property changed event handler on current "model" object
                               if (_Model != null)
                               {
                                   _Model.PropertyChanged -= _ModelChangedHandler;
                                   _Model.Data.PropertyChanged -= _ModelChangedHandler;
                                   _Model.Data.Properties.PropertyChanged -= _ModelChangedHandler;
                               }
                               // update property
                               obj._Model = value;
                               // Add property changed handler on "model"
                               _Model.PropertyChanged += _ModelChangedHandler;
                               _Model.Data.PropertyChanged += _ModelChangedHandler;
                               _Model.Data.Properties.PropertyChanged += _ModelChangedHandler;
                               _UpdateModelBindings();
                           },
                           // Add change event handler
                           delegate (ContextMenuContent<D> menu, EventHandler<EventArgs> changeEventHandler)
                           { },
                           // remove change event handler
                           delegate (ContextMenuContent<D> menu, EventHandler<EventArgs> changeEventHandler)
                           { }
                           );
        #endregion
        #region public methods
        /// <summary>
        /// 
        /// </summary>
        public ContextMenuContent() : base()
        { }
        #endregion
        #region protected/private methods
        #endregion
    }
    /// <summary>
    /// Base abstract class to display a Borderless context menu base on Eto.Form class
    /// <para>
    /// T is the type of contents
    /// </para>
    /// </summary>
    public abstract class BaseContextMenuForm<CONTENTS, DATA> : Form where CONTENTS : BaseContent<DATA> where DATA : BaseINotifyPropertyChanged, new()
    {

        #region public properties
        public new BaseContent<DATA> Content
        {
            get => _Contents;
            set
            {
                _Contents = value;
                base.Content = _Contents;
            }
        }
        public DATA Model
        {
            get => _Model;
            set
            {
                _Contents.ModelBinding.Unbind();
                _Model = value;
                _Contents.ModelBinding.Bind(this, "Model");
            }
        }
        #endregion

        #region protected/private properties
        protected DATA _Model;
        protected BaseContent<DATA> _Contents;
        #endregion
        #region public methods
        public BaseContextMenuForm() : base()
        {
            WindowStyle = WindowStyle.None;// No border/decoration
            AutoSize = false;
            Resizable = false;
            Topmost = true;
            ShowActivated = false;
            // Set context menu always above other windows. i.e. the radialmenu shouldn't overlap context menu
            var ctrlProp = Handler.GetType().GetProperty("Control");
            var nsWindow = (NSWindow)ctrlProp.GetValue(Handler, null);
            nsWindow.Level = NSWindowLevel.PopUpMenu;

            MouseLeave += (s, e) =>
            {
                if (_ShouldClose()) Close();
            };
        }
        #endregion
        #region protected/private methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // protected void _ModelChangedHandler(object sender, PropertyChangedEventArgs e)
        // {
        //     switch (e.PropertyName)
        //     {
        //         case nameof(Model.Data):
        //         case nameof(Model.Data.Properties):
        //             break;
        //         default: break;
        //     }
        // }
        /// <summary>
        /// Method is requested when menu should close. Default behavior is "true"
        /// <para>
        /// Override this method to change the behavior
        /// </para> 
        /// </summary>
        /// <returns>True if menu can close, false</returns>
        protected virtual bool _ShouldClose()
        {
            return true;
        }
        #endregion
    }
}