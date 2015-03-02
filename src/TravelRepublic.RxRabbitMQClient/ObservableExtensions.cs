using System;
using System.Reactive.Linq;

namespace TravelRepublic.RxRabbitMQClient
{
    public static class ObservableExtensions
    {
        public static IObservable<T> Pace<T>(this IObservable<T> source, TimeSpan interval)
        {
            return source.Select(i => Observable.Empty<T>()
                .Delay(interval)
                .StartWith(i)).Concat();
        }
    }
}