using System;
using System.Collections.Generic;
using Toggl.Multivac;

namespace Toggl.Foundation.Sync
{
    public sealed class TransitionHandlerProvider : ITransitionHandlerProvider
    {
        private readonly Dictionary<IStateResult, TransitionHandler> transitionHandlers
            = new Dictionary<IStateResult, TransitionHandler>();
        
        public void ConfigureTransition(IStateResult result, Func<IObservable<ITransition>> stateFactory)
        {
            Ensure.Argument.IsNotNull(result, nameof(result));
            Ensure.Argument.IsNotNull(stateFactory, nameof(stateFactory));

            transitionHandlers.Add(result, _ => stateFactory());
        }

        public void ConfigureTransition<T>(StateResult<T> result, Func<T, IObservable<ITransition>> stateFactory)
        {
            Ensure.Argument.IsNotNull(result, nameof(result));
            Ensure.Argument.IsNotNull(stateFactory, nameof(stateFactory));

            transitionHandlers.Add(
                result,
                t => stateFactory(((Transition<T>)t).Parameter)
            );
        }

        public TransitionHandler GetTransitionHandler(IStateResult result)
        {
            Ensure.Argument.IsNotNull(result, nameof(result));

            transitionHandlers.TryGetValue(result, out var handler);
            return handler;
        }
    }
}
