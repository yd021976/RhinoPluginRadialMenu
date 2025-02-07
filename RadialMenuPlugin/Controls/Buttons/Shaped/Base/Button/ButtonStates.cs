using System;
using System.Collections.Generic;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.States;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.Types;

///
/// State flow is described in freeform <see href="https://www.icloud.com/freeform/036L6shKtvcdN1cYQAIbx5m7A#Button_States"/>
///
namespace RadialMenuPlugin.Controls.Buttons.Shaped.Base
{
    /// <summary>
    /// Default state handler
    /// </summary>
    public class DefaultState : State
    {
        public DefaultState(Action<Action[]> renderer, Action[] renderers) : base(renderer, renderers) { }

        public override State NextState(IBaseEnumKey action, StatePool statePool, Dictionary<EnumKey, State> concurrentStates = null)
        {
            State nextState = this;
            switch (action)
            {
                case IBaseEnumKey i when i == ButtonEvent.Enter:
                    nextState = statePool[typeof(HoverState)];
                    nextState.PreviousState = this;
                    nextState.EnterState();
                    break;
                case IBaseEnumKey i when i == ButtonEvent.MouseDown:
                    nextState = statePool[typeof(MouseDownState)];
                    nextState.PreviousState = this;
                    nextState.EnterState();
                    break;
                default:
                    break;
            }
            return nextState;
        }
    }

    /// <summary>
    /// Hover state handler
    /// </summary>
    public class HoverState : State
    {
        public HoverState(Action<Action[]> render, Action[] renderers) : base(render, renderers) { }
        public override State NextState(IBaseEnumKey action, StatePool statePool, Dictionary<EnumKey, State> concurrentStates = null)
        {
            State nextState = this;

            switch (action)
            {
                case IBaseEnumKey i when i == ButtonEvent.Exit:
                    nextState = statePool[PreviousState.GetType()];
                    nextState.EnterState();
                    break;
                case IBaseEnumKey i when i == ButtonEvent.MouseDown:
                    Logger.Debug($"Mouse hover state to mouse down state");
                    nextState = statePool[typeof(MouseDownState)];
                    nextState.PreviousState = this;
                    nextState.EnterState();
                    break;
                default:
                    break;
            }
            return nextState;
        }
    }

    /// <summary>
    /// Mouse down state handler
    /// </summary>
    public class MouseDownState : State
    {
        public MouseDownState(Action<Action[]> render, Action[] renderers) : base(render, renderers) { }
        public override State NextState(IBaseEnumKey action, StatePool statePool, Dictionary<EnumKey, State> concurrentStates = null)
        {
            State nextState = this;

            switch (action)
            {
                case IBaseEnumKey i when i == ButtonEvent.MouseUp || i == ButtonEvent.Default || i == ButtonEvent.Disable || i == ButtonEvent.Enter || i == ButtonEvent.Exit:
                    Logger.Debug($"Mouse down state to previous state");
                    nextState = statePool[PreviousState.GetType()];
                    nextState.EnterState();
                    break;
                default:
                    break;
            }
            return nextState;
        }
    }
    public class DisableState : State
    {
        public DisableState(Action<Action[]> render, Action[] renderers) : base(render, renderers) { }
        public override State NextState(IBaseEnumKey action, StatePool statePool, Dictionary<EnumKey, State> concurrentStates = null)
        {
            State nextState = this;

            switch (action)
            {
                case IBaseEnumKey i when i == ButtonEvent.Default:
                    nextState = statePool[typeof(DefaultState)];
                    nextState.EnterState();
                    break;
                default:
                    break;
            }
            return nextState;
        }
    }
}