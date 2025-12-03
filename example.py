#!/usr/bin/env python3
"""
Example usage of META-PROMPT RESEARCH LOOP MODE

This script demonstrates how to use the MetaPromptResearchLoop class
for generating and improving trading research prompts.
"""

from meta_prompt_research_loop import MetaPromptResearchLoop


def example_single_iteration():
    """Example: Generate a single iteration"""
    print("="*80)
    print("EXAMPLE 1: Single Iteration Generation")
    print("="*80)
    
    loop = MetaPromptResearchLoop()
    result = loop.run_iteration()
    
    print(f"\nGenerated iteration {result['iteration']}")
    print(f"Timestamp: {result['timestamp']}")
    print(f"\nMaster prompt preview (first 500 chars):")
    print(result['master_prompt'][:500] + "...")
    print("\n" + "="*80)


def example_multiple_iterations():
    """Example: Generate multiple iterations programmatically"""
    print("\n" + "="*80)
    print("EXAMPLE 2: Multiple Iterations (3 cycles)")
    print("="*80)
    
    loop = MetaPromptResearchLoop()
    
    for i in range(3):
        result = loop.run_iteration()
        print(f"\nIteration {result['iteration']} completed")
        
        # Show improvement focus
        if 'focus on' in result['next_steps']:
            focus_start = result['next_steps'].find('focus on:')
            if focus_start != -1:
                focus_text = result['next_steps'][focus_start:focus_start+100]
                print(f"Next focus: {focus_text}...")
    
    print(f"\nTotal iterations completed: {loop.iteration_count}")
    print("="*80)


def example_extract_specific_component():
    """Example: Extract specific components from iterations"""
    print("\n" + "="*80)
    print("EXAMPLE 3: Extract Self-Critique from Iterations")
    print("="*80)
    
    loop = MetaPromptResearchLoop()
    
    # Generate two iterations
    result1 = loop.run_iteration()
    result2 = loop.run_iteration()
    
    print("\nIteration 1 Self-Critique (first 300 chars):")
    print(result1['self_critique'][:300] + "...")
    
    print("\n" + "-"*80)
    
    print("\nIteration 2 Self-Critique (first 300 chars):")
    print(result2['self_critique'][:300] + "...")
    
    print("\n" + "="*80)


def example_save_and_analyze():
    """Example: Save iterations and analyze them"""
    print("\n" + "="*80)
    print("EXAMPLE 4: Save and Analyze Iterations")
    print("="*80)
    
    loop = MetaPromptResearchLoop()
    
    # Generate and save iterations
    results = []
    for i in range(2):
        result = loop.run_iteration()
        results.append(result)
        loop.save_iteration(result, f"example_iteration_{i+1}.json")
    
    # Analyze progression
    print("\nAnalyzing prompt evolution:")
    for i, result in enumerate(results, 1):
        prompt_length = len(result['master_prompt'])
        critique_length = len(result['self_critique'])
        plan_length = len(result['improvement_plan'])
        
        print(f"\nIteration {i}:")
        print(f"  - Master Prompt: {prompt_length} characters")
        print(f"  - Self-Critique: {critique_length} characters")
        print(f"  - Improvement Plan: {plan_length} characters")
    
    print("\nFiles saved: example_iteration_1.json, example_iteration_2.json")
    print("="*80)


if __name__ == "__main__":
    # Run all examples
    print("\n")
    print("╔" + "="*78 + "╗")
    print("║" + " "*20 + "META-PROMPT RESEARCH LOOP EXAMPLES" + " "*24 + "║")
    print("╚" + "="*78 + "╝")
    
    example_single_iteration()
    example_multiple_iterations()
    example_extract_specific_component()
    example_save_and_analyze()
    
    print("\n" + "="*80)
    print("ALL EXAMPLES COMPLETED SUCCESSFULLY")
    print("="*80)
    print("\nTo run the full interactive loop, use:")
    print("  python meta_prompt_research_loop.py")
    print("\nTo run automatically with max iterations:")
    print("  python meta_prompt_research_loop.py --auto --max-iterations 5")
    print("="*80 + "\n")
