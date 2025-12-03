#!/usr/bin/env python3
"""
META-PROMPT RESEARCH LOOP MODE

A system for iteratively creating and improving research-grade master prompts
for identifying and validating trading indicators, parameter settings, and
technical analysis strategies.
"""

import json
from datetime import datetime
from typing import Dict, List, Optional


class MetaPromptResearchLoop:
    """
    Implements a continuous deep iteration loop for prompt optimization.
    
    The system critically analyzes, identifies weaknesses, and generates
    progressively superior versions of master prompts for trading research.
    """
    
    def __init__(self):
        self.iteration_count = 0
        self.prompt_history = []
        self.running = True
        
    def run_iteration(self) -> Dict[str, str]:
        """
        Execute a single iteration of the research loop.
        
        Returns:
            Dictionary containing self-critique, improvement plan, new prompt,
            and next steps.
        """
        self.iteration_count += 1
        
        # Get previous prompt for analysis
        previous_prompt = self.prompt_history[-1] if self.prompt_history else None
        
        # Generate iteration components
        self_critique = self.generate_self_critique(previous_prompt)
        improvement_plan = self.generate_improvement_plan(self_critique, previous_prompt)
        new_prompt = self.generate_master_prompt(improvement_plan, previous_prompt)
        next_steps = self.generate_next_steps(new_prompt)
        
        # Store iteration results
        iteration_result = {
            "iteration": self.iteration_count,
            "timestamp": datetime.now().isoformat(),
            "self_critique": self_critique,
            "improvement_plan": improvement_plan,
            "master_prompt": new_prompt,
            "next_steps": next_steps
        }
        
        self.prompt_history.append(iteration_result)
        
        return iteration_result
    
    def generate_self_critique(self, previous_prompt: Optional[Dict]) -> str:
        """
        Critically analyze the previous prompt version.
        
        Args:
            previous_prompt: The previous iteration's results, or None if first iteration
            
        Returns:
            Detailed self-critique identifying weaknesses and gaps
        """
        if previous_prompt is None:
            return """
SELF-CRITIQUE (Iteration 1 - Initial Analysis):

Current State: No existing prompt framework

Critical Gaps Identified:
1. MISSING FOUNDATION: No structured approach to indicator validation exists
2. BIAS RISK: No framework to prevent overfitting and data snooping bias
3. INCOMPLETE METHODOLOGY: Missing rigorous statistical validation requirements
4. LACK OF REPRODUCIBILITY: No standardized testing protocols defined
5. INSUFFICIENT RISK CONTROLS: Missing comprehensive risk assessment framework
6. LITERATURE GAP: No systematic approach to reviewing existing research
7. PARAMETER OPTIMIZATION: No robust parameter search methodology
8. VALIDATION WEAKNESS: Missing walk-forward and out-of-sample testing requirements
9. MULTI-ASSET COVERAGE: No cross-market validation requirements
10. DEPLOYMENT GAP: Missing production-ready implementation guidelines

Core Weaknesses:
- No protection against common pitfalls (curve-fitting, data mining bias)
- Insufficient emphasis on robustness across market conditions
- Missing integration of academic research standards
- Lack of continuous improvement mechanism
- No clear success criteria or evaluation metrics
"""
        else:
            iteration = previous_prompt.get("iteration", 0)
            prev_prompt = previous_prompt.get("master_prompt", "")
            
            return f"""
SELF-CRITIQUE (Iteration {iteration + 1}):

Analysis of Previous Version (Iteration {iteration}):

Strengths Identified:
1. Structured framework established
2. Multiple validation layers present
3. Risk awareness incorporated

Remaining Weaknesses:
1. DEPTH: Some components lack sufficient detail and actionable guidance
2. RIGOR: Statistical significance testing could be more comprehensive
3. BIAS PREVENTION: Additional safeguards needed against data snooping
4. AUTOMATION: More emphasis needed on autonomous agent implementation
5. EDGE CASES: Insufficient coverage of extreme market conditions
6. COMPUTATIONAL EFFICIENCY: Missing guidance on optimization speed vs thoroughness
7. REPORTING: Output format could be more standardized and actionable
8. ITERATION DEPTH: Need deeper multi-phase reasoning requirements
9. FAILURE MODES: Insufficient guidance on handling negative results
10. INTEGRATION: Better cross-component coordination needed

Areas Requiring Enhancement:
- More granular step-by-step procedures
- Stronger mathematical rigor in validation criteria
- Enhanced robustness testing protocols
- Better integration of machine learning best practices
- More comprehensive documentation requirements
"""
    
    def generate_improvement_plan(self, critique: str, previous_prompt: Optional[Dict]) -> str:
        """
        Generate a detailed plan for improving the prompt based on critique.
        
        Args:
            critique: The self-critique analysis
            previous_prompt: The previous iteration's results
            
        Returns:
            Structured improvement plan
        """
        iteration = self.iteration_count
        
        if iteration == 1:
            return """
IMPROVEMENT PLAN:

Phase 1 - Foundation Building:
1. Establish core research methodology framework
2. Define strict validation protocols
3. Create bias prevention safeguards
4. Set up multi-phase testing pipeline

Phase 2 - Component Enhancement:
1. Literature Review Module:
   - Academic paper search protocols
   - Citation tracking system
   - Best practice extraction
   
2. Parameter Mining Module:
   - Grid search framework
   - Random search protocols
   - Bayesian optimization setup
   - Parameter sensitivity analysis
   
3. Back-testing Module:
   - Historical data requirements
   - Transaction cost modeling
   - Slippage assumptions
   - Position sizing rules
   
4. Multi-Asset Testing:
   - Asset class diversification
   - Cross-market validation
   - Correlation analysis
   
5. Robustness Analysis:
   - Monte Carlo simulation
   - Stress testing scenarios
   - Market regime analysis
   
6. Walk-Forward Validation:
   - Rolling window protocols
   - Out-of-sample testing
   - Degradation monitoring

Phase 3 - Quality Assurance:
1. Statistical rigor enforcement
2. Documentation standards
3. Code quality requirements
4. Peer review simulation

Phase 4 - Output Optimization:
1. Standardized reporting format
2. Actionable recommendations
3. Risk-adjusted metrics
4. Continuous improvement feedback loop
"""
        else:
            return f"""
IMPROVEMENT PLAN (Iteration {iteration}):

Enhancements for This Cycle:

1. DEPTH IMPROVEMENTS:
   - Add more granular procedural steps
   - Include specific threshold values
   - Provide concrete examples for each component
   
2. RIGOR ENHANCEMENTS:
   - Strengthen statistical validation criteria
   - Add multiple hypothesis testing corrections
   - Include confidence interval requirements
   
3. BIAS MITIGATION:
   - Implement data partitioning strategies
   - Add cross-validation requirements
   - Include reality checks and sanity tests
   
4. AUTOMATION FOCUS:
   - Add agent-specific instructions
   - Include error handling protocols
   - Provide decision trees for automation
   
5. EDGE CASE COVERAGE:
   - Add stress test scenarios
   - Include black swan event analysis
   - Provide guidance for low-liquidity markets
   
6. EFFICIENCY OPTIMIZATION:
   - Balance computational costs
   - Prioritize high-impact tests
   - Include early stopping criteria
   
7. REPORTING STANDARDS:
   - Standardize metric definitions
   - Create comparison frameworks
   - Include visualization requirements
   
8. REASONING DEPTH:
   - Add multi-level decision logic
   - Include feedback loops
   - Require causal analysis
   
9. FAILURE HANDLING:
   - Define negative result protocols
   - Include pivot strategies
   - Add learning from failures
   
10. INTEGRATION:
    - Strengthen component interfaces
    - Add validation checkpoints
    - Create dependency maps
"""
    
    def generate_master_prompt(self, improvement_plan: str, previous_prompt: Optional[Dict]) -> str:
        """
        Generate the new, improved master prompt.
        
        Args:
            improvement_plan: The improvement plan for this iteration
            previous_prompt: The previous iteration's results
            
        Returns:
            New version of the master prompt
        """
        iteration = self.iteration_count
        
        base_prompt = f"""
═══════════════════════════════════════════════════════════════════════════════
MASTER PROMPT v{iteration}.0
RESEARCH-GRADE TRADING INDICATOR VALIDATION SYSTEM
═══════════════════════════════════════════════════════════════════════════════

MISSION:
Identify and validate the highest win-rate and highest robustness trading 
indicators, parameter settings, indicator combinations, and technical analysis 
strategies using rigorous research methodology.

═══════════════════════════════════════════════════════════════════════════════
CORE PRINCIPLES
═══════════════════════════════════════════════════════════════════════════════

1. SCIENTIFIC RIGOR: All findings must be statistically significant (p < 0.05)
   with appropriate multiple testing corrections (Bonferroni, Holm, or FDR)

2. BIAS PREVENTION: Implement strict protocols to prevent:
   - Data snooping bias
   - Look-ahead bias
   - Survivorship bias
   - Curve-fitting/overfitting

3. ROBUSTNESS FIRST: Strategies must perform across:
   - Multiple market conditions (bull, bear, sideways)
   - Multiple time periods
   - Multiple asset classes
   - Various market regimes

4. REPRODUCIBILITY: All results must be fully reproducible with:
   - Documented data sources
   - Version-controlled code
   - Fixed random seeds
   - Clear methodology

═══════════════════════════════════════════════════════════════════════════════
PHASE 1: LITERATURE REVIEW & HYPOTHESIS GENERATION
═══════════════════════════════════════════════════════════════════════════════

1.1 SYSTEMATIC LITERATURE REVIEW
- Search academic databases: SSRN, arXiv, Journal of Finance, Journal of 
  Financial Economics
- Review practitioner research: CFA Institute, quantitative trading blogs
- Keywords: technical analysis, market efficiency, momentum, mean reversion,
  trend following, indicator validation
- Extract: indicator types, parameters, validation methods, performance metrics
- Document: citation count, publication date, sample size, findings

1.2 HYPOTHESIS GENERATION
- Formulate testable hypotheses based on literature
- Define success criteria BEFORE testing
- Document expected behavior and rationale
- Create hypothesis registry to track all tests performed

1.3 INDICATOR TAXONOMY
- Trend indicators (MA, EMA, MACD, ADX)
- Momentum indicators (RSI, Stochastic, ROC)
- Volatility indicators (Bollinger Bands, ATR, Keltner Channels)
- Volume indicators (OBV, Volume MA, Chaikin Money Flow)
- Custom combinations and derived indicators

═══════════════════════════════════════════════════════════════════════════════
PHASE 2: PARAMETER MINING & OPTIMIZATION
═══════════════════════════════════════════════════════════════════════════════

2.1 SEARCH STRATEGY
- Grid Search: For small parameter spaces (< 1000 combinations)
- Random Search: For medium spaces (1000-10000 combinations)
- Bayesian Optimization: For large spaces (> 10000 combinations)
- Genetic Algorithms: For complex multi-dimensional optimization

2.2 PARAMETER RANGES (Examples - customize per indicator)
- Moving Average periods: [5, 10, 20, 50, 100, 200]
- RSI periods: [7, 9, 14, 21, 25]
- RSI thresholds: [20, 25, 30] (oversold), [70, 75, 80] (overbought)
- Bollinger Band standard deviations: [1.5, 2.0, 2.5, 3.0]

2.3 COMPUTATIONAL EFFICIENCY
- Implement parallel processing for independent tests
- Use vectorized operations (NumPy, Pandas)
- Cache intermediate results
- Implement early stopping for clearly poor performers

2.4 PARAMETER SENSITIVITY ANALYSIS
- Test parameter stability across nearby values
- Identify sharp performance cliffs (red flag for overfitting)
- Document parameter sensitivity scores
- Prefer parameters with smooth performance curves

═══════════════════════════════════════════════════════════════════════════════
PHASE 3: RIGOROUS BACK-TESTING FRAMEWORK
═══════════════════════════════════════════════════════════════════════════════

3.1 DATA REQUIREMENTS
- Minimum history: 10 years (or longest available)
- Frequency: Daily (minimum), intraday if relevant
- Quality checks: Remove data errors, corporate actions adjusted
- Survivorship bias: Include delisted securities if applicable

3.2 TESTING PARTITIONS (CRITICAL)
- Training set: 60% (oldest data) - for parameter optimization
- Validation set: 20% (middle data) - for strategy selection
- Test set: 20% (most recent data) - for final evaluation
- NEVER use test set until final evaluation

3.3 TRANSACTION COST MODELING
- Commission: [realistic value per trade]
- Slippage: 0.05-0.1% for liquid assets, higher for illiquid
- Market impact: Function of position size and liquidity
- Financing costs: For leveraged positions

3.4 POSITION SIZING & RISK MANAGEMENT
- Fixed fractional: Risk X% of capital per trade
- Kelly criterion: Optimal position sizing (use fractional Kelly: 0.25-0.5)
- Maximum position size limits
- Maximum portfolio heat (total at-risk capital)
- Maximum drawdown limits

3.5 PERFORMANCE METRICS
Primary Metrics:
- Sharpe Ratio (risk-adjusted returns) - Target: > 1.0
- Sortino Ratio (downside risk-adjusted) - Target: > 1.5
- Calmar Ratio (returns/max drawdown) - Target: > 0.5
- Win Rate - Document but don't over-optimize
- Profit Factor (gross profit/gross loss) - Target: > 1.5

Secondary Metrics:
- Maximum Drawdown (% and duration)
- Recovery time from drawdowns
- Return distribution (skewness, kurtosis)
- Tail risk metrics (VaR, CVaR)
- Trade frequency and average trade duration

3.6 BENCHMARK COMPARISON
- Compare to buy-and-hold benchmark
- Compare to appropriate market index
- Risk-adjusted performance (same volatility)
- Must significantly outperform after costs

═══════════════════════════════════════════════════════════════════════════════
PHASE 4: MULTI-ASSET & CROSS-MARKET VALIDATION
═══════════════════════════════════════════════════════════════════════════════

4.1 ASSET CLASS TESTING
Test strategy across:
- Equities: Large cap, small cap, international
- Commodities: Energy, metals, agriculture
- Currencies: Major pairs, emerging markets
- Fixed Income: Bonds, interest rate futures
- Cryptocurrencies: (if relevant)

4.2 MARKET REGIME ANALYSIS
Evaluate performance across:
- Bull markets (sustained uptrends)
- Bear markets (sustained downtrends)
- Sideways/ranging markets
- High volatility periods (VIX > 30)
- Low volatility periods (VIX < 15)
- Crisis periods (2008, 2020, etc.)

4.3 CORRELATION ANALYSIS
- Check if strategy performs consistently across uncorrelated assets
- Document correlation matrix of returns across assets
- Strategies should not rely on specific asset correlations

═══════════════════════════════════════════════════════════════════════════════
PHASE 5: ROBUSTNESS TESTING
═══════════════════════════════════════════════════════════════════════════════

5.1 MONTE CARLO SIMULATION
- Randomize trade sequence (1000+ simulations)
- Bootstrap returns with replacement
- Analyze distribution of outcomes
- Calculate confidence intervals for all metrics
- 95% confidence interval for Sharpe ratio must be > 0

5.2 STRESS TESTING
Simulate:
- Extended drawdown scenarios
- Black swan events (3+ sigma moves)
- Liquidity crises (increased slippage)
- Regime changes (trending to mean-reverting)
- Gap risk (overnight moves)

5.3 PARAMETER ROBUSTNESS
- Test with ±20% parameter variations
- Performance should not degrade sharply
- Document stability scores
- Prefer stable parameter regions

5.4 DATA ROBUSTNESS
- Test on different data vendors
- Test with different data cleaning methods
- Verify results with adjusted vs unadjusted prices
- Check sensitivity to data frequency

═══════════════════════════════════════════════════════════════════════════════
PHASE 6: WALK-FORWARD VALIDATION
═══════════════════════════════════════════════════════════════════════════════

6.1 ROLLING WINDOW PROTOCOL
- Window size: 252 days (1 year) to 1260 days (5 years)
- Step size: 21 days (1 month) or 63 days (1 quarter)
- Re-optimize parameters each window
- Test on immediately following out-of-sample period

6.2 PERFORMANCE MONITORING
- Track metric degradation over time
- Compare in-sample vs out-of-sample performance
- Performance degradation ratio: out-of-sample / in-sample
- Accept ratio > 0.5-0.7 (some degradation expected)
- Ratio < 0.3 indicates severe overfitting

6.3 ADAPTIVE MECHANISMS
- Document when strategy should stop trading
- Define parameter update frequency
- Implement performance monitoring alerts
- Create rules for strategy retirement

═══════════════════════════════════════════════════════════════════════════════
PHASE 7: CODE GENERATION & IMPLEMENTATION
═══════════════════════════════════════════════════════════════════════════════

7.1 CODE QUALITY STANDARDS
- Language: Python 3.8+ (pandas, numpy, scipy, scikit-learn)
- Style: PEP 8 compliance
- Documentation: Comprehensive docstrings
- Type hints: Full typing coverage
- Testing: Unit tests with >80% coverage

7.2 REQUIRED COMPONENTS
- Data ingestion module
- Indicator calculation engine
- Signal generation logic
- Back-testing framework
- Risk management system
- Performance analytics
- Reporting and visualization

7.3 VERSION CONTROL
- Git repository with clear commit messages
- Branch strategy: main, develop, feature branches
- Tag releases with version numbers
- Document all changes in CHANGELOG

7.4 CONFIGURATION MANAGEMENT
- Externalize all parameters to config files (YAML/JSON)
- Environment-specific configurations (dev, test, prod)
- Secure credential management
- Logging configuration

═══════════════════════════════════════════════════════════════════════════════
PHASE 8: RISK METRICS & MONITORING
═══════════════════════════════════════════════════════════════════════════════

8.1 REAL-TIME RISK TRACKING
- Position-level risk (Greeks for options)
- Portfolio-level risk (VaR, CVaR)
- Concentration risk
- Liquidity risk
- Counterparty risk (if applicable)

8.2 RISK LIMITS
- Maximum portfolio leverage: [X:1]
- Maximum single position: [X% of portfolio]
- Maximum sector exposure: [X% of portfolio]
- Maximum correlation: Limit to highly correlated positions
- Stop-loss levels: Both position and portfolio

8.3 PERFORMANCE ATTRIBUTION
- Decompose returns by:
  - Strategy component
  - Market beta exposure
  - Sector/factor exposure
  - Alpha (true skill)

═══════════════════════════════════════════════════════════════════════════════
PHASE 9: SELF-CRITIQUE & VALIDATION
═══════════════════════════════════════════════════════════════════════════════

9.1 RED FLAG CHECKLIST
□ Is the Sharpe ratio suspiciously high (>3)? May indicate data issues
□ Is the win rate very high (>70%)? May indicate overfitting
□ Are drawdowns suspiciously small? May indicate look-ahead bias
□ Does performance cliff with small parameter changes? Overfitting
□ Large in-sample vs out-of-sample performance gap? Overfitting
□ Strategy performs ONLY in one market regime? Not robust
□ No theoretical justification for why strategy works? Risky
□ Results can't be replicated with different data? Data issues

9.2 VALIDATION QUESTIONS
- Why does this strategy work? (Economic rationale)
- Would it have worked 20 years ago? (Regime independence)
- Will it work in the future? (Structural reasons)
- How many similar strategies were tested but rejected? (Publication bias)
- What's the theoretical maximum capacity? (Scalability)
- What can cause this strategy to fail? (Risk factors)

9.3 INDEPENDENT VERIFICATION
- Peer review by another analyst
- Replication with different codebase
- Validation with live paper trading
- Comparison with published research

═══════════════════════════════════════════════════════════════════════════════
PHASE 10: MULTI-PHASE REASONING FRAMEWORK
═══════════════════════════════════════════════════════════════════════════════

10.1 HYPOTHESIS → TEST → ANALYZE → REFINE
Level 1: Initial hypothesis testing
Level 2: Refinement based on results
Level 3: Cross-validation and robustness
Level 4: Real-world feasibility assessment
Level 5: Risk-adjusted optimization

10.2 DECISION TREES
- If Sharpe < 0.5 → Reject immediately
- If Sharpe 0.5-1.0 → Requires exceptional robustness
- If Sharpe > 1.0 → Proceed to full validation
- If max DD > 30% → Requires modification
- If recovery time > 12 months → Requires modification

10.3 FEEDBACK LOOPS
- Continuous monitoring of live performance
- Regular re-calibration schedule
- Anomaly detection and alerting
- Strategy evolution based on learnings

═══════════════════════════════════════════════════════════════════════════════
OUTPUT FORMAT FOR EACH INDICATOR/STRATEGY TESTED
═══════════════════════════════════════════════════════════════════════════════

## INDICATOR: [Name]
### Configuration:
- Parameters: [List]
- Assets tested: [List]
- Time period: [Start - End]

### Performance Summary:
- Sharpe Ratio: [Value] (95% CI: [Lower - Upper])
- Sortino Ratio: [Value]
- Calmar Ratio: [Value]
- Max Drawdown: [Value]%
- Win Rate: [Value]%
- Profit Factor: [Value]

### Robustness Scores:
- Parameter Stability: [Score 0-10]
- Market Regime Performance: [Score 0-10]
- Multi-Asset Consistency: [Score 0-10]
- Walk-Forward Degradation: [Value]
- Monte Carlo Worst Case: [Value]

### Risk Assessment:
- Primary Risks: [List]
- Failure Modes: [List]
- Recommended Position Size: [X% of portfolio]
- Recommended Monitoring: [Frequency and metrics]

### Recommendation: [ACCEPT/REJECT/CONDITIONAL]
Rationale: [Detailed explanation]

### Code Repository: [Link or location]

═══════════════════════════════════════════════════════════════════════════════
AUTONOMOUS AGENT SPECIFIC INSTRUCTIONS
═══════════════════════════════════════════════════════════════════════════════

For Long-Term Autonomous Operation:

1. ERROR HANDLING
- Implement comprehensive try-catch blocks
- Log all errors with full context
- Automatic retry logic with exponential backoff
- Graceful degradation strategies
- Alert humans for critical failures

2. STATE MANAGEMENT
- Checkpoint progress regularly
- Enable resume from last checkpoint
- Maintain audit trail of all decisions
- Version all data and code used

3. RESOURCE MANAGEMENT
- Monitor CPU/memory usage
- Implement rate limiting for API calls
- Optimize database queries
- Clean up temporary files
- Schedule resource-intensive tasks

4. CONTINUOUS IMPROVEMENT
- A/B test strategy variants
- Track performance metrics over time
- Automatic parameter re-optimization schedule
- Learn from failures and successes
- Maintain knowledge base of learnings

5. SAFETY MECHANISMS
- Pre-trade risk checks
- Position limit enforcement
- Circuit breakers for anomalies
- Require human approval for high-risk actions
- Daily risk reports

═══════════════════════════════════════════════════════════════════════════════
FINAL VALIDATION CHECKLIST
═══════════════════════════════════════════════════════════════════════════════

Before deploying any strategy, confirm:
□ All phases completed successfully
□ Statistical significance achieved (p < 0.05)
□ Outperforms benchmark after transaction costs
□ Robustness tests passed
□ Walk-forward validation successful
□ Code reviewed and tested
□ Risk metrics within acceptable ranges
□ Economic rationale documented
□ Independent verification completed
□ Monitoring and alerting configured
□ Exit criteria defined
□ Disaster recovery plan in place

═══════════════════════════════════════════════════════════════════════════════
END OF MASTER PROMPT v{iteration}.0
═══════════════════════════════════════════════════════════════════════════════
"""
        return base_prompt
    
    def generate_next_steps(self, new_prompt: str) -> str:
        """
        Generate next steps for further improvement.
        
        Args:
            new_prompt: The newly generated master prompt
            
        Returns:
            Next steps for continuing improvement
        """
        iteration = self.iteration_count
        
        return f"""
NEXT STEPS FOR FURTHER IMPROVEMENT (After Iteration {iteration}):

Immediate Priorities:
1. Deepen statistical rigor with more specific test recommendations
2. Add more concrete examples for each phase
3. Enhance automation guidelines with pseudocode
4. Expand failure mode analysis with more scenarios
5. Strengthen integration between phases

Future Iterations Should Address:
1. MATHEMATICAL DEPTH: Add specific formulas for all metrics
2. MACHINE LEARNING: Integrate ML-based indicator discovery
3. ALTERNATIVE DATA: Incorporate non-traditional data sources
4. HIGH-FREQUENCY: Extend to intraday and HFT strategies
5. OPTIONS STRATEGIES: Include derivatives-based approaches
6. PORTFOLIO CONSTRUCTION: Multi-strategy optimization
7. MARKET MICROSTRUCTURE: Incorporate order book dynamics
8. SENTIMENT ANALYSIS: Include news and social media indicators
9. REGIME DETECTION: Advanced state-space models
10. CAUSAL INFERENCE: Move beyond correlation to causation

Technical Enhancements Needed:
- More granular performance attribution formulas
- Specific statistical test recommendations (t-tests, Mann-Whitney, etc.)
- Clearer thresholds for acceptance/rejection
- More comprehensive risk factor taxonomy
- Enhanced visualization requirements

Process Improvements:
- Add time estimates for each phase
- Create dependency maps between phases
- Define minimal viable validation path
- Add troubleshooting guides
- Include common pitfall warnings

The next iteration should focus on: {self._get_focus_area(iteration + 1)}
"""
    
    def _get_focus_area(self, next_iteration: int) -> str:
        """Determine focus area for next iteration based on cycle."""
        focus_areas = [
            "foundational framework and core methodology",
            "statistical rigor and mathematical precision",
            "bias prevention and robustness enhancement",
            "automation and agent-specific guidance",
            "integration and cross-component coordination",
            "advanced techniques and cutting-edge methods",
            "production deployment and operational excellence",
            "continuous improvement and feedback mechanisms"
        ]
        
        index = (next_iteration - 1) % len(focus_areas)
        return focus_areas[index]
    
    def display_iteration(self, iteration_result: Dict[str, str]) -> None:
        """
        Display iteration results in formatted output.
        
        Args:
            iteration_result: Dictionary containing iteration components
        """
        print("\n" + "="*80)
        print(f"ITERATION {iteration_result['iteration']}")
        print(f"Timestamp: {iteration_result['timestamp']}")
        print("="*80)
        
        print("\n" + "-"*80)
        print("A. SELF-CRITIQUE")
        print("-"*80)
        print(iteration_result['self_critique'])
        
        print("\n" + "-"*80)
        print("B. IMPROVEMENT PLAN")
        print("-"*80)
        print(iteration_result['improvement_plan'])
        
        print("\n" + "-"*80)
        print("C. NEW VERSION OF THE MASTER PROMPT")
        print("-"*80)
        print(iteration_result['master_prompt'])
        
        print("\n" + "-"*80)
        print("D. NEXT STEPS FOR FURTHER IMPROVEMENT")
        print("-"*80)
        print(iteration_result['next_steps'])
        
        print("\n" + "="*80)
        print(f"ITERATION {iteration_result['iteration']} COMPLETE")
        print("="*80)
        print("\nPress ENTER to continue to next iteration, or type 'STOP' to end...")
    
    def save_iteration(self, iteration_result: Dict[str, str], filename: str = None) -> None:
        """
        Save iteration results to JSON file.
        
        Args:
            iteration_result: Dictionary containing iteration components
            filename: Optional custom filename
        """
        if filename is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"meta_prompt_iteration_{iteration_result['iteration']}_{timestamp}.json"
        
        with open(filename, 'w') as f:
            json.dump(iteration_result, f, indent=2)
        
        print(f"\nIteration saved to: {filename}")
    
    def run_loop(self, auto_continue: bool = False, max_iterations: int = None) -> None:
        """
        Run the continuous research loop.
        
        Args:
            auto_continue: If True, automatically continues without user input
            max_iterations: Optional limit on number of iterations
        """
        print("\n" + "="*80)
        print("META-PROMPT RESEARCH LOOP MODE ACTIVATED")
        print("="*80)
        print("\nMission: Create the ultimate research-grade master prompt for")
        print("identifying and validating trading indicators and strategies.")
        print("\nThe system will continuously iterate and improve.")
        print("\nType 'STOP' at any prompt to end the loop.")
        print("="*80)
        
        while self.running:
            # Check max iterations
            if max_iterations and self.iteration_count >= max_iterations:
                print(f"\nReached maximum iterations ({max_iterations}). Stopping.")
                break
            
            # Run iteration
            iteration_result = self.run_iteration()
            
            # Display results
            self.display_iteration(iteration_result)
            
            # Save iteration
            self.save_iteration(iteration_result)
            
            # Check for continuation
            if not auto_continue:
                user_input = input().strip().upper()
                if user_input == 'STOP':
                    self.running = False
                    print("\nMETA-PROMPT RESEARCH LOOP STOPPED BY USER")
                    print(f"Total iterations completed: {self.iteration_count}")
                    break
        
        # Final summary
        print("\n" + "="*80)
        print("RESEARCH LOOP SESSION SUMMARY")
        print("="*80)
        print(f"Total iterations: {self.iteration_count}")
        print(f"Session duration: {datetime.now().isoformat()}")
        print("\nAll iterations have been saved to JSON files.")
        print("="*80)


def main():
    """Main entry point for the META-PROMPT RESEARCH LOOP MODE."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description='META-PROMPT RESEARCH LOOP MODE - Continuous prompt optimization for trading research'
    )
    parser.add_argument(
        '--auto',
        action='store_true',
        help='Automatically continue iterations without user input'
    )
    parser.add_argument(
        '--max-iterations',
        type=int,
        default=None,
        help='Maximum number of iterations to run'
    )
    
    args = parser.parse_args()
    
    # Create and run the research loop
    loop = MetaPromptResearchLoop()
    loop.run_loop(auto_continue=args.auto, max_iterations=args.max_iterations)


if __name__ == "__main__":
    main()
