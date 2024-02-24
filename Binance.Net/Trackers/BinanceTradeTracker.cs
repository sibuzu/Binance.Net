using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Trackers.Trades;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Binance.Net.Trackers
{
    /// <summary>
    /// Binance trade tracker
    /// </summary>
    public class BinanceTradeTracker : TradeTracker
    {
        private readonly IBinanceSocketClient _socketClient;
        private readonly CancellationTokenSource _cts;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="limit"></param>
        /// <param name="period"></param>
        public BinanceTradeTracker(string symbol, int? limit = null, TimeSpan? period = null)
            : this(symbol, limit, period, null, null)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="limit"></param>
        /// <param name="period"></param>
        /// <param name="socketClient"></param>
        /// <param name="logger"></param>
        public BinanceTradeTracker(
            string symbol,
            int? limit,
            TimeSpan? period,
            IBinanceSocketClient? socketClient,
            ILogger<BinanceTradeTracker>? logger) : base(logger, symbol, limit, period)
        {
            _socketClient = socketClient ?? new BinanceSocketClient();
            _cts = new CancellationTokenSource();
        }

        /// <inheritdoc />
        protected override async Task<CallResult<UpdateSubscription>> DoStartAsync()
        {
            var subResult = await _socketClient.SpotApi.ExchangeData.SubscribeToTradeUpdatesAsync(_symbol, HandleTradeUpdate, _cts.Token).ConfigureAwait(false);
            if (!subResult)
                return subResult.As<UpdateSubscription>(null);

            var snapshotResult = await _socketClient.SpotApi.ExchangeData.GetRecentTradesAsync(_symbol).ConfigureAwait(false);
            if (!snapshotResult)
            {
                _cts.Cancel();
                return snapshotResult.As<UpdateSubscription>(null);
            }

            SetInitialData(snapshotResult.Data.Result);
            return new CallResult<UpdateSubscription>(subResult.Data);
        }

        private void HandleTradeUpdate(DataEvent<BinanceStreamTrade> @event)
        {
            AddData(@event.Data);
        }
    }
}
