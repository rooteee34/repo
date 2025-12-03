# META-PROMPT RESEARCH LOOP MODE - Usage Guide

## Overview

The META-PROMPT RESEARCH LOOP MODE is an advanced system designed to iteratively create and optimize research-grade master prompts for identifying and validating trading indicators, parameter settings, and technical analysis strategies.

## Features

### Core Capabilities
- **Continuous Deep Iteration**: Automatically analyzes, critiques, and improves prompts
- **Self-Critique Mechanism**: Identifies weaknesses, gaps, and biases in each version
- **Improvement Planning**: Generates structured plans for enhancement
- **Progressive Enhancement**: Each iteration builds upon previous versions
- **Comprehensive Coverage**: Includes all aspects of trading research validation

### Research Components Included
1. **Literature Review**: Academic and practitioner research integration
2. **Parameter Mining**: Grid search, random search, Bayesian optimization
3. **Back-testing Framework**: Rigorous historical validation protocols
4. **Multi-Asset Testing**: Cross-market validation requirements
5. **Robustness Analysis**: Monte Carlo simulation, stress testing
6. **Walk-Forward Validation**: Out-of-sample testing protocols
7. **Code Generation**: Production-ready implementation guidelines
8. **Risk Metrics**: Comprehensive risk management framework
9. **Self-Critique**: Built-in validation and quality checks
10. **Multi-Phase Reasoning**: Deep decision-making frameworks

## Installation

### Prerequisites
- Python 3.8 or higher
- No external dependencies required (uses standard library only)

### Setup
```bash
# Clone or download the repository
cd /path/to/repo

# Make the script executable (optional)
chmod +x meta_prompt_research_loop.py
```

## Usage

### Basic Usage (Interactive Mode)

Run the loop in interactive mode where you control each iteration:

```bash
python meta_prompt_research_loop.py
```

This will:
1. Start the first iteration
2. Display Self-Critique, Improvement Plan, New Master Prompt, and Next Steps
3. Wait for your input (press ENTER to continue, type 'STOP' to end)
4. Continue to the next iteration

### Automatic Mode

Run multiple iterations automatically without manual intervention:

```bash
python meta_prompt_research_loop.py --auto --max-iterations 10
```

Options:
- `--auto`: Automatically continue iterations without user input
- `--max-iterations N`: Limit the number of iterations to N

### Examples

#### Example 1: Generate 5 iterations automatically
```bash
python meta_prompt_research_loop.py --auto --max-iterations 5
```

#### Example 2: Interactive exploration
```bash
python meta_prompt_research_loop.py
# Review each iteration carefully
# Press ENTER to continue or type STOP when satisfied
```

#### Example 3: As a Python module
```python
from meta_prompt_research_loop import MetaPromptResearchLoop

# Create loop instance
loop = MetaPromptResearchLoop()

# Run a single iteration
iteration_result = loop.run_iteration()

# Access components
print(iteration_result['master_prompt'])
print(iteration_result['self_critique'])

# Continue the loop
loop.run_loop(auto_continue=True, max_iterations=3)
```

## Output

### Console Output

Each iteration displays four sections:

1. **A. SELF-CRITIQUE**
   - Analysis of current state or previous version
   - Identification of weaknesses and gaps
   - Areas requiring enhancement

2. **B. IMPROVEMENT PLAN**
   - Structured plan for enhancements
   - Phase-by-phase improvements
   - Specific action items

3. **C. NEW VERSION OF THE MASTER PROMPT**
   - Complete, improved master prompt
   - All research phases and components
   - Actionable guidelines and protocols

4. **D. NEXT STEPS FOR FURTHER IMPROVEMENT**
   - Future priorities
   - Focus areas for next iteration
   - Long-term enhancement goals

### Saved Files

Each iteration is automatically saved to a JSON file:
- Filename format: `meta_prompt_iteration_N_YYYYMMDD_HHMMSS.json`
- Contains all four components plus metadata
- Enables version tracking and comparison

## Master Prompt Structure

Each generated master prompt includes:

### Phase 1: Literature Review & Hypothesis Generation
- Systematic literature review protocols
- Hypothesis formulation
- Indicator taxonomy

### Phase 2: Parameter Mining & Optimization
- Search strategies (grid, random, Bayesian, genetic)
- Parameter ranges and sensitivity analysis
- Computational efficiency guidelines

### Phase 3: Rigorous Back-Testing Framework
- Data requirements and quality checks
- Testing partitions (train/validation/test)
- Transaction cost modeling
- Performance metrics (Sharpe, Sortino, Calmar, etc.)

### Phase 4: Multi-Asset & Cross-Market Validation
- Asset class diversification
- Market regime analysis
- Correlation analysis

### Phase 5: Robustness Testing
- Monte Carlo simulation
- Stress testing scenarios
- Parameter and data robustness

### Phase 6: Walk-Forward Validation
- Rolling window protocols
- Performance monitoring
- Adaptive mechanisms

### Phase 7: Code Generation & Implementation
- Code quality standards
- Required components
- Version control and configuration management

### Phase 8: Risk Metrics & Monitoring
- Real-time risk tracking
- Risk limits and controls
- Performance attribution

### Phase 9: Self-Critique & Validation
- Red flag checklist
- Validation questions
- Independent verification

### Phase 10: Multi-Phase Reasoning Framework
- Decision trees
- Feedback loops
- Hypothesis testing cycles

## Advanced Features

### Bias Prevention
The system includes safeguards against:
- Data snooping bias
- Look-ahead bias
- Survivorship bias
- Overfitting/curve-fitting

### Statistical Rigor
- P-value thresholds (p < 0.05)
- Multiple testing corrections
- Confidence intervals
- Robust performance metrics

### Autonomous Agent Support
- Error handling protocols
- State management
- Resource optimization
- Continuous improvement mechanisms
- Safety controls

## Iteration Strategy

The system progressively enhances the prompt through focused iterations:

1. **Iteration 1**: Foundation building and core methodology
2. **Iteration 2**: Statistical rigor and mathematical precision
3. **Iteration 3**: Bias prevention and robustness enhancement
4. **Iteration 4**: Automation and agent-specific guidance
5. **Iteration 5**: Integration and cross-component coordination
6. **Iteration 6+**: Advanced techniques and refinements

Each iteration builds upon previous work, addressing identified weaknesses and adding depth.

## Best Practices

### For Interactive Use
1. Review each iteration carefully
2. Note specific improvements made
3. Stop when the prompt meets your requirements
4. Use saved JSON files for comparison

### For Automatic Generation
1. Start with 5-10 iterations for comprehensive coverage
2. Review the final version and select the best iteration
3. Compare early vs late iterations to see evolution
4. Use the most recent iteration for maximum depth

### For Integration
1. Import the module into your trading research pipeline
2. Use generated prompts as guidelines for validation
3. Customize parameter ranges for your specific use case
4. Adapt the framework to your asset classes

## Customization

### Modifying Focus Areas
Edit the `_get_focus_area()` method to change iteration priorities.

### Adjusting Iteration Content
Modify the `generate_*()` methods to customize:
- Self-critique depth
- Improvement plan structure
- Master prompt components
- Next steps recommendations

### Adding New Phases
Extend the master prompt template in `generate_master_prompt()` to include additional research phases.

## Troubleshooting

### Issue: Iterations not progressing
- Ensure Python 3.8+ is installed
- Check file permissions for writing JSON files

### Issue: Need to resume from specific iteration
- Load the saved JSON file
- Extract the master prompt
- Create a new loop instance and continue

### Issue: Output too verbose
- Redirect output to file: `python meta_prompt_research_loop.py > output.log`
- Review saved JSON files instead of console output

## Integration with Trading Systems

The generated master prompts can be used to:

1. **Guide Manual Research**: Follow the protocols step-by-step
2. **Build Automated Systems**: Use as specification for development
3. **Validate Existing Strategies**: Check against the framework
4. **Train Research Teams**: Share as best practices documentation
5. **Create Checklists**: Extract validation requirements

## Version History

- **v1.0**: Initial implementation with 10-phase framework
- Continuous improvement through iteration mechanism

## Support

For issues, questions, or contributions:
- Review the code comments for detailed documentation
- Examine saved iteration JSON files for examples
- Modify the source code to fit your specific needs

## License

See repository LICENSE file for details.

## Credits

Designed for rigorous, research-grade trading indicator validation with focus on:
- Statistical significance
- Robustness across market conditions
- Bias prevention
- Reproducibility
- Production readiness
