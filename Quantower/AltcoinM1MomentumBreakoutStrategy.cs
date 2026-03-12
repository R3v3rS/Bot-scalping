using System;
using TradingPlatform.BusinessLayer;

namespace QuantowerStrategies
{
    /// <summary>
    /// Altcoin M1 strategy:
    /// - Trend filter: EMA(50) vs EMA(200)
    /// - Breakout: break highest/lowest from N bars
    /// - Volatility filter: ATR(14) > SMA(ATR,100)
    /// - Volume filter: current volume > volume multiplier * SMA(volume,20)
    /// - Exit: SL/TP in ATR multiples + optional time stop
    ///
    /// Add on 1-minute chart and connect account/symbol in strategy panel.
    /// </summary>
    public sealed class AltcoinM1MomentumBreakoutStrategy : Strategy
    {
        [InputParameter("Quantity", 10, 0.001, 100000, 0.001, 3)]
        public double Quantity = 10;

        [InputParameter("Fast EMA", 20, 2, 300, 1, 0)]
        public int FastEmaPeriod = 50;

        [InputParameter("Slow EMA", 30, 2, 500, 1, 0)]
        public int SlowEmaPeriod = 200;

        [InputParameter("Breakout Lookback", 40, 2, 200, 1, 0)]
        public int BreakoutLookback = 20;

        [InputParameter("ATR Period", 50, 2, 100, 1, 0)]
        public int AtrPeriod = 14;

        [InputParameter("ATR SMA Period", 60, 5, 500, 1, 0)]
        public int AtrSmaPeriod = 100;

        [InputParameter("Volume SMA Period", 70, 2, 200, 1, 0)]
        public int VolumeSmaPeriod = 20;

        [InputParameter("Volume Multiplier", 80, 0.5, 10, 0.1, 2)]
        public double VolumeMultiplier = 1.5;

        [InputParameter("SL ATR Multiplier", 90, 0.2, 10, 0.1, 2)]
        public double SlAtrMultiplier = 1.5;

        [InputParameter("TP ATR Multiplier", 100, 0.2, 20, 0.1, 2)]
        public double TpAtrMultiplier = 2.5;

        [InputParameter("Enable Time Stop", 110)]
        public bool EnableTimeStop = true;

        [InputParameter("Time Stop (bars)", 120, 1, 300, 1, 0)]
        public int TimeStopBars = 20;

        [InputParameter("Allow Short", 130)]
        public bool AllowShort = true;

        [InputParameter("Allow Long", 140)]
        public bool AllowLong = true;

        private Indicator emaFast;
        private Indicator emaSlow;
        private Indicator atr;

        private int barsInPosition;

        public AltcoinM1MomentumBreakoutStrategy()
            : base()
        {
            this.Name = "Altcoin M1 Momentum Breakout";
            this.Description = "EMA trend + breakout + ATR/volume filters for 1-minute altcoin trading.";
        }

        protected override void OnRun()
        {
            if (this.Symbol == null)
                throw new InvalidOperationException("Symbol is not selected.");

            this.emaFast = Core.Instance.Indicators.BuiltIn.EMA(this.FastEmaPeriod, PriceType.Close);
            this.emaSlow = Core.Instance.Indicators.BuiltIn.EMA(this.SlowEmaPeriod, PriceType.Close);
            this.atr = Core.Instance.Indicators.BuiltIn.ATR(this.AtrPeriod, MaMode.SMA);

            this.AddIndicator(this.emaFast);
            this.AddIndicator(this.emaSlow);
            this.AddIndicator(this.atr);

            this.barsInPosition = 0;
            this.Log("Strategy started.", StrategyLoggingLevel.Info);
        }

        protected override void OnStop()
        {
            this.Log("Strategy stopped.", StrategyLoggingLevel.Info);
        }

        protected override void OnUpdate(UpdateArgs args)
        {
            if (args.Reason != UpdateReason.NewBar)
                return;

            int minBars = Math.Max(
                Math.Max(this.SlowEmaPeriod, this.BreakoutLookback + 2),
                Math.Max(this.AtrSmaPeriod + this.AtrPeriod + 2, this.VolumeSmaPeriod + 2));

            if (this.HistoricalData.Count < minBars)
                return;

            if (this.CurrentPosition != null && this.CurrentPosition.Quantity != 0)
            {
                this.barsInPosition++;
                HandleTimeStop();
                return;
            }

            this.barsInPosition = 0;

            double close = this.GetPrice(PriceType.Close, 1);
            double currentAtr = this.GetAtr(1);
            double atrSma = this.GetAtrSma(this.AtrSmaPeriod, 1);

            bool volatilityOk = currentAtr > atrSma;
            bool volumeOk = this.GetVolume(1) > this.GetVolumeSma(this.VolumeSmaPeriod, 1) * this.VolumeMultiplier;

            if (!volatilityOk || !volumeOk)
                return;

            bool upTrend = this.GetIndicatorValue(this.emaFast, 1) > this.GetIndicatorValue(this.emaSlow, 1);
            bool downTrend = this.GetIndicatorValue(this.emaFast, 1) < this.GetIndicatorValue(this.emaSlow, 1);

            double highest = this.GetHighestHigh(this.BreakoutLookback, 2);
            double lowest = this.GetLowestLow(this.BreakoutLookback, 2);

            bool longSignal = this.AllowLong && upTrend && close > highest;
            bool shortSignal = this.AllowShort && downTrend && close < lowest;

            if (longSignal)
                OpenPosition(Side.Buy, currentAtr);
            else if (shortSignal)
                OpenPosition(Side.Sell, currentAtr);
        }

        private void OpenPosition(Side side, double currentAtr)
        {
            if (this.Account == null)
            {
                this.Log("Account not selected.", StrategyLoggingLevel.Error);
                return;
            }

            double slOffset = currentAtr * this.SlAtrMultiplier;
            double tpOffset = currentAtr * this.TpAtrMultiplier;

            var request = new PlaceOrderRequestParameters
            {
                Account = this.Account,
                Symbol = this.Symbol,
                Side = side,
                OrderTypeId = OrderType.Market,
                Quantity = this.Quantity,
                StopLoss = SlTpHolder.CreateSL(slOffset, PriceMeasurement.Offset),
                TakeProfit = SlTpHolder.CreateTP(tpOffset, PriceMeasurement.Offset)
            };

            var result = Core.Instance.PlaceOrder(request);
            if (result != null && result.Status == TradingOperationResultStatus.Failure)
                this.Log($"Order failed: {result.Message}", StrategyLoggingLevel.Error);
            else
                this.Log($"Opened {side} Qty={this.Quantity} | ATR={currentAtr:F6} | SL={slOffset:F6} | TP={tpOffset:F6}", StrategyLoggingLevel.Trading);
        }

        private void HandleTimeStop()
        {
            if (!this.EnableTimeStop)
                return;

            if (this.barsInPosition < this.TimeStopBars)
                return;

            var closeResult = this.CurrentPosition.Close();
            if (closeResult != null && closeResult.Status == TradingOperationResultStatus.Failure)
                this.Log($"Time stop close failed: {closeResult.Message}", StrategyLoggingLevel.Error);
            else
                this.Log($"Position closed by time stop after {this.barsInPosition} bars.", StrategyLoggingLevel.Trading);
        }

        private double GetIndicatorValue(Indicator indicator, int offset)
            => indicator.GetValue(0, offset);

        private double GetAtr(int offset)
            => this.atr.GetValue(0, offset);

        private double GetPrice(PriceType priceType, int offset)
            => this.HistoricalData[offset, SeekOriginHistory.End][priceType];

        private double GetVolume(int offset)
            => this.HistoricalData[offset, SeekOriginHistory.End].Volume;

        private double GetHighestHigh(int period, int startOffset)
        {
            double highest = double.MinValue;
            for (int i = startOffset; i < startOffset + period; i++)
            {
                double high = this.GetPrice(PriceType.High, i);
                if (high > highest)
                    highest = high;
            }
            return highest;
        }

        private double GetLowestLow(int period, int startOffset)
        {
            double lowest = double.MaxValue;
            for (int i = startOffset; i < startOffset + period; i++)
            {
                double low = this.GetPrice(PriceType.Low, i);
                if (low < lowest)
                    lowest = low;
            }
            return lowest;
        }

        private double GetAtrSma(int period, int startOffset)
        {
            double sum = 0;
            for (int i = startOffset; i < startOffset + period; i++)
                sum += this.GetAtr(i);
            return sum / period;
        }

        private double GetVolumeSma(int period, int startOffset)
        {
            double sum = 0;
            for (int i = startOffset; i < startOffset + period; i++)
                sum += this.GetVolume(i);
            return sum / period;
        }
    }
}
