"""
Example Usage of MCM-VRF Trading Strategy

This script demonstrates how to use the strategy with sample data.
For live trading, you would replace the sample data generation with
actual market data from an exchange API (Binance, Coinbase, etc.)
"""

import numpy as np
import pandas as pd
from datetime import datetime, timedelta
from strategy_implementation import MCMVRFStrategy


def generate_sample_data(days=365, timeframe_hours=4):
    """
    Generate sample OHLCV data for demonstration
    
    In production, replace this with real data from:
    - Binance API
    - Coinbase API
    - CCXT library
    - Any other data provider
    """
    num_candles = days * (24 // timeframe_hours)
    
    # Generate realistic-looking price data
    np.random.seed(42)
    base_price = 40000  # Starting price (like BTC)
    prices = [base_price]
    
    for i in range(num_candles):
        # Random walk with drift
        change_pct = np.random.normal(0.001, 0.02)  # Mean 0.1%, Std 2%
        new_price = prices[-1] * (1 + change_pct)
        prices.append(new_price)
    
    # Generate OHLCV data
    data = []
    start_time = datetime.now() - timedelta(days=days)
    
    for i in range(num_candles):
        timestamp = start_time + timedelta(hours=i * timeframe_hours)
        close = prices[i + 1]
        open_price = prices[i]
        high = max(open_price, close) * (1 + abs(np.random.normal(0, 0.005)))
        low = min(open_price, close) * (1 - abs(np.random.normal(0, 0.005)))
        volume = np.random.uniform(100, 1000)
        
        data.append({
            'timestamp': timestamp,
            'open': open_price,
            'high': high,
            'low': low,
            'close': close,
            'volume': volume
        })
    
    return pd.DataFrame(data)


def print_signal_details(signal):
    """Pretty print signal information"""
    print("\n" + "=" * 60)
    if signal.signal_type == 'NONE':
        print("❌ NO TRADING SIGNAL")
        print(f"Confidence Score: {signal.confidence_score}/3 (need 3+ to trade)")
        print("\nReasons:")
        for reason, value in signal.reasons.items():
            emoji = "✅" if value else "❌"
            print(f"  {emoji} {reason}: {value}")
    else:
        print(f"🎯 {signal.signal_type} SIGNAL DETECTED!")
        print("=" * 60)
        print(f"\n📅 Timestamp: {signal.timestamp}")
        print(f"💰 Entry Price: ${signal.entry_price:,.2f}")
        print(f"🛑 Stop Loss: ${signal.stop_loss:,.2f} ({((signal.stop_loss - signal.entry_price) / signal.entry_price * 100):.2f}%)")
        print(f"\n🎯 Take Profit Targets:")
        for i, tp in enumerate(signal.take_profits, 1):
            pct = ((tp - signal.entry_price) / signal.entry_price * 100)
            print(f"   TP{i}: ${tp:,.2f} ({pct:+.2f}%)")
        print(f"\n📊 Position Size: {signal.position_size:.6f} units")
        print(f"⭐ Confidence Score: {signal.confidence_score}/3")
        print(f"📈 ATR: ${signal.atr:,.2f}")
        
        print("\n✅ Signal Conditions Met:")
        for reason, value in signal.reasons.items():
            if value:
                print(f"   ✓ {reason}")
    print("=" * 60 + "\n")


def print_backtest_results(results):
    """Pretty print backtest results"""
    print("\n" + "=" * 60)
    print("📊 BACKTEST RESULTS")
    print("=" * 60)
    print(f"\n💵 Initial Balance: ${results['initial_balance']:,.2f}")
    print(f"💰 Final Balance: ${results['final_balance']:,.2f}")
    print(f"📈 Total Return: {results['total_return']:+.2f}%")
    print(f"\n📉 Trading Statistics:")
    print(f"   Total Trades: {results['total_trades']}")
    print(f"   Winning Trades: {results['winning_trades']} ({results['win_rate']:.2f}%)")
    print(f"   Losing Trades: {results['losing_trades']}")
    print(f"\n💹 Performance Metrics:")
    print(f"   Average Win: ${results['avg_win']:,.2f}")
    print(f"   Average Loss: ${results['avg_loss']:,.2f}")
    print(f"   Profit Factor: {results['profit_factor']:.2f}")
    print(f"   Sharpe Ratio: {results['sharpe_ratio']:.2f}")
    print(f"   Max Drawdown: {results['max_drawdown']:.2f}%")
    print("=" * 60 + "\n")
    
    # Show some sample trades
    if results['trades']:
        print("📋 Sample Trades (First 5):")
        print("-" * 60)
        for i, trade in enumerate(results['trades'][:5], 1):
            pnl_emoji = "✅" if trade['pnl'] > 0 else "❌"
            print(f"{pnl_emoji} Trade #{i}:")
            print(f"   Type: {trade['type']}")
            print(f"   Entry: ${trade['entry_price']:,.2f}")
            print(f"   Exit: ${trade['exit_price']:,.2f} ({trade['exit_reason']})")
            print(f"   P&L: ${trade['pnl']:+,.2f} ({trade['return_pct']:+.2f}%)")
            print()


def main():
    """Main example script"""
    print("\n🚀 MCM-VRF Trading Strategy - Example Usage")
    print("=" * 60)
    
    # Generate sample data (replace with real data in production)
    print("\n📊 Generating sample data...")
    data = generate_sample_data(days=365, timeframe_hours=4)
    print(f"Generated {len(data)} candles of 4-hour data")
    print(f"Date range: {data['timestamp'].iloc[0]} to {data['timestamp'].iloc[-1]}")
    
    # Initialize strategy
    print("\n🔧 Initializing strategy...")
    strategy = MCMVRFStrategy()
    print("Strategy initialized with default configuration")
    
    # Generate signal for latest data
    print("\n🎯 Generating trading signal for current market...")
    account_balance = 10000  # $10,000 account
    signal = strategy.generate_signal(data, account_balance=account_balance)
    print_signal_details(signal)
    
    # Run backtest
    print("\n⏳ Running backtest on historical data...")
    print("This may take a moment...\n")
    results = strategy.backtest(data, initial_balance=account_balance)
    print_backtest_results(results)
    
    # Custom configuration example
    print("\n🔧 Example: Custom Configuration")
    print("-" * 60)
    custom_config = {
        'risk_per_trade': 0.015,  # 1.5% risk per trade instead of 2%
        'max_positions': 2,        # Max 2 positions instead of 3
        'adx_threshold': 25,       # More aggressive trend filter
    }
    
    custom_strategy = MCMVRFStrategy(config=custom_config)
    print("Created strategy with custom configuration:")
    print(f"  - Risk per trade: 1.5%")
    print(f"  - Max positions: 2")
    print(f"  - ADX threshold: 25")
    
    # Generate signal with custom strategy
    custom_signal = custom_strategy.generate_signal(data, account_balance=account_balance)
    print("\n🎯 Signal with custom configuration:")
    print(f"Signal Type: {custom_signal.signal_type}")
    print(f"Confidence: {custom_signal.confidence_score}/3")
    
    print("\n" + "=" * 60)
    print("✅ Example completed!")
    print("\nNext steps:")
    print("1. Replace sample data with real market data")
    print("2. Connect to exchange API (Binance, Coinbase, etc.)")
    print("3. Implement paper trading to test live")
    print("4. Add monitoring and alerting")
    print("5. Consider risk management automation")
    print("\n⚠️  Remember: Always paper trade before using real money!")
    print("=" * 60 + "\n")


if __name__ == "__main__":
    main()
