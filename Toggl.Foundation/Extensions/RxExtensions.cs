using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Extensions
{
    public static class RxExtensions
    {
        public static IObservable<T> DelayedConditionalRetry<T>(
            this IObservable<T> source,
            int maxRetries,
            Func<int, TimeSpan> backOffStrategy,
            Func<Exception, bool> shouldRetryOn,
            IScheduler scheduler)
        {
            var currentAttempt = 0;
            
            IObservable<T> delayedSource()
            {
                var timeSpan = currentAttempt == 0 ? TimeSpan.Zero : backOffStrategy(currentAttempt);
                currentAttempt++;
                return source.DelaySubscription(timeSpan, scheduler);
            }

            (bool shouldCompleteWithSuccess, T objectToReturn, Exception) wrapSuccessfulResult(T objectOnSuccess) =>
                (shouldCompleteWithSuccess: true, objectOnSuccess, null);

            IObservable<(bool shouldCompleteWithSuccess, T, Exception)> proceedWithFailure(Exception exception) =>
                Observable.Return((shouldCompleteWithSuccess: false, default(T), exception));
            
            IObservable<(bool shouldCompleteWithSuccess, T, Exception)> triggerRetry(Exception exception) =>
                Observable.Throw<(bool, T, Exception)>(exception);

            IObservable<(bool shouldCompleteWithSuccess, T objectToReturn, Exception exception)>
            processException(Exception exception) => 
                shouldRetryOn(exception)
                ? triggerRetry(exception)
                : proceedWithFailure(exception);
            
            IObservable<T>
            unwrapResult((bool shouldCompleteWithSuccess, T objectToReturn, Exception exception) result) =>
                result.shouldCompleteWithSuccess
                    ? Observable.Return(result.objectToReturn)
                    : Observable.Throw<T>(result.exception);

            return Observable
                .Defer(delayedSource)
                .Select(wrapSuccessfulResult)
                .Catch<(bool shouldCompleteWithSuccess, T, Exception exception), Exception>(processException)
                .Retry(maxRetries + 1)
                .SelectMany(unwrapResult);
        }
    }
}