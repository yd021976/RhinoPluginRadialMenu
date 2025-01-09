using System;
using System.ComponentModel;
using Eto.Forms;
using RadialMenuPlugin.Data;

namespace RadialMenuPlugin.Controls.ContextMenu.Base
{
    /// <summary>
    /// Define interface for contextual menu data bindings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    public interface IContextMenuBinding<T, D> where T : Control where D : Model
    {
        /// <summary>
        /// Binding to Model object
        /// </summary>
        public BindableBinding<T, D> ModelBinding { get; }
    }
    /// <summary>
    /// Abstract class for contextual menu contents
    /// </summary>
    public abstract class ContextMenuContent<D> : Panel, IContextMenuBinding<ContextMenuContent<D>, D> where D : Model
    {
        #region  public properties
        public BindableBinding<ContextMenuContent<D>, D> ModelBinding => _ModelBinding;
        #endregion
        #region protected/private properties
        /// <summary>
        /// 
        /// </summary>
        protected StackLayout _Layout = new StackLayout();
        /// <summary>
        /// 
        /// </summary>
        protected D _Model;
        /// <summary>
        /// 
        /// </summary>
        protected BindableBinding<ContextMenuContent<D>, D> _ModelBinding => new BindableBinding<ContextMenuContent<D>, D>(
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
        {

        }
        #endregion
        #region protected/private methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected abstract void _ModelChangedHandler(object sender, PropertyChangedEventArgs e);
        #endregion
    }
    /// <summary>
    /// Base abstract class to display a Borderless context menu
    /// <para>
    /// T is the type of contents
    /// </para>
    /// </summary>
    public abstract class ContextMenuForm<CONTENTS, DATA> : Form where CONTENTS : ContextMenuContent<DATA> where DATA : Model
    {
        #region public properties
        public DATA Model
        {
            get => _Model;
            set
            {
                _Contents.ModelBinding.Unbind();
                _Model = value;
                _Contents.ModelBinding.Bind(this, obj => obj.Model);
            }
        }
        #endregion

        #region protected/private properties
        protected DATA _Model;
        protected ContextMenuContent<DATA> _Contents;
        #endregion
        #region public methods
        public ContextMenuForm() : base()
        {
            WindowStyle = WindowStyle.None;
            AutoSize = false;
            Resizable = false;
            Topmost = true;
            ShowActivated = false;
        }
        #endregion
        #region protected/private methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _ModelChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Model.Data):
                case nameof(Model.Data.Properties):
                    break;
                default: break;
            }
        }
        #endregion
    }
}