using TickerQ.Utilities.Base;
using TickerQ.Utilities.Models;

namespace TickerQSpike;

public class ReservationUpdater(ILogger<ReservationUpdater> logger, AppDbContext dbContext)
{
    [TickerFunction("SomeFuncName")]
    public async Task Update(TickerFunctionContext<string> tickerContext,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("{description}", tickerContext.Request);
    }
}