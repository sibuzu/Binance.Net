using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Trackers.Klines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Binance.Net.Trackers
{
    public class BinanceKlineTracker : KlineTracker
    {
        private readonly IBinanceSocketClient _socketClient;
        private readonly string _symbol;
        private readonly KlineInterval _interval;
        private readonly CancellationTokenSource _cts;

        public BinanceKlineTracker(string symbol, KlineInterval interval, int? limit = null, TimeSpan? period = null, IBinanceSocketClient client = null) : base(limit, period)
        {
            _socketClient = client ?? new BinanceSocketClient();
            _symbol = symbol;
            _interval = interval;
            _cts = new CancellationTokenSource();
        }

        protected override async Task<CallResult> DoStartAsync()
        {
            var subResult = await _socketClient.SpotApi.ExchangeData.SubscribeToKlineUpdatesAsync(_symbol, _interval, HandleKlineUpdate, _cts.Token).ConfigureAwait(false);
            if (!subResult)
                return subResult.AsDataless();

            var snapshotResult = await _socketClient.SpotApi.ExchangeData.GetKlinesAsync(_symbol, _interval).ConfigureAwait(false);
            if (!snapshotResult)
            {
                _cts.Cancel();
                return snapshotResult.AsDataless();
            }

            SetInitialData(snapshotResult.Data.Result);
            return new CallResult(null);
        }

        protected override async Task DoStopAsync()
        {
            _cts.Cancel();
        }

        private void HandleKlineUpdate(DataEvent<IBinanceStreamKlineData> @event)
        {
            AddData((BinanceStreamKline)@event.Data.Data);
        }
    }
}
