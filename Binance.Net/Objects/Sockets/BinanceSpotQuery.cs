﻿using Binance.Net.Objects.Internal;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Binance.Net.Objects.Sockets
{
    internal class BinanceSpotQuery<T> : Query<T> where T: BinanceResponse
    {
        public BinanceSpotQuery(BinanceSocketQuery request, bool authenticated, int weight = 1) : base(request, authenticated, weight)
        {
        }

        public override CallResult<T> HandleResponse(ParsedMessage<T> message) => new CallResult<T>(message.Data);
        public override bool MessageMatchesQuery(ParsedMessage<T> message) => ((BinanceSocketQuery)Request).Id == message.Data.Id;
    }
}