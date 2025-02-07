using System;
using System.Collections.Generic;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.States;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.Types;

namespace RadialMenuPlugin.Controls.Buttons.Shaped.Form.Center.States
{
    public class TooltipEvent : EnumKey
    {
        private TooltipEvent(string key) : base(key) { }
        public static readonly TooltipEvent Default = new TooltipEvent("default");
        // public static readonly TooltipEvent Hover = new TooltipEvent("hover");
        public static readonly TooltipEvent Tooltip = new TooltipEvent("tooltip");
    }
    public class ConcurrentStates : EnumKey
    {
        private ConcurrentStates(string key) : base(key) { }
        public static readonly ConcurrentStates MainState = new ConcurrentStates("mainState");
    }

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
                case IBaseEnumKey i when i == TooltipEvent.Tooltip:
                    Logger.Debug("Switch from default to tooltip");
                    nextState = statePool[typeof(TooltipState)];
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
    /// Display main button actions tooltip
    /// </summary>
    public class TooltipState : State
    {
        public TooltipState(Action<Action[]> renderer, Action[] renderers) : base(renderer, renderers) { }

        public override State NextState(IBaseEnumKey action, StatePool statePool, Dictionary<EnumKey, State> concurrentStates = null)
        {
            State nextState = this;
            switch (action)
            {
                case IBaseEnumKey i when i == TooltipEvent.Default:
                    Logger.Debug("Switch from Tooltip to default");
                    nextState = statePool[typeof(DefaultState)];
                    PreviousState = null;
                    nextState.EnterState();
                    break;

                case IBaseEnumKey i when i == TooltipEvent.Tooltip:
                    Logger.Debug("Switch from Tooltip to Tooltip");
                    PreviousState = this;
                    EnterState();
                    break;
                default:
                    break;
            }
            return nextState;
        }
    }
}