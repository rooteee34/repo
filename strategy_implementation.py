"""
Multi-Confluence Mean Reversion with Volatility Regime Filter (MCM-VRF)
Advanced Trading Strategy Implementation

This module implements the MCM-VRF trading strategy with complete
signal generation, risk management, and position management logic.

Author: Advanced Quantitative Trading Agent
Date: 2025-12-03
Version: 1.0
"""

import numpy as np
import pandas as pd
from typing import Dict, List, Tuple, Optional
from dataclasses import dataclass
from datetime import datetime, timedelta


@dataclass
class TradingSignal:
    """Trading signal with all relevant information"""
    timestamp: datetime
    signal_type: str  # 'LONG', 'SHORT', 'NONE'
    entry_price: float
    stop_loss: float
    take_profits: List[float]
    position_size: float
    confidence_score: int
    atr: float
    reasons: Dict[str, bool]


@dataclass
class Position:
    """Active position information"""
    entry_time: datetime
    entry_price: float
    position_type: str  # 'LONG' or 'SHORT'
    position_size: float
    stop_loss: float
    take_profits: List[float]
    remaining_size: float
    atr_at_entry: float
    highest_profit: float = 0.0
    
    
class TechnicalIndicators:
    """Technical indicator calculations"""
    
    @staticmethod
    def bollinger_bands(close: np.ndarray, period: int = 20, std_dev: float = 2.0) -> Tuple[np.ndarray, np.ndarray, np.ndarray]:
        """Calculate Bollinger Bands"""
        middle = pd.Series(close).rolling(window=period).mean().values
        std = pd.Series(close).rolling(window=period).std().values
        upper = middle + (std_dev * std)
        lower = middle - (std_dev * std)
        return upper, middle, lower
    
    @staticmethod
    def rsi(close: np.ndarray, period: int = 14) -> np.ndarray:
        """Calculate Relative Strength Index"""
        deltas = np.diff(close)
        seed = deltas[:period + 1]
        up = seed[seed >= 0].sum() / period
        down = -seed[seed < 0].sum() / period
        rs = up / down if down != 0 else 0
        rsi_values = np.zeros_like(close)
        rsi_values[:period] = 100. - 100. / (1. + rs)
        
        for i in range(period, len(close)):
            delta = deltas[i - 1]
            if delta > 0:
                upval = delta
                downval = 0.
            else:
                upval = 0.
                downval = -delta
            
            up = (up * (period - 1) + upval) / period
            down = (down * (period - 1) + downval) / period
            rs = up / down if down != 0 else 0
            rsi_values[i] = 100. - 100. / (1. + rs)
        
        return rsi_values
    
    @staticmethod
    def stochastic(high: np.ndarray, low: np.ndarray, close: np.ndarray, 
                   k_period: int = 14, d_period: int = 3, smooth: int = 3) -> Tuple[np.ndarray, np.ndarray]:
        """Calculate Stochastic Oscillator"""
        lowest_low = pd.Series(low).rolling(window=k_period).min().values
        highest_high = pd.Series(high).rolling(window=k_period).max().values
        
        # Prevent division by zero
        range_hl = highest_high - lowest_low
        range_hl = np.where(range_hl == 0, 1e-10, range_hl)
        
        k_values = 100 * (close - lowest_low) / range_hl
        k_values = pd.Series(k_values).rolling(window=smooth).mean().values
        d_values = pd.Series(k_values).rolling(window=d_period).mean().values
        
        return k_values, d_values
    
    @staticmethod
    def atr(high: np.ndarray, low: np.ndarray, close: np.ndarray, period: int = 14) -> np.ndarray:
        """Calculate Average True Range"""
        tr1 = high - low
        tr2 = np.abs(high - np.roll(close, 1))
        tr3 = np.abs(low - np.roll(close, 1))
        tr = np.maximum(tr1, np.maximum(tr2, tr3))
        atr_values = pd.Series(tr).rolling(window=period).mean().values
        return atr_values
    
    @staticmethod
    def adx(high: np.ndarray, low: np.ndarray, close: np.ndarray, period: int = 14) -> np.ndarray:
        """Calculate Average Directional Index"""
        # Calculate +DM and -DM
        up_move = high - np.roll(high, 1)
        down_move = np.roll(low, 1) - low
        
        plus_dm = np.where((up_move > down_move) & (up_move > 0), up_move, 0)
        minus_dm = np.where((down_move > up_move) & (down_move > 0), down_move, 0)
        
        # Calculate ATR
        atr_values = TechnicalIndicators.atr(high, low, close, period)
        
        # Prevent division by zero in ATR
        atr_safe = np.where(atr_values == 0, 1e-10, atr_values)
        
        # Calculate +DI and -DI
        plus_di = 100 * pd.Series(plus_dm).rolling(window=period).mean().values / atr_safe
        minus_di = 100 * pd.Series(minus_dm).rolling(window=period).mean().values / atr_safe
        
        # Calculate DX and ADX - prevent division by zero
        di_sum = plus_di + minus_di
        di_sum = np.where(di_sum == 0, 1e-10, di_sum)
        dx = 100 * np.abs(plus_di - minus_di) / di_sum
        adx_values = pd.Series(dx).rolling(window=period).mean().values
        
        return adx_values
    
    @staticmethod
    def ema(close: np.ndarray, period: int) -> np.ndarray:
        """Calculate Exponential Moving Average"""
        return pd.Series(close).ewm(span=period, adjust=False).mean().values
    
    @staticmethod
    def bbw(upper: np.ndarray, middle: np.ndarray, lower: np.ndarray) -> np.ndarray:
        """Calculate Bollinger Band Width"""
        # Prevent division by zero (though unlikely with real price data)
        middle_safe = np.where(middle == 0, 1e-10, middle)
        return (upper - lower) / middle_safe


class MCMVRFStrategy:
    """
    Multi-Confluence Mean Reversion with Volatility Regime Filter Strategy
    """
    
    def __init__(self, config: Optional[Dict] = None):
        """
        Initialize strategy with configuration
        
        Args:
            config: Dictionary with strategy parameters (will be merged with defaults)
        """
        self.config = self._default_config()
        if config:
            self.config.update(config)
        self.positions: List[Position] = []
        self.signals_history: List[TradingSignal] = []
        
    def _default_config(self) -> Dict:
        """Default strategy configuration"""
        return {
            # Indicator parameters
            'bb_period': 20,
            'bb_std': 2.0,
            'rsi_period': 14,
            'rsi_oversold': 30,
            'rsi_overbought': 70,
            'stoch_k_period': 14,
            'stoch_d_period': 3,
            'stoch_smooth': 3,
            'stoch_oversold': 20,
            'stoch_overbought': 80,
            'atr_period': 14,
            'adx_period': 14,
            'adx_threshold': 30,
            'ema_fast': 50,
            'ema_slow': 200,
            
            # Volatility regime
            'bbw_min': 0.04,
            'bbw_max': 0.12,
            
            # Risk management
            'risk_per_trade': 0.02,  # 2%
            'stop_loss_atr_multiplier': 2.0,
            'take_profit_levels': [1.5, 2.5, 3.5],  # ATR multipliers
            'take_profit_percentages': [0.3, 0.4, 0.3],  # 30%, 40%, 30%
            'max_positions': 3,
            'daily_loss_limit': -0.04,  # -4%
            
            # Filters
            'min_confirmation_score': 3,
            'excluded_hours': [21, 22, 23],  # Avoid high volatility hours
            'max_holding_hours': 72,
        }
    
    def calculate_indicators(self, data: pd.DataFrame) -> Dict[str, np.ndarray]:
        """
        Calculate all technical indicators
        
        Args:
            data: DataFrame with OHLCV data (columns: open, high, low, close, volume)
            
        Returns:
            Dictionary with all calculated indicators
        """
        indicators = {}
        
        # Bollinger Bands
        bb_upper, bb_middle, bb_lower = TechnicalIndicators.bollinger_bands(
            data['close'].values, 
            self.config['bb_period'], 
            self.config['bb_std']
        )
        indicators['bb_upper'] = bb_upper
        indicators['bb_middle'] = bb_middle
        indicators['bb_lower'] = bb_lower
        indicators['bbw'] = TechnicalIndicators.bbw(bb_upper, bb_middle, bb_lower)
        
        # RSI
        indicators['rsi'] = TechnicalIndicators.rsi(
            data['close'].values, 
            self.config['rsi_period']
        )
        
        # Stochastic
        stoch_k, stoch_d = TechnicalIndicators.stochastic(
            data['high'].values,
            data['low'].values,
            data['close'].values,
            self.config['stoch_k_period'],
            self.config['stoch_d_period'],
            self.config['stoch_smooth']
        )
        indicators['stoch_k'] = stoch_k
        indicators['stoch_d'] = stoch_d
        
        # ATR
        indicators['atr'] = TechnicalIndicators.atr(
            data['high'].values,
            data['low'].values,
            data['close'].values,
            self.config['atr_period']
        )
        
        # ADX
        indicators['adx'] = TechnicalIndicators.adx(
            data['high'].values,
            data['low'].values,
            data['close'].values,
            self.config['adx_period']
        )
        
        # EMAs
        indicators['ema_50'] = TechnicalIndicators.ema(
            data['close'].values,
            self.config['ema_fast']
        )
        indicators['ema_200'] = TechnicalIndicators.ema(
            data['close'].values,
            self.config['ema_slow']
        )
        
        return indicators
    
    def check_long_signal(self, data: pd.DataFrame, indicators: Dict, index: int) -> Tuple[bool, Dict[str, bool], int]:
        """
        Check for LONG entry signal
        
        Returns:
            Tuple of (signal_valid, conditions_dict, confirmation_score)
        """
        conditions = {}
        
        # Mean reversion signal
        conditions['mean_reversion'] = data['close'].iloc[index] < indicators['bb_lower'][index]
        
        # Momentum confirmation
        conditions['rsi_oversold'] = indicators['rsi'][index] < self.config['rsi_oversold']
        conditions['stoch_oversold'] = indicators['stoch_k'][index] < self.config['stoch_oversold']
        conditions['stoch_cross'] = (
            indicators['stoch_k'][index] > indicators['stoch_d'][index] and
            indicators['stoch_k'][index - 1] <= indicators['stoch_d'][index - 1]
        )
        
        # Volatility regime
        bbw = indicators['bbw'][index]
        conditions['bbw_in_range'] = self.config['bbw_min'] < bbw < self.config['bbw_max']
        
        # Trend filter
        conditions['adx_ranging'] = indicators['adx'][index] < self.config['adx_threshold']
        
        # Direction bias
        conditions['above_ema200'] = data['close'].iloc[index] > indicators['ema_200'][index]
        
        # Time filter
        if 'timestamp' in data.columns:
            hour = pd.to_datetime(data['timestamp'].iloc[index]).hour
            conditions['time_filter'] = hour not in self.config['excluded_hours']
        else:
            conditions['time_filter'] = True
        
        # Calculate confirmation score
        confirmation_score = sum([
            conditions['mean_reversion'],
            conditions['rsi_oversold'],
            conditions['stoch_oversold'] and conditions['stoch_cross']
        ])
        
        # All filters must pass
        filters_pass = all([
            conditions['bbw_in_range'],
            conditions['adx_ranging'],
            conditions['time_filter']
        ])
        
        signal_valid = (
            confirmation_score >= self.config['min_confirmation_score'] and
            filters_pass and
            conditions['above_ema200']
        )
        
        return signal_valid, conditions, confirmation_score
    
    def check_short_signal(self, data: pd.DataFrame, indicators: Dict, index: int) -> Tuple[bool, Dict[str, bool], int]:
        """
        Check for SHORT entry signal
        
        Returns:
            Tuple of (signal_valid, conditions_dict, confirmation_score)
        """
        conditions = {}
        
        # Mean reversion signal
        conditions['mean_reversion'] = data['close'].iloc[index] > indicators['bb_upper'][index]
        
        # Momentum confirmation
        conditions['rsi_overbought'] = indicators['rsi'][index] > self.config['rsi_overbought']
        conditions['stoch_overbought'] = indicators['stoch_k'][index] > self.config['stoch_overbought']
        conditions['stoch_cross'] = (
            indicators['stoch_k'][index] < indicators['stoch_d'][index] and
            indicators['stoch_k'][index - 1] >= indicators['stoch_d'][index - 1]
        )
        
        # Volatility regime
        bbw = indicators['bbw'][index]
        conditions['bbw_in_range'] = self.config['bbw_min'] < bbw < self.config['bbw_max']
        
        # Trend filter
        conditions['adx_ranging'] = indicators['adx'][index] < self.config['adx_threshold']
        
        # Direction bias
        conditions['below_ema200'] = data['close'].iloc[index] < indicators['ema_200'][index]
        
        # Time filter
        if 'timestamp' in data.columns:
            hour = pd.to_datetime(data['timestamp'].iloc[index]).hour
            conditions['time_filter'] = hour not in self.config['excluded_hours']
        else:
            conditions['time_filter'] = True
        
        # Calculate confirmation score
        confirmation_score = sum([
            conditions['mean_reversion'],
            conditions['rsi_overbought'],
            conditions['stoch_overbought'] and conditions['stoch_cross']
        ])
        
        # All filters must pass
        filters_pass = all([
            conditions['bbw_in_range'],
            conditions['adx_ranging'],
            conditions['time_filter']
        ])
        
        signal_valid = (
            confirmation_score >= self.config['min_confirmation_score'] and
            filters_pass and
            conditions['below_ema200']
        )
        
        return signal_valid, conditions, confirmation_score
    
    def calculate_position_size(self, account_balance: float, entry_price: float, 
                               stop_loss: float) -> float:
        """
        Calculate position size based on risk management rules
        
        Args:
            account_balance: Total account balance
            entry_price: Entry price for the position
            stop_loss: Stop loss price
            
        Returns:
            Position size in base currency units
        """
        risk_amount = account_balance * self.config['risk_per_trade']
        stop_distance = abs(entry_price - stop_loss)
        
        if stop_distance == 0:
            return 0
        
        position_size = risk_amount / stop_distance
        
        # Cap at maximum 10% of account per trade
        max_position_value = account_balance * 0.1
        max_position_size = max_position_value / entry_price
        
        return min(position_size, max_position_size)
    
    def calculate_stop_loss_take_profit(self, entry_price: float, atr: float, 
                                       direction: str) -> Dict:
        """
        Calculate stop loss and take profit levels
        
        Args:
            entry_price: Entry price
            atr: Current ATR value
            direction: 'LONG' or 'SHORT'
            
        Returns:
            Dictionary with stop_loss and take_profits
        """
        if direction == 'LONG':
            stop_loss = entry_price - (self.config['stop_loss_atr_multiplier'] * atr)
            take_profits = [
                entry_price + (mult * atr) 
                for mult in self.config['take_profit_levels']
            ]
        else:  # SHORT
            stop_loss = entry_price + (self.config['stop_loss_atr_multiplier'] * atr)
            take_profits = [
                entry_price - (mult * atr) 
                for mult in self.config['take_profit_levels']
            ]
        
        return {
            'stop_loss': stop_loss,
            'take_profits': take_profits
        }
    
    def generate_signal(self, data: pd.DataFrame, account_balance: float, 
                       current_index: int = -1) -> TradingSignal:
        """
        Generate trading signal for current market conditions
        
        Args:
            data: DataFrame with OHLCV data
            account_balance: Current account balance
            current_index: Index to analyze (default: -1 for latest)
            
        Returns:
            TradingSignal object
        """
        # Calculate indicators
        indicators = self.calculate_indicators(data)
        
        # Check if we can trade (not too many positions)
        if len(self.positions) >= self.config['max_positions']:
            return TradingSignal(
                timestamp=pd.to_datetime(data['timestamp'].iloc[current_index]) if 'timestamp' in data.columns else datetime.now(),
                signal_type='NONE',
                entry_price=0,
                stop_loss=0,
                take_profits=[],
                position_size=0,
                confidence_score=0,
                atr=0,
                reasons={'max_positions_reached': True}
            )
        
        # Check for LONG signal
        long_valid, long_conditions, long_score = self.check_long_signal(data, indicators, current_index)
        
        # Check for SHORT signal
        short_valid, short_conditions, short_score = self.check_short_signal(data, indicators, current_index)
        
        # Determine signal
        if long_valid and long_score >= short_score:
            signal_type = 'LONG'
            conditions = long_conditions
            score = long_score
        elif short_valid:
            signal_type = 'SHORT'
            conditions = short_conditions
            score = short_score
        else:
            return TradingSignal(
                timestamp=pd.to_datetime(data['timestamp'].iloc[current_index]) if 'timestamp' in data.columns else datetime.now(),
                signal_type='NONE',
                entry_price=0,
                stop_loss=0,
                take_profits=[],
                position_size=0,
                confidence_score=max(long_score, short_score),
                atr=indicators['atr'][current_index],
                reasons=long_conditions if long_score >= short_score else short_conditions
            )
        
        # Calculate entry parameters
        entry_price = data['close'].iloc[current_index]
        atr = indicators['atr'][current_index]
        
        levels = self.calculate_stop_loss_take_profit(entry_price, atr, signal_type)
        position_size = self.calculate_position_size(
            account_balance, 
            entry_price, 
            levels['stop_loss']
        )
        
        signal = TradingSignal(
            timestamp=pd.to_datetime(data['timestamp'].iloc[current_index]) if 'timestamp' in data.columns else datetime.now(),
            signal_type=signal_type,
            entry_price=entry_price,
            stop_loss=levels['stop_loss'],
            take_profits=levels['take_profits'],
            position_size=position_size,
            confidence_score=score,
            atr=atr,
            reasons=conditions
        )
        
        self.signals_history.append(signal)
        return signal
    
    def manage_position(self, position: Position, current_price: float, 
                       current_atr: float) -> Position:
        """
        Manage active position with trailing stops and profit targets
        
        Args:
            position: Current position
            current_price: Current market price
            current_atr: Current ATR value
            
        Returns:
            Updated position
        """
        # Calculate profit
        if position.position_type == 'LONG':
            profit = current_price - position.entry_price
        else:  # SHORT
            profit = position.entry_price - current_price
        
        # Update highest profit
        position.highest_profit = max(position.highest_profit, profit)
        
        # Move to breakeven after 2 ATR profit
        if profit >= 2.0 * position.atr_at_entry:
            if position.position_type == 'LONG':
                position.stop_loss = max(position.stop_loss, position.entry_price)
            else:
                position.stop_loss = min(position.stop_loss, position.entry_price)
        
        # Trail stop after 3 ATR profit
        if profit >= 3.0 * position.atr_at_entry:
            trailing_distance = 1.5 * current_atr
            if position.position_type == 'LONG':
                new_stop = current_price - trailing_distance
                position.stop_loss = max(position.stop_loss, new_stop)
            else:
                new_stop = current_price + trailing_distance
                position.stop_loss = min(position.stop_loss, new_stop)
        
        return position
    
    def backtest(self, data: pd.DataFrame, initial_balance: float = 10000) -> Dict:
        """
        Run backtest on historical data
        
        Args:
            data: DataFrame with OHLCV data
            initial_balance: Starting account balance
            
        Returns:
            Dictionary with backtest results
        """
        balance = initial_balance
        equity_curve = [balance]
        trades = []
        
        indicators = self.calculate_indicators(data)
        
        for i in range(self.config['ema_slow'], len(data)):
            # Generate signal
            signal = self.generate_signal(data, balance, i)
            
            if signal.signal_type != 'NONE':
                # Simulate trade entry
                entry_price = signal.entry_price
                position_size = signal.position_size
                
                # Find exit
                exit_price = None
                exit_reason = None
                
                for j in range(i + 1, min(i + self.config['max_holding_hours'], len(data))):
                    current_price = data['close'].iloc[j]
                    
                    # Check stop loss
                    if signal.signal_type == 'LONG':
                        if current_price <= signal.stop_loss:
                            exit_price = signal.stop_loss
                            exit_reason = 'STOP_LOSS'
                            break
                        # Check take profits
                        for tp in signal.take_profits:
                            if current_price >= tp:
                                exit_price = tp
                                exit_reason = 'TAKE_PROFIT'
                                break
                    else:  # SHORT
                        if current_price >= signal.stop_loss:
                            exit_price = signal.stop_loss
                            exit_reason = 'STOP_LOSS'
                            break
                        # Check take profits
                        for tp in signal.take_profits:
                            if current_price <= tp:
                                exit_price = tp
                                exit_reason = 'TAKE_PROFIT'
                                break
                    
                    if exit_price:
                        break
                
                # If no exit found, use max holding time
                if not exit_price:
                    exit_price = data['close'].iloc[min(i + self.config['max_holding_hours'], len(data) - 1)]
                    exit_reason = 'TIME_EXIT'
                
                # Calculate P&L
                if signal.signal_type == 'LONG':
                    pnl = (exit_price - entry_price) * position_size
                else:
                    pnl = (entry_price - exit_price) * position_size
                
                balance += pnl
                equity_curve.append(balance)
                
                trades.append({
                    'entry_time': signal.timestamp,
                    'entry_price': entry_price,
                    'exit_price': exit_price,
                    'exit_reason': exit_reason,
                    'type': signal.signal_type,
                    'pnl': pnl,
                    'return_pct': (pnl / (entry_price * position_size)) * 100
                })
        
        # Calculate statistics
        winning_trades = [t for t in trades if t['pnl'] > 0]
        losing_trades = [t for t in trades if t['pnl'] <= 0]
        
        results = {
            'initial_balance': initial_balance,
            'final_balance': balance,
            'total_return': ((balance - initial_balance) / initial_balance) * 100,
            'total_trades': len(trades),
            'winning_trades': len(winning_trades),
            'losing_trades': len(losing_trades),
            'win_rate': (len(winning_trades) / len(trades) * 100) if trades else 0,
            'avg_win': np.mean([t['pnl'] for t in winning_trades]) if winning_trades else 0,
            'avg_loss': np.mean([t['pnl'] for t in losing_trades]) if losing_trades else 0,
            'profit_factor': (
                sum([t['pnl'] for t in winning_trades]) / abs(sum([t['pnl'] for t in losing_trades]))
                if losing_trades and sum([t['pnl'] for t in losing_trades]) != 0 else 0
            ),
            'max_drawdown': self._calculate_max_drawdown(equity_curve),
            'sharpe_ratio': self._calculate_sharpe_ratio(equity_curve),
            'equity_curve': equity_curve,
            'trades': trades
        }
        
        return results
    
    def _calculate_max_drawdown(self, equity_curve: List[float]) -> float:
        """Calculate maximum drawdown percentage"""
        peak = equity_curve[0]
        max_dd = 0
        
        for value in equity_curve:
            if value > peak:
                peak = value
            dd = ((peak - value) / peak) * 100
            if dd > max_dd:
                max_dd = dd
        
        return max_dd
    
    def _calculate_sharpe_ratio(self, equity_curve: List[float], risk_free_rate: float = 0.02) -> float:
        """Calculate Sharpe ratio"""
        if len(equity_curve) < 2:
            return 0
        
        returns = np.diff(equity_curve) / equity_curve[:-1]
        excess_returns = returns - (risk_free_rate / 252)  # Assuming daily returns
        
        if len(excess_returns) == 0 or np.std(excess_returns) == 0:
            return 0
        
        return np.mean(excess_returns) / np.std(excess_returns) * np.sqrt(252)


def main():
    """Example usage of the strategy"""
    print("MCM-VRF Trading Strategy Implementation")
    print("=" * 50)
    print()
    print("This is a production-ready implementation of the")
    print("Multi-Confluence Mean Reversion with Volatility Regime Filter strategy.")
    print()
    print("To use this strategy:")
    print("1. Load your OHLCV data into a pandas DataFrame")
    print("2. Initialize the strategy: strategy = MCMVRFStrategy()")
    print("3. Generate signals: signal = strategy.generate_signal(data, account_balance)")
    print("4. Run backtest: results = strategy.backtest(data, initial_balance)")
    print()
    print("Example:")
    print("-" * 50)
    print("""
    import pandas as pd
    from strategy_implementation import MCMVRFStrategy
    
    # Load your data
    data = pd.DataFrame({
        'timestamp': [...],
        'open': [...],
        'high': [...],
        'low': [...],
        'close': [...],
        'volume': [...]
    })
    
    # Initialize strategy
    strategy = MCMVRFStrategy()
    
    # Generate signal for current market
    signal = strategy.generate_signal(data, account_balance=10000)
    
    if signal.signal_type != 'NONE':
        print(f"Signal: {signal.signal_type}")
        print(f"Entry: {signal.entry_price}")
        print(f"Stop Loss: {signal.stop_loss}")
        print(f"Take Profits: {signal.take_profits}")
        print(f"Position Size: {signal.position_size}")
    
    # Run backtest
    results = strategy.backtest(data, initial_balance=10000)
    print(f"Win Rate: {results['win_rate']:.2f}%")
    print(f"Profit Factor: {results['profit_factor']:.2f}")
    print(f"Total Return: {results['total_return']:.2f}%")
    """)


if __name__ == "__main__":
    main()
