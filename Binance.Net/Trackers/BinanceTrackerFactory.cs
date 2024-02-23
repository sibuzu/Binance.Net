using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Options;
using Binance.Net.SymbolOrderBooks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Trackers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Binance.Net.Trackers
{
    public class BinanceTrackerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving logging and clients</param>
        public BinanceTrackerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public ISymbolOrderBook CreateSpotOrderBook(string symbol, Action<BinanceOrderBookOptions>? options = null)
            => new BinanceSpotSymbolOrderBook(symbol,
                                             options,
                                             _serviceProvider.GetRequiredService<ILogger<BinanceSpotSymbolOrderBook>>(),
                                             _serviceProvider.GetRequiredService<IBinanceRestClient>(),
                                             _serviceProvider.GetRequiredService<IBinanceSocketClient>());

        /// <inheritdoc />
        public KlineTracker CreateSpotKlineTracker(string symbol, KlineInterval interval, int? limit = null, TimeSpan? period = null)
            => new BinanceKlineTracker(symbol, interval, limit, period, _serviceProvider.GetRequiredService<IBinanceSocketClient>());

        /// <inheritdoc />
        public TradeTracker CreateSpotTradeTracker(string symbol, int? limit = null, TimeSpan? period = null)
            => new BinanceTradeTracker(symbol, limit, period, _serviceProvider.GetRequiredService<IBinanceSocketClient>(), _serviceProvider.GetRequiredService<ILogger<BinanceTradeTracker>>());
    }
}
