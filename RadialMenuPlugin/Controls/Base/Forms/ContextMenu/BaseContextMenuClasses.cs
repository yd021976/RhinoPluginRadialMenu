using System;
using System.ComponentModel;
using Eto.Forms;
using RadialMenuPlugin.Data;

namespace RadialMenuPlugin.Controls.Base.ContextMenu
{
    /// <summary>
    /// Base class that implement Notify property changed
    /// </summary>
    public abstract class BaseINotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void _OnDataPropertiesChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    /// <summary>
    /// Interface for data bindings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    public interface IContextMenuBinding<T, D> where T : Control where D : BaseINotifyPropertyChanged
    {
        /// <summary>
        /// Binding to Model object
        /// </summary>
        public BindableBinding<T, D> ModelBinding { get; }
    }


    /// <summary>
    /// Base contents abstract class for any data type. Could be subclassed if need specialized feature, specifically on data (D)
    /// </summary>
    /// <typeparam name="D"></typeparam>
    public abstract class BaseContent<D> : StackLayout, IContextMenuBinding<BaseContent<D>, D> where D : BaseINotifyPropertyChanged, new()
    {
        public BindableBinding<BaseContent<D>, D> ModelBinding => _ModelBinding;
        protected D _Model = new D();
        /// <summary>
        /// Override this binding to add some "model" event handler or other logic when assigning new binding object
        /// </summary>
        protected virtual BindableBinding<BaseContent<D>, D> _ModelBinding => new BindableBinding<BaseContent<D>, D>(
                         this,
                         (BaseContent<D> obj) => obj._Model,
                         // Update "model" property with new value and register a property changed event handler of the "model" object
                         delegate (BaseContent<D> obj, D value)
                         {
                             // Remove property changed event handler on current "model" object
                             if (_Model != null)
                             {
                                 _Model.PropertyChanged -= _ModelChangedHandler;
                             }
                             // update property
                             obj._Model = value;
                             // Add property changed handler on "model"
                             _Model.PropertyChanged += _ModelChangedHandler;
                             _UpdateModelBindings();
                         },
                         // Add change event handler
                         delegate (BaseContent<D> menu, EventHandler<EventArgs> changeEventHandler)
                         { },
                         // remove change event handler
                         delegate (BaseContent<D> menu, EventHandler<EventArgs> changeEventHandler)
                         { }
                         );
        protected abstract void _UpdateModelBindings();
        protected abstract void _ModelChangedHandler(object sender, PropertyChangedEventArgs e);
    }
}