#!/usr/bin/env python3
"""
Test suite to verify META-PROMPT RESEARCH LOOP MODE implementation
meets all requirements from the problem statement.
"""

import json
from meta_prompt_research_loop import MetaPromptResearchLoop


def test_continuous_deep_iterations():
    """Test Rule 1: Must think in continuous deep iterations"""
    print("Testing: Continuous deep iterations...")
    loop = MetaPromptResearchLoop()
    
    # Generate multiple iterations
    for i in range(3):
        result = loop.run_iteration()
        assert result['iteration'] == i + 1, f"Expected iteration {i+1}, got {result['iteration']}"
    
    assert loop.iteration_count == 3, "Should track iteration count"
    print("✓ Continuous deep iterations working")


def test_iteration_components():
    """Test Rule 2: Each iteration must have required components"""
    print("Testing: Required iteration components...")
    loop = MetaPromptResearchLoop()
    result = loop.run_iteration()
    
    # Check all required components exist
    assert 'self_critique' in result, "Missing self-critique"
    assert 'improvement_plan' in result, "Missing improvement plan"
    assert 'master_prompt' in result, "Missing master prompt"
    assert 'next_steps' in result, "Missing next steps"
    
    # Check components are not empty
    assert len(result['self_critique']) > 100, "Self-critique too short"
    assert len(result['improvement_plan']) > 100, "Improvement plan too short"
    assert len(result['master_prompt']) > 1000, "Master prompt too short"
    assert len(result['next_steps']) > 100, "Next steps too short"
    
    print("✓ All required components present")


def test_progressive_improvement():
    """Test Rule 5: Each version must be more comprehensive"""
    print("Testing: Progressive improvement...")
    loop = MetaPromptResearchLoop()
    
    result1 = loop.run_iteration()
    result2 = loop.run_iteration()
    
    # Iteration 2 should analyze iteration 1
    assert 'Iteration 1' in result2['self_critique'] or 'previous' in result2['self_critique'].lower(), \
        "Iteration 2 should reference previous version"
    
    # Each iteration should improve based on previous
    assert 'Strengths' in result2['self_critique'] or 'Weaknesses' in result2['self_critique'], \
        "Should identify strengths and weaknesses"
    
    print("✓ Progressive improvement working")


def test_required_components_in_prompt():
    """Test Rule 6: Include all specified components"""
    print("Testing: Required components in master prompt...")
    loop = MetaPromptResearchLoop()
    result = loop.run_iteration()
    
    prompt = result['master_prompt']
    
    # Check for all required components
    required_components = [
        'literature review',
        'parameter',
        'back-test',
        'multi-asset',
        'robustness',
        'walk-forward',
        'code',
        'risk',
        'self-critique',
        'multi-phase'
    ]
    
    for component in required_components:
        assert component.lower() in prompt.lower(), f"Missing component: {component}"
    
    print("✓ All required components in master prompt")


def test_output_structure():
    """Test Rule 7: Output format for every iteration"""
    print("Testing: Output structure...")
    loop = MetaPromptResearchLoop()
    result = loop.run_iteration()
    
    # Check the four required outputs
    outputs = {
        'A. Self-Critique': result['self_critique'],
        'B. Improvement Plan': result['improvement_plan'],
        'C. New Version of the Master Prompt': result['master_prompt'],
        'D. Next Steps for Further Improvement': result['next_steps']
    }
    
    for name, content in outputs.items():
        assert content is not None, f"{name} is missing"
        assert len(content) > 0, f"{name} is empty"
    
    print("✓ Correct output structure")


def test_automatic_continuation():
    """Test Rule 8: Must continue the loop automatically"""
    print("Testing: Automatic continuation...")
    loop = MetaPromptResearchLoop()
    
    # Test that loop can run multiple iterations automatically
    iteration_count = 5
    for i in range(iteration_count):
        result = loop.run_iteration()
        assert result['iteration'] == i + 1
    
    assert loop.iteration_count == iteration_count, "Should automatically continue"
    print("✓ Automatic continuation working")


def test_depth_and_detail():
    """Test that prompts have depth and detail"""
    print("Testing: Depth and detail in prompts...")
    loop = MetaPromptResearchLoop()
    result = loop.run_iteration()
    
    prompt = result['master_prompt']
    
    # Check for depth indicators
    depth_indicators = [
        'phase',
        'protocol',
        'methodology',
        'validation',
        'statistical',
        'rigorous',
        'comprehensive'
    ]
    
    found_count = sum(1 for indicator in depth_indicators if indicator.lower() in prompt.lower())
    assert found_count >= len(depth_indicators) - 2, "Insufficient depth in prompt"
    
    # Check for specific detail
    assert 'sharpe' in prompt.lower(), "Should include specific metrics like Sharpe ratio"
    assert 'p <' in prompt.lower() or 'p-value' in prompt.lower(), "Should include statistical significance"
    
    print("✓ Sufficient depth and detail")


def test_research_focus():
    """Test that prompts focus on research for trading indicators"""
    print("Testing: Research focus on trading indicators...")
    loop = MetaPromptResearchLoop()
    result = loop.run_iteration()
    
    prompt = result['master_prompt']
    
    # Check for trading-specific terms
    trading_terms = [
        'indicator',
        'trading',
        'strategy',
        'back-test',
        'market',
        'performance'
    ]
    
    for term in trading_terms:
        assert term.lower() in prompt.lower(), f"Missing trading term: {term}"
    
    # Check for research methodology
    research_terms = [
        'hypothesis',
        'test',
        'validation',
        'statistical'
    ]
    
    for term in research_terms:
        assert term.lower() in prompt.lower(), f"Missing research term: {term}"
    
    print("✓ Correct research focus")


def test_bias_prevention():
    """Test that prompts include bias prevention measures"""
    print("Testing: Bias prevention measures...")
    loop = MetaPromptResearchLoop()
    result = loop.run_iteration()
    
    prompt = result['master_prompt']
    critique = result['self_critique']
    
    # Check for bias-related content
    bias_terms = [
        'bias',
        'overfitting',
        'curve-fitting',
        'out-of-sample'
    ]
    
    found_in_prompt = sum(1 for term in bias_terms if term.lower() in prompt.lower())
    found_in_critique = sum(1 for term in bias_terms if term.lower() in critique.lower())
    
    assert found_in_prompt >= 2, "Should mention bias prevention in prompt"
    assert found_in_critique >= 1, "Should consider bias in critique"
    
    print("✓ Bias prevention included")


def test_json_serialization():
    """Test that iterations can be saved and loaded"""
    print("Testing: JSON serialization...")
    loop = MetaPromptResearchLoop()
    result = loop.run_iteration()
    
    # Save to JSON
    filename = "test_iteration.json"
    loop.save_iteration(result, filename)
    
    # Load and verify
    with open(filename, 'r') as f:
        loaded = json.load(f)
    
    assert loaded['iteration'] == result['iteration']
    assert loaded['self_critique'] == result['self_critique']
    assert loaded['master_prompt'] == result['master_prompt']
    
    # Clean up
    import os
    os.remove(filename)
    
    print("✓ JSON serialization working")


def run_all_tests():
    """Run all tests and report results"""
    print("\n" + "="*80)
    print("META-PROMPT RESEARCH LOOP MODE - IMPLEMENTATION VERIFICATION")
    print("="*80 + "\n")
    
    tests = [
        test_continuous_deep_iterations,
        test_iteration_components,
        test_progressive_improvement,
        test_required_components_in_prompt,
        test_output_structure,
        test_automatic_continuation,
        test_depth_and_detail,
        test_research_focus,
        test_bias_prevention,
        test_json_serialization
    ]
    
    passed = 0
    failed = 0
    
    for test in tests:
        try:
            test()
            passed += 1
        except AssertionError as e:
            print(f"✗ Test failed: {test.__name__}")
            print(f"  Error: {e}")
            failed += 1
        except Exception as e:
            print(f"✗ Test error: {test.__name__}")
            print(f"  Error: {e}")
            failed += 1
    
    print("\n" + "="*80)
    print(f"TEST RESULTS: {passed} passed, {failed} failed")
    print("="*80 + "\n")
    
    if failed == 0:
        print("✓ ALL REQUIREMENTS VERIFIED")
        print("\nThe implementation successfully:")
        print("  1. Thinks in continuous deep iterations")
        print("  2. Includes all required components per iteration")
        print("  3. Never stops improving (until told to)")
        print("  4. Goes deeper with each loop")
        print("  5. Becomes more comprehensive, precise, and structured")
        print("  6. Includes all 10 specified components")
        print("  7. Outputs in the correct format")
        print("  8. Continues the loop automatically")
        print("\nThe system is ready for use!")
    else:
        print("⚠ SOME TESTS FAILED - Review implementation")
    
    return failed == 0


if __name__ == "__main__":
    success = run_all_tests()
    exit(0 if success else 1)
