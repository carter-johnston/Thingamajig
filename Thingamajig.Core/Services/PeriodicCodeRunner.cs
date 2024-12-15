namespace Thingamajig.Core.Services
{
    public class PeriodicCodeRunner
    {
        public static readonly TimeSpan _defaultInterval = TimeSpan.FromSeconds(2);

        public async Task RunPeriodicallyAsync(
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken, 
            TimeSpan? interval = null)
        {
            var timer = new PeriodicTimer(interval ?? _defaultInterval);
            do
            {
                await action(cancellationToken);
            }
            while (await timer.WaitForNextTickAsync(cancellationToken));
        }

        public async Task RunPeriodicallyAsync(
            Action<CancellationToken> action,
            CancellationToken cancellationToken, 
            TimeSpan? interval = null)
        {
            var timer = new PeriodicTimer(interval ?? _defaultInterval);
            do
            {
                action(cancellationToken);
            }
            while (await timer.WaitForNextTickAsync(cancellationToken));
        }
    }
}
