using System;
using System.Collections.Generic;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.States;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.Types;


namespace RadialMenuPlugin.Controls.Buttons.Shaped.Base
{
    /// <summary>
    /// Default state handler
    /// </summary>
    public class DefaultState : State
    {
        public DefaultState(Action<Action[]> renderer, Action[] renderers) : base(renderer,renderers) { }

        public override State NextState(IBaseEnumKey action, StatePool statePool,Dictionary<EnumKey, State> concurrentStates = null)
        {
            State nextState = this;
            switch (action)
            {
                case IBaseEnumKey i when i == ButtonEvent.Enter:
                    nextState = statePool[typeof(HoverState)];
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
        public override State NextState(IBaseEnumKey action, StatePool statePool,Dictionary<EnumKey, State> concurrentStates = null)
        {
            State nextState = this;

            switch (action)
            {
                case IBaseEnumKey i when i == ButtonEvent.Exit:
                    nextState = statePool[PreviousState.GetType()];
                    nextState.EnterState();
                    break;
                default:
                    break;
            }
            return nextState;
        }
    }
    /// <summary>
    /// Disabled state handler
    /// </summary>
    public class DisableState : State
    {
        public DisableState(Action<Action[]> render, Action[] renderers) : base(render, renderers) { }
        public override State NextState(IBaseEnumKey action, StatePool statePool,Dictionary<EnumKey, State> concurrentStates = null)
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