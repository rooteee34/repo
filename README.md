# META-PROMPT RESEARCH LOOP MODE

An advanced system for iteratively creating and optimizing research-grade master prompts for identifying and validating trading indicators, parameter settings, and technical analysis strategies.

## Overview

This repository implements a continuous deep iteration loop that:
- Critically analyzes and improves prompts for trading research
- Generates progressively superior versions through self-critique
- Includes comprehensive validation frameworks
- Provides production-ready research protocols

## Quick Start

```bash
# Interactive mode (manual control)
python meta_prompt_research_loop.py

# Automatic mode (5 iterations)
python meta_prompt_research_loop.py --auto --max-iterations 5
```

## Features

- **Continuous Iteration**: Automatically improves prompts through deep analysis
- **Self-Critique**: Identifies weaknesses and gaps in each version
- **10-Phase Framework**: Comprehensive research methodology
  - Literature Review
  - Parameter Mining
  - Back-testing
  - Multi-Asset Testing
  - Robustness Analysis
  - Walk-Forward Validation
  - Code Generation
  - Risk Metrics
  - Self-Critique
  - Multi-Phase Reasoning
- **Bias Prevention**: Safeguards against overfitting and data snooping
- **Statistical Rigor**: P-values, confidence intervals, multiple testing corrections
- **Autonomous Agent Ready**: Built for long-term automated operation

## Documentation

- [Usage Guide](USAGE.md) - Detailed usage instructions and examples
- See inline code documentation for technical details

## Output

Each iteration produces:
1. **Self-Critique** - Analysis of current state
2. **Improvement Plan** - Structured enhancement plan
3. **Master Prompt** - New optimized version
4. **Next Steps** - Future improvement priorities

All iterations are saved to JSON files for version tracking.

## Requirements

- Python 3.8+
- No external dependencies (standard library only)

## Use Cases

- Trading strategy research and validation
- Algorithmic trading system development
- Risk management framework design
- Quantitative research methodology
- Technical analysis validation

## License

See LICENSE file for details.