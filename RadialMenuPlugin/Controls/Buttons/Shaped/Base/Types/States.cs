using System;
using System.Collections.Generic;
using NLog;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.Types;

namespace RadialMenuPlugin.Controls.Buttons.Shaped.Base.States
{
    /// <summary>
    /// Pool of state objects. Used to assign a state type to a state instance
    /// </summary>
    public class StatePool : Dictionary<Type, State> { }

    /// <summary>
    /// State interface. Use it for state abstract class, dont use directly in state class
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Previous state : Usefull for "intermediate" state like "hover" that needs to restore previous state when "exit" action
        /// </summary>
        public State PreviousState { get; set; }

        public State NextState(IBaseEnumKey action, StatePool statePool, Dictionary<EnumKey, State> concurrentStates = null);
        public void EnterState();
    }
    /// <summary>
    /// Base class for implementating states
    /// </summary>
    public abstract class State : IState
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Render functions to call to update visual aspect of button
        /// </summary>
        protected Action[] Renderers;
        /// <summary>
        /// Function to render visuals 
        /// </summary>
        protected Action<Action[]> RenderHandler;
        protected State _PreviousState;
        public State PreviousState
        {
            get => _PreviousState;
            set
            {
                _PreviousState = value;
            }
        }
        public State(Action<Action[]> renderer, Action[] renderers) { Renderers = renderers; RenderHandler = renderer; }

        public abstract State NextState(IBaseEnumKey action, StatePool statePool, Dictionary<EnumKey, State> concurrentStates = null);

        public void EnterState()
        {
            RenderHandler(Renderers);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return GetType() == obj.GetType();
        }
    }
    public class ButtonEvent : EnumKey
    {
        private ButtonEvent(string key) : base(key) { }
        public static readonly ButtonEvent Enter = new ButtonEvent("enter");
        public static readonly ButtonEvent Exit = new ButtonEvent("exit");
        public static readonly ButtonEvent MouseDown = new ButtonEvent("mouseDown");
        public static readonly ButtonEvent MouseUp = new ButtonEvent("mouseUp");
        public static readonly ButtonEvent Disable = new ButtonEvent("disable");
        public static readonly ButtonEvent Default = new ButtonEvent("default");
    }
}