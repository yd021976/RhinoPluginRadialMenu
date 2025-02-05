using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AppKit;
using Eto.Drawing;
using Eto.Forms;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.States;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.Types.Images;
using RadialMenuPlugin.Utilities.Events;

namespace RadialMenuPlugin.Controls.Buttons.Shaped.Base
{
    using MouseEventHandler = AppEventHandler<object, MouseEventArgs>; // Used for mouse events
    public interface IButton
    {
        void RunAnimation(Action[] renderers);
    }

    /// <summary>
    /// Base class to implement a shaped button.
    /// <para>
    /// This class provides all basic functionalities of a shaped button, like mouse enter, hover, leave and clicks.
    /// Any class that derive from this abstract class MUST provide images for each button state and rules to change button visual for each state
    /// </para>
    /// <para>
    /// 1 - Provide visual changes by override any of the <code>Animation****Handler</code>
    /// 2 - Provide image by setting the <see cref="ImageList"/> property
    /// </para>
    /// </summary>
    public abstract class ShapedButton : PixelLayout, IButton, INotifyPropertyChanged
    {
        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        #region Events declaration
        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Button click event
        /// </summary>
        public event MouseEventHandler OnButtonClickEvent;
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event MouseEventHandler OnButtonMouseMove;
        /// <summary>
        /// Event to notify mouse leaves a button
        /// </summary>
        public event MouseEventHandler OnButtonMouseLeave;
        /// <summary>
        /// Event to notify mouse enters a button
        /// </summary>
        public event MouseEventHandler OnButtonMouseEnter;
        #endregion

        #region Protected properties
        /// <summary>
        /// 
        /// </summary>
        protected AnimationProperties AnimationProperties = new AnimationProperties();
        protected BitmapData LockedBitmapData;
        protected virtual ShapedButtonImageButtonList Buttons { get; private set; } = new ShapedButtonImageButtonList();
        protected virtual StatePool StatePool { get; set; }
        protected State CurrentState;
        #endregion


        #region Constructor
        public ShapedButton() : base()
        {
            StatePool = new StatePool() {
                {
                    typeof(DefaultState), new DefaultState(RunAnimation, new List<Action>(){AnimationNormalHandler}.ToArray())
                },
                {
                    typeof(HoverState), new HoverState(RunAnimation, new List<Action>{AnimationHoverHandler}.ToArray())
                },
                {
                    typeof(DisableState), new DisableState(RunAnimation, new List<Action>(){AnimationDisabledHandler}.ToArray())
                },
            };
            CurrentState = StatePool[typeof(DefaultState)];
            InitEventHandlers();
        }
        public ShapedButton(ShapedButtonImageList images) : this()
        {
            ImageList = images;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShapedButton()
        {
            if (LockedBitmapData != null) LockedBitmapData.Dispose();
        }
        #endregion


        #region Public methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderers"></param>
        public void RunAnimation(Action[] renderers)
        {
            NSAnimationContext.RunAnimation((context) =>
           {
               context.Duration = AnimationProperties.AnimationDuration;
               context.AllowsImplicitAnimation = true;
               foreach (var renderFx in renderers)
               {
                   renderFx();
               }
           });
        }
        public virtual ShapedButtonImageList ImageList { get; protected set; }
        #endregion


        #region Protected methods
        /// <summary>
        /// 
        /// </summary>
        protected virtual void InitEventHandlers()
        {
            MouseMove += MouseMoveHandler;
            MouseEnter += MouseEnterHandler;
            MouseLeave += MouseLeaveHandler;
            MouseDown += MouseDownHandler;
            MouseUp += MouseUpHandler;
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            if (IsPointInShape(new Point(e.Location)))
            {
                HandleStateChange(ButtonEvent.Enter, OnButtonMouseEnter, e);
            }
        }
        protected virtual void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (IsPointInShape(new Point(e.Location)))
            {
                if (CurrentState.GetType() == typeof(HoverState))
                {
                    // No state change because we already are in "hover" state : Just raise "mouse move" event
                    RaiseEvent(OnButtonMouseMove, e);
                }
                else
                {
                    HandleStateChange(ButtonEvent.Enter, OnButtonMouseEnter, e);
                }
            }
            else // Fire "leave" event if mouse over location is not in shape
            {
                HandleStateChange(ButtonEvent.Exit, OnButtonMouseLeave, e);
            }
        }
        protected virtual void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            HandleStateChange(ButtonEvent.Exit, OnButtonMouseLeave, e);
        }
        protected virtual void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (IsPointInShape(new Point(e.Location)))
            {
                HandleStateChange(ButtonEvent.MouseDown, OnButtonClickEvent, e);
            }
        }
        protected virtual void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (IsPointInShape(new Point(e.Location)))
            {
                HandleStateChange(ButtonEvent.MouseUp, null, e); // Will not raise event
            }
        }

        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
        /// <summary>
        /// Invoke provided event handler <paramref name="action"/> if not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        protected void RaiseEvent<E>(AppEventHandler<object, E> action, E e)
        {
            action?.Invoke(this, e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected bool IsPointInShape(Point point)
        {
            var isInShape = false;
            var color = LockedBitmapData.GetPixel(point);
            if (color.A != 0) isInShape = true;
            return isInShape;
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void InitButtons()
        {
            var filteredImageList = ImageList.Where(o => o.Key != ShapedButtonImageNames.mask);
            foreach (var imagePair in filteredImageList)
            {
                var button = new ImageButton(imagePair.Value.Image, imagePair.Value.Image.Size);
                Buttons.Add(imagePair.Key, button);
                Add(button, 0, 0);
            }
        }
        /// <summary>
        /// Set a new image list
        /// </summary>
        /// <param name="images"></param>
        /// <exception cref="Exception"></exception>
        protected void SetImageList(ShapedButtonImageList images)
        {
            clearButtons();
            if (LockedBitmapData != null) LockedBitmapData.Dispose();
            ImageList = images;
            if (!ImageList.ContainsKey(ShapedButtonImageNames.mask)) throw new Exception("Image list must contains a \"mask\" key bitmap");
            try
            {
                LockedBitmapData = new Bitmap(ImageList[ShapedButtonImageNames.mask].Image).Lock(); // Keep ref to lock bitmap data
                InitButtons();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
        /// <summary>
        /// Remove all buttons and dispoe all image. Should be called when a new image list is provided
        /// </summary>
        private void clearButtons()
        {
            // Clear previous buttons
            RemoveAll();

            // Imagelist can be null, avoid exception here with try/catch
            try
            {
                foreach (var image in ImageList)
                {
                    image.Value.Image.Dispose();
                }
                ImageList.Clear();
            }
            catch { }
            Buttons.Clear();
        }
        /// <summary>
        /// Given a button event, check if state will change. If so, raise event with <paramref name="buttonEventHandler"/> and <paramref name="raiseButtonEvent"/>
        /// <para>
        /// Set <paramref name="buttonEventHandler"/> to null if no event handler should be invoked
        /// </para>
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="buttonEventType"></param>
        /// <param name="buttonEventHandler"></param>
        /// <param name="raiseButtonEvent"></param>
        protected void HandleStateChange<E>(ButtonEvent buttonEventType, AppEventHandler<object, E> buttonEventHandler, E raiseButtonEvent)
        {
            var nextState = CurrentState.NextState(buttonEventType, StatePool);
            if (nextState != CurrentState)
            {
                CurrentState = nextState;
                RaiseEvent(buttonEventHandler, raiseButtonEvent);
            }
        }
        #endregion


        #region Animation handlers to override
        /// <summary>
        /// 
        /// </summary>
        protected virtual void AnimationNormalHandler() { }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void AnimationHoverHandler() { }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void AnimationDisabledHandler() { }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void AnimationSelectedHandler() { }
        #endregion
    }
}