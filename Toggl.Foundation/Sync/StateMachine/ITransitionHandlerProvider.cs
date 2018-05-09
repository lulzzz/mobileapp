using System;

namespace Toggl.Foundation.Sync
{
    public delegate IObservable<ITransition> TransitionHandler(ITransition transition);

    public interface ITransitionHandlerProvider
    {
        TransitionHandler GetTransitionHandler(IStateResult stateResult);
    }
}
