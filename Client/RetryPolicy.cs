using Microsoft.AspNetCore.SignalR.Client;
using System;

namespace Treachery.Client
{
    public class RetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            if (retryContext.PreviousRetryCount == 0)
            {
                return TimeSpan.FromSeconds(0);
            }
            else if (retryContext.PreviousRetryCount < 5)
            {
                return TimeSpan.FromSeconds(1);
                //until 30 seconds after disconnect
            }
            else if (retryContext.PreviousRetryCount < 30)
            {
                return TimeSpan.FromSeconds(2);
                //until 30 seconds after disconnect
            }
            else if (retryContext.PreviousRetryCount < 100)
            {
                return TimeSpan.FromSeconds(10);
                //until 12 minutes after disconnect
            }
            else if (retryContext.PreviousRetryCount < 200)
            {
                return TimeSpan.FromSeconds(30);
                //until about 1 our after disconnect
            }
            else
            {
                return null;
            }
        }
    }
}
