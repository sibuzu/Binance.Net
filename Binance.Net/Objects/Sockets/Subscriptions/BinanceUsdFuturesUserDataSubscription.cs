﻿using Binance.Net.Objects.Internal;
using Binance.Net.Objects.Models;
using Binance.Net.Objects.Models.Futures.Socket;
using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binance.Net.Objects.Sockets
{
    /// <inheritdoc />
    public class BinanceUsdFuturesUserDataSubscription : Subscription<BinanceSocketQueryResponse, BinanceCombinedStream<BinanceStreamEvent>>
    {
        /// <inheritdoc />
        public override List<string> Identifiers => _identifiers;

        private readonly Action<DataEvent<BinanceFuturesStreamOrderUpdate>>? _orderHandler;
        private readonly Action<DataEvent<BinanceFuturesStreamConfigUpdate>>? _configHandler;
        private readonly Action<DataEvent<BinanceFuturesStreamMarginUpdate>>? _marginHandler;
        private readonly Action<DataEvent<BinanceFuturesStreamAccountUpdate>>? _accountHandler;
        private readonly Action<DataEvent<BinanceStreamEvent>>? _listenkeyHandler;
        private readonly Action<DataEvent<BinanceStrategyUpdate>>? _strategyHandler;
        private readonly Action<DataEvent<BinanceGridUpdate>>? _gridHandler;
        private readonly Action<DataEvent<BinanceConditionOrderTriggerRejectUpdate>>? _condOrderHandler;
        private readonly List<string> _identifiers;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="topics"></param>
        /// <param name="orderHandler"></param>
        /// <param name="configHandler"></param>
        /// <param name="marginHandler"></param>
        /// <param name="accountHandler"></param>
        /// <param name="listenkeyHandler"></param>
        /// <param name="strategyHandler"></param>
        /// <param name="gridHandler"></param>
        /// <param name="condOrderHandler"></param>
        public BinanceUsdFuturesUserDataSubscription(
            ILogger logger,
            List<string> topics,
            Action<DataEvent<BinanceFuturesStreamOrderUpdate>>? orderHandler,
            Action<DataEvent<BinanceFuturesStreamConfigUpdate>>? configHandler,
            Action<DataEvent<BinanceFuturesStreamMarginUpdate>>? marginHandler,
            Action<DataEvent<BinanceFuturesStreamAccountUpdate>>? accountHandler,
            Action<DataEvent<BinanceStreamEvent>>? listenkeyHandler,
            Action<DataEvent<BinanceStrategyUpdate>>? strategyHandler,
            Action<DataEvent<BinanceGridUpdate>>? gridHandler,
            Action<DataEvent<BinanceConditionOrderTriggerRejectUpdate>>? condOrderHandler) : base(logger, false)
        {
            _orderHandler = orderHandler;
            _configHandler = configHandler;
            _marginHandler = marginHandler;
            _accountHandler = accountHandler;
            _listenkeyHandler = listenkeyHandler;
            _strategyHandler = strategyHandler;
            _gridHandler = gridHandler;
            _condOrderHandler = condOrderHandler;
            _identifiers = topics;
        }

        /// <inheritdoc />
        public override BaseQuery? GetSubQuery()
        {
            return new BinanceSystemQuery<BinanceSocketQueryResponse>(new BinanceSocketRequest
            {
                Method = "SUBSCRIBE",
                Params = _identifiers.ToArray(),
                Id = ExchangeHelpers.NextId()
            }, false);
        }

        /// <inheritdoc />
        public override BaseQuery? GetUnsubQuery()
        {
            return new BinanceSystemQuery<BinanceSocketQueryResponse>(new BinanceSocketRequest
            {
                Method = "UNSUBSCRIBE",
                Params = _identifiers.ToArray(),
                Id = ExchangeHelpers.NextId()
            }, false);
        }

        /// <inheritdoc />
        public override Task HandleEventAsync(DataEvent<ParsedMessage<BinanceCombinedStream<BinanceStreamEvent>>> message)
        {
            var data = message.Data.Data.Data;
            if (data is BinanceFuturesStreamConfigUpdate configUpdate)
            {
                configUpdate.ListenKey = message.Data.Data.Stream;
                _configHandler?.Invoke(message.As(configUpdate, message.Data.Data.Stream, SocketUpdateType.Update));
            }
            else if (data is BinanceFuturesStreamMarginUpdate marginUpdate)
            {
                marginUpdate.ListenKey = message.Data.Data.Stream;
                _marginHandler?.Invoke(message.As(marginUpdate, message.Data.Data.Stream, SocketUpdateType.Update));
            }
            else if (data is BinanceFuturesStreamAccountUpdate accountUpdate)
            {
                accountUpdate.ListenKey = message.Data.Data.Stream;
                _accountHandler?.Invoke(message.As(accountUpdate, message.Data.Data.Stream, SocketUpdateType.Update));
            }
            else if (data is BinanceFuturesStreamOrderUpdate orderUpate)
            {
                orderUpate.ListenKey = message.Data.Data.Stream;
                _orderHandler?.Invoke(message.As(orderUpate, message.Data.Data.Stream, SocketUpdateType.Update));
            }
            else if (data is BinanceStreamEvent listenKeyUpdate)
            {
                _listenkeyHandler?.Invoke(message.As(listenKeyUpdate, message.Data.Data.Stream, SocketUpdateType.Update));
            }
            else if (data is BinanceStrategyUpdate strategyUpdate)
            {
                _strategyHandler?.Invoke(message.As(strategyUpdate, message.Data.Data.Stream, SocketUpdateType.Update));
            }
            else if (data is BinanceGridUpdate gridUpdate)
            {
                _gridHandler?.Invoke(message.As(gridUpdate, message.Data.Data.Stream, SocketUpdateType.Update));
            }
            else if (data is BinanceConditionOrderTriggerRejectUpdate condUpdate)
            {
                _condOrderHandler?.Invoke(message.As(condUpdate, message.Data.Data.Stream, SocketUpdateType.Update));
            }
            return Task.CompletedTask;
        }
    }
}