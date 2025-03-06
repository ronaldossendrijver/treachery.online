using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace Treachery.Client;

public class RetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return retryContext.PreviousRetryCount switch
        {
            0 => TimeSpan.FromSeconds(0),
            < 5 => TimeSpan.FromSeconds(1),
            //until 30 seconds after disconnect
            < 30 => TimeSpan.FromSeconds(2),
            //until 30 seconds after disconnect
            < 100 => TimeSpan.FromSeconds(10),
            //until 12 minutes after disconnect
            < 200 => TimeSpan.FromSeconds(30),
            _ => null
        };
        //until about 1 our after disconnect
    }
}