# Example Usage - Trading Strategy Research System

## Quick Start

### 1. Run the Meta-Prompt Research Loop

```bash
python3 meta_prompt_system.py
```

This will execute 3 iterations of the multi-agent research loop, producing:
- Strategy discoveries
- Critical analysis
- Technical improvements
- Risk assessments
- Synthesized recommendations

### 2. Expected Output

Each iteration will show:

```
ITERATION 1
================================================================================

AGENT 1: Araştırmacı (Research Agent)
--------------------------------------------------------------------------------
Top Strategies:
1. Volatility Expansion Breakout (Win Rate: 91%, RR: 4.0:1)
2. Multi-MA Bounce (Win Rate: 90%, RR: 2.3:1)
3. Momentum Filtered Trend (Win Rate: 89%, RR: 3.0:1)

AGENT 2: Eleştirmen (Critic Agent)
--------------------------------------------------------------------------------
Major Concerns:
⚠️ Win rates likely inflated by 10-15%
⚠️ Missing market regime testing
⚠️ Slippage costs not factored

AGENT 3: Geliştirici (Developer Agent)
--------------------------------------------------------------------------------
Enhancements:
✓ Market Regime Filter: +5-10% win rate
✓ Multi-Timeframe Confirmation: -30% false signals

AGENT 4: Risk Yöneticisi (Risk Manager Agent)
--------------------------------------------------------------------------------
Critical Risks:
🔴 Overfitting [HIGH] - Mitigation: Walk-forward optimization
🔴 Regime Change [HIGH] - Mitigation: Real-time detection

AGENT 5: Sentezci (Synthesis Agent)
--------------------------------------------------------------------------------
🏆 FINAL RECOMMENDATIONS:
1. Multi-MA Bounce (Enhanced) - Score: 86.5/100
2. Momentum Filtered Trend (Enhanced) - Score: 84.2/100
3. Volatility Expansion Breakout (Enhanced) - Score: 82.8/100
```

## Strategy Examples

### Example 1: Multi-MA Bounce Strategy

**Setup:**
```python
# Indicators
sma_20 = SMA(close, 20)
ema_50 = EMA(close, 50)
hma_100 = HMA(close, 100)
rsi = RSI(close, 14)
bb = BollingerBands(close, 20, 2)

# Entry Conditions
ma_cluster = abs(sma_20 - ema_50) / sma_20 < 0.02 and abs(ema_50 - hma_100) / ema_50 < 0.02
price_at_ma = min(abs(close - sma_20), abs(close - ema_50), abs(close - hma_100)) < ATR(14) * 0.5
rsi_extreme = rsi < 30 or rsi > 70
reversal_candle = is_hammer(candle) or is_shooting_star(candle)
at_bb = close <= bb.lower or close >= bb.upper

entry_long = ma_cluster and price_at_ma and rsi < 30 and reversal_candle and close <= bb.lower

# Exit Conditions
target = opposite_ma_in_cluster()  # Typically middle MA
stop = close - (ATR(14) * 2)
```

**Expected Performance:**
- Win Rate: 81% (adjusted from 90%)
- Risk/Reward: 2.3:1
- Signals: 3-4 per week per asset
- Best Timeframe: 1H, 4H

**Clever Tactics:**
1. Wait for all 3 MAs to converge (magnetic effect)
2. Use HMA for early exit detection (fastest moving)
3. Scale in: 50% at first MA touch, 50% if price extends
4. Only trade when MAs are converging, not diverging

### Example 2: Momentum Filtered Trend Strategy

**Setup:**
```python
# Indicators
ema_9 = EMA(close, 9)
ema_21 = EMA(close, 21)
ema_50 = EMA(close, 50)
roc_10 = ROC(close, 10)
hma_16 = HMA(close, 16)
adx = ADX(14)

# Entry Conditions (Long)
ema_cross = crossover(ema_9, ema_21)
both_above_50 = ema_9 > ema_50 and ema_21 > ema_50
strong_momentum = roc_10 > 3.0
hma_up = hma_16 > hma_16[1]
price_aligned = close > ema_9 > ema_21 > ema_50
regime_trending = adx > 25  # Added filter

entry_long = ema_cross and both_above_50 and strong_momentum and hma_up and regime_trending

# Exit Conditions
exit_signal = crossunder(ema_9, ema_21) or roc_10 < 0 or close < hma_16
```

**Expected Performance:**
- Win Rate: 80% (adjusted from 89%)
- Risk/Reward: 3.0:1
- Signals: 2-4 per week per asset
- Best Timeframe: 4H, Daily

**Clever Tactics:**
1. Scale entry: 50% on EMA cross, 50% on ROC confirmation
2. Use HMA for early exit (faster than EMA)
3. Only trade when ROC is accelerating (ROC > ROC[1])
4. Filter low volume periods (Volume > 1.5x SMA(20))

### Example 3: Volatility Expansion Breakout

**Setup:**
```python
# Indicators
bb = BollingerBands(close, 20, 2)
bb_width = (bb.upper - bb.lower) / bb.middle
rsi = RSI(close, 14)
macd = MACD(close, 12, 26, 9)
adx = ADX(14)

# Calculate squeeze
bb_width_percentile = percentile_rank(bb_width, 120)  # 6 months
is_squeeze = bb_width_percentile < 10  # <10th percentile

# Consolidation check
consolidation_bars = count_bars_between_bands(bb, close)
is_consolidating = consolidation_bars >= 15

# Neutral indicators
rsi_neutral = 45 <= rsi <= 55
macd_flat = abs(macd.histogram) < abs(macd.histogram[1])

# Entry (after breakout)
breakout_up = close > bb.upper and volume > volume_sma(20) * 2
confirm_candle = close[1] > bb.upper[1]  # 2nd candle confirmation

entry_long = is_squeeze and is_consolidating and breakout_up and confirm_candle
```

**Expected Performance:**
- Win Rate: 82% (adjusted from 91%)
- Risk/Reward: 4.0:1
- Signals: 1-2 per week per asset (rare but powerful)
- Best Timeframe: 4H, Daily

**Clever Tactics:**
1. Only trade <10th percentile squeezes (tightest)
2. Measure prior consolidation range, project as target
3. Wait for 2nd confirming candle (avoid false breakouts)
4. Multi-timeframe: Confirm squeeze on both 4H and Daily

## Configuration Customization

Edit `config.json` to customize:

```json
{
  "target_metrics": {
    "win_rate_target": 0.85,     // Your target win rate
    "min_risk_reward": 2.0        // Minimum acceptable RR
  },
  "risk_management": {
    "max_risk_per_trade": 0.02,   // 2% risk per trade
    "max_drawdown": 0.10          // 10% max drawdown
  }
}
```

## Multi-Agent Interaction Flow

```
Research Agent → Generates 8 strategy ideas
       ↓
Critic Agent → Challenges assumptions, identifies weaknesses
       ↓
Developer Agent → Proposes technical solutions & enhancements
       ↓
Risk Manager Agent → Analyzes risks, proposes mitigation
       ↓
Synthesis Agent → Integrates all inputs, ranks strategies
       ↓
Meta-Prompt Generator → Creates improved prompt for next iteration
       ↓
Self-Critique → Identifies gaps and improvement areas
       ↓
[LOOP CONTINUES]
```

## Implementation Checklist

### Phase 1: Setup (Week 1)
- [ ] Review all 8 discovered strategies
- [ ] Choose 2-3 strategies that fit your style
- [ ] Set up charting platform with indicators
- [ ] Configure alerts for entry conditions

### Phase 2: Paper Trading (Weeks 2-5)
- [ ] Trade strategies in paper account
- [ ] Track all trades in spreadsheet
- [ ] Measure actual win rate vs expected
- [ ] Calculate actual RR vs expected
- [ ] Monitor for edge degradation

### Phase 3: Live Trading (Week 6+)
- [ ] Start with 50% intended position size
- [ ] Trade only highest-confidence setups
- [ ] Scale up gradually over 8 weeks
- [ ] Continuously monitor performance
- [ ] Adjust based on results

## Performance Tracking

Track these metrics weekly:

```python
metrics = {
    "win_rate": wins / total_trades,
    "avg_rr": sum(profits) / sum(losses),
    "signal_frequency": trades / weeks,
    "max_drawdown": max(peak - trough),
    "sharpe_ratio": (avg_return - risk_free) / std_returns,
    "consecutive_losses": max_losing_streak
}
```

**Performance Thresholds:**

```
🟢 HEALTHY: Win Rate > 65%, Drawdown < 10%, RR > 2.0
🟡 CAUTION: Win Rate 55-65%, Drawdown 10-15%, RR 1.5-2.0  
🔴 HALT: Win Rate < 55%, Drawdown > 15%, RR < 1.5
```

## Clever Tactics Library

### 1. Multi-Timeframe Synergy
```python
# Get signal on higher TF, entry on lower TF
signal_tf = "4H"
entry_tf = "15min"

if strategy_signal(signal_tf):
    wait_for_entry_signal(entry_tf)
```

### 2. Time-of-Day Filter
```python
# Trade only during high-liquidity hours
TRADE_HOURS = [
    (9, 30, 11, 30),   # Morning session
    (14, 0, 16, 0)     # Afternoon session
]

if not in_trading_hours(current_time):
    skip_signal()
```

### 3. Volume Confirmation
```python
# Require volume spike for breakouts
avg_volume = SMA(volume, 20)

if signal_type == "breakout":
    if volume < avg_volume * 2.0:
        skip_signal()  # Weak breakout
```

### 4. Dynamic Position Sizing
```python
# Adjust size based on recent performance
recent_win_rate = calculate_win_rate(last_20_trades)
expected_win_rate = 0.75

size_multiplier = recent_win_rate / expected_win_rate
position_size = base_size * min(size_multiplier, 1.5)
```

### 5. Adaptive Stops
```python
# ATR-based stops
atr = ATR(14)
stop_distance = atr * 2.0

if entry_long:
    stop_loss = entry_price - stop_distance
else:
    stop_loss = entry_price + stop_distance
```

## Troubleshooting

### Win Rate Lower Than Expected
- Check if market regime matches strategy type
- Verify all entry conditions are being met
- Review if exits are too early
- Consider tightening entry criteria

### Too Few Signals
- Relax some confirmation requirements
- Trade on multiple timeframes
- Trade multiple assets
- Consider adding complementary strategies

### High Drawdown
- Reduce position size immediately
- Tighten risk per trade to 1%
- Review if market regime has changed
- Check if following all rules strictly

### Consecutive Losses
- Normal: 3-5 losses can happen with 75% win rate
- At 3 losses: Reduce size by 50%
- At 5 losses: Pause and review system
- Check if edge has degraded

## Next Steps

1. **Study the Documentation**: Read `TRADING_STRATEGY_GUIDE.md` completely
2. **Run the System**: Execute `python3 meta_prompt_system.py`
3. **Choose Strategies**: Select 2-3 strategies that resonate
4. **Paper Trade**: Minimum 4 weeks, 20+ trades
5. **Go Live Small**: Start with 50% size
6. **Scale Gradually**: Increase over 2 months
7. **Monitor & Adapt**: Weekly reviews, monthly optimization

## Warning

⚠️ **CRITICAL REMINDERS:**

1. These win rates are ESTIMATES - expect 10-20% lower in live trading
2. Psychology reduces performance by 20-30% for most traders
3. NEVER skip paper trading - minimum 4 weeks required
4. NEVER risk more than 2% per trade
5. NEVER trade without stop losses
6. Edge can and will degrade over time - monitor constantly
7. Past performance does NOT guarantee future results

**The goal is not perfection. The goal is consistent, disciplined execution of a robust, repeatable edge with proper risk management.**

Good luck! 🎯
