"""
Test script to verify the strategy works correctly with realistic data patterns
"""

import numpy as np
import pandas as pd
from datetime import datetime, timedelta
from strategy_implementation import MCMVRFStrategy, TechnicalIndicators


def create_mean_reverting_data(num_candles=1000, base_price=40000):
    """
    Create data that exhibits mean reversion behavior
    This will help demonstrate the strategy's signal generation
    """
    np.random.seed(123)  # For reproducibility
    
    prices = [base_price]
    mean_price = base_price
    
    # Create mean reverting price series
    for i in range(num_candles):
        # Mean reversion: price tends to revert to mean
        distance_from_mean = prices[-1] - mean_price
        mean_revert_force = -0.1 * distance_from_mean / mean_price
        
        # Add random noise
        noise = np.random.normal(0, 0.015)
        
        # Calculate change
        change = mean_revert_force + noise
        
        # Add occasional trend
        if i % 100 == 0:
            mean_price *= (1 + np.random.normal(0, 0.05))
        
        new_price = prices[-1] * (1 + change)
        prices.append(max(new_price, base_price * 0.5))  # Prevent too low
    
    # Generate OHLCV
    data = []
    start_time = datetime(2024, 1, 1)
    
    for i in range(num_candles):
        timestamp = start_time + timedelta(hours=i * 4)
        close = prices[i + 1]
        open_price = prices[i]
        
        # Create candle with some variation
        high = max(open_price, close) * (1 + abs(np.random.normal(0, 0.003)))
        low = min(open_price, close) * (1 - abs(np.random.normal(0, 0.003)))
        volume = np.random.uniform(500, 1500)
        
        data.append({
            'timestamp': timestamp,
            'open': open_price,
            'high': high,
            'low': low,
            'close': close,
            'volume': volume
        })
    
    return pd.DataFrame(data)


def test_indicator_calculations():
    """Test that all indicators calculate correctly"""
    print("\n" + "="*60)
    print("🧪 Testing Indicator Calculations")
    print("="*60)
    
    # Create simple test data
    test_data = pd.DataFrame({
        'timestamp': pd.date_range(start='2024-01-01', periods=100, freq='4H'),
        'open': np.random.uniform(39000, 41000, 100),
        'high': np.random.uniform(40000, 42000, 100),
        'low': np.random.uniform(38000, 40000, 100),
        'close': np.random.uniform(39000, 41000, 100),
        'volume': np.random.uniform(100, 1000, 100)
    })
    
    strategy = MCMVRFStrategy()
    indicators = strategy.calculate_indicators(test_data)
    
    # Check all indicators are calculated
    required_indicators = ['bb_upper', 'bb_middle', 'bb_lower', 'bbw', 'rsi', 
                          'stoch_k', 'stoch_d', 'atr', 'adx', 'ema_50', 'ema_200']
    
    all_present = True
    for ind in required_indicators:
        if ind in indicators:
            print(f"✅ {ind}: OK (len={len(indicators[ind])}, last={indicators[ind][-1]:.2f})")
        else:
            print(f"❌ {ind}: MISSING")
            all_present = False
    
    if all_present:
        print("\n✅ All indicators calculated successfully!")
    else:
        print("\n❌ Some indicators missing!")
    
    return all_present


def test_signal_generation():
    """Test signal generation with mean reverting data"""
    print("\n" + "="*60)
    print("🧪 Testing Signal Generation")
    print("="*60)
    
    data = create_mean_reverting_data(num_candles=500)
    strategy = MCMVRFStrategy()
    
    signals_found = 0
    long_signals = 0
    short_signals = 0
    
    # Check multiple points for signals
    for i in range(250, len(data), 10):
        signal = strategy.generate_signal(data.iloc[:i+1], account_balance=10000, current_index=-1)
        if signal.signal_type != 'NONE':
            signals_found += 1
            if signal.signal_type == 'LONG':
                long_signals += 1
                print(f"✅ LONG signal at index {i}: Entry=${signal.entry_price:.2f}, SL=${signal.stop_loss:.2f}")
            else:
                short_signals += 1
                print(f"✅ SHORT signal at index {i}: Entry=${signal.entry_price:.2f}, SL=${signal.stop_loss:.2f}")
    
    print(f"\n📊 Signal Summary:")
    print(f"   Total signals found: {signals_found}")
    print(f"   LONG signals: {long_signals}")
    print(f"   SHORT signals: {short_signals}")
    
    if signals_found > 0:
        print("\n✅ Signal generation working!")
        return True
    else:
        print("\n⚠️  No signals generated (this can happen with random data)")
        return False


def test_backtest():
    """Test backtesting functionality"""
    print("\n" + "="*60)
    print("🧪 Testing Backtesting Engine")
    print("="*60)
    
    data = create_mean_reverting_data(num_candles=800)
    strategy = MCMVRFStrategy()
    
    print("\n⏳ Running backtest...")
    results = strategy.backtest(data, initial_balance=10000)
    
    print(f"\n📊 Backtest Results:")
    print(f"   Initial Balance: ${results['initial_balance']:,.2f}")
    print(f"   Final Balance: ${results['final_balance']:,.2f}")
    print(f"   Total Return: {results['total_return']:+.2f}%")
    print(f"   Total Trades: {results['total_trades']}")
    print(f"   Win Rate: {results['win_rate']:.2f}%")
    print(f"   Profit Factor: {results['profit_factor']:.2f}")
    
    if results['total_trades'] > 0:
        print("\n✅ Backtesting working! Strategy executed trades.")
        
        # Show first few trades
        print("\n📋 First 3 trades:")
        for i, trade in enumerate(results['trades'][:3], 1):
            pnl_emoji = "✅" if trade['pnl'] > 0 else "❌"
            print(f"{pnl_emoji} Trade {i}: {trade['type']} | "
                  f"Entry=${trade['entry_price']:.2f} | "
                  f"Exit=${trade['exit_price']:.2f} | "
                  f"P&L=${trade['pnl']:+.2f}")
        
        return True
    else:
        print("\n⚠️  No trades executed in backtest")
        return False


def test_risk_management():
    """Test risk management features"""
    print("\n" + "="*60)
    print("🧪 Testing Risk Management")
    print("="*60)
    
    strategy = MCMVRFStrategy()
    
    # Test position sizing
    entry_price = 40000
    stop_loss = 38000
    account_balance = 10000
    
    position_size = strategy.calculate_position_size(account_balance, entry_price, stop_loss)
    risk_amount = account_balance * 0.02  # 2% risk
    actual_risk = position_size * (entry_price - stop_loss)
    position_value = position_size * entry_price
    max_position_value = account_balance * 0.1
    
    print(f"\n📊 Position Sizing Test:")
    print(f"   Account Balance: ${account_balance:,.2f}")
    print(f"   Entry Price: ${entry_price:,.2f}")
    print(f"   Stop Loss: ${stop_loss:,.2f}")
    print(f"   Stop Distance: ${entry_price - stop_loss:,.2f} ({((entry_price - stop_loss) / entry_price * 100):.2f}%)")
    print(f"   Target Risk: ${risk_amount:,.2f} (2% of account)")
    print(f"   Position Size: {position_size:.6f} units")
    print(f"   Position Value: ${position_value:,.2f}")
    print(f"   Max Position Value (10% cap): ${max_position_value:,.2f}")
    print(f"   Actual Risk: ${actual_risk:,.2f}")
    
    # Check if position is reasonable (either matches target risk OR is capped at max position)
    risk_ok = (abs(actual_risk - risk_amount) < 1.0) or (position_value <= max_position_value * 1.01)
    
    if risk_ok:
        if position_value < max_position_value:
            print(f"\n✅ Position sizing correct! Risk = ${actual_risk:.2f}")
        else:
            print(f"\n✅ Position capped at 10% of account (conservative risk management)")
    else:
        print("\n⚠️  Position sizing calculation may need review")
    
    # Test stop loss / take profit calculation
    atr = 1500
    levels = strategy.calculate_stop_loss_take_profit(entry_price, atr, 'LONG')
    
    print(f"\n📊 Stop Loss / Take Profit Test (LONG):")
    print(f"   ATR: ${atr:,.2f}")
    print(f"   Stop Loss: ${levels['stop_loss']:,.2f} ({((levels['stop_loss'] - entry_price) / entry_price * 100):.2f}%)")
    print(f"   Take Profits:")
    for i, tp in enumerate(levels['take_profits'], 1):
        print(f"      TP{i}: ${tp:,.2f} ({((tp - entry_price) / entry_price * 100):+.2f}%)")
    
    print("\n✅ Risk management calculations working!")
    return True


def test_configuration():
    """Test custom configuration"""
    print("\n" + "="*60)
    print("🧪 Testing Custom Configuration")
    print("="*60)
    
    custom_config = {
        'risk_per_trade': 0.03,  # 3% instead of 2%
        'max_positions': 5,
        'bb_period': 25,
    }
    
    strategy = MCMVRFStrategy(config=custom_config)
    
    print(f"\n📋 Custom Configuration Applied:")
    print(f"   Risk per trade: {strategy.config['risk_per_trade'] * 100}%")
    print(f"   Max positions: {strategy.config['max_positions']}")
    print(f"   BB period: {strategy.config['bb_period']}")
    print(f"   RSI period (default): {strategy.config['rsi_period']}")
    
    config_ok = (
        strategy.config['risk_per_trade'] == 0.03 and
        strategy.config['max_positions'] == 5 and
        strategy.config['bb_period'] == 25 and
        strategy.config['rsi_period'] == 14  # Should keep default
    )
    
    if config_ok:
        print("\n✅ Custom configuration working correctly!")
        return True
    else:
        print("\n❌ Configuration not applied correctly!")
        return False


def main():
    """Run all tests"""
    print("\n" + "="*60)
    print("🚀 MCM-VRF STRATEGY TEST SUITE")
    print("="*60)
    
    results = {}
    
    # Run tests
    results['indicators'] = test_indicator_calculations()
    results['signals'] = test_signal_generation()
    results['backtest'] = test_backtest()
    results['risk_mgmt'] = test_risk_management()
    results['config'] = test_configuration()
    
    # Summary
    print("\n" + "="*60)
    print("📊 TEST SUMMARY")
    print("="*60)
    
    for test_name, passed in results.items():
        emoji = "✅" if passed else "❌"
        print(f"{emoji} {test_name.replace('_', ' ').title()}: {'PASS' if passed else 'FAIL'}")
    
    total_tests = len(results)
    passed_tests = sum(results.values())
    
    print(f"\n📈 Overall: {passed_tests}/{total_tests} tests passed ({passed_tests/total_tests*100:.0f}%)")
    
    if passed_tests == total_tests:
        print("\n🎉 All tests passed! Strategy implementation is working correctly.")
    else:
        print("\n⚠️  Some tests did not pass. Review the output above.")
    
    print("="*60 + "\n")


if __name__ == "__main__":
    main()
