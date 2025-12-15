#!/usr/bin/env python3

import os
import json
import tree_sitter_c_sharp as tscsharp
from tree_sitter import Language, Parser

# Language setup
CS_LANGUAGE = Language(tscsharp.language())
parser = Parser(CS_LANGUAGE)

def extract_methods_from_file(file_path):
    """Extract all method definitions from a C# file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception as e:
        print(f"Error reading {file_path}: {e}")
        return []
    
    try:
        tree = parser.parse(bytes(content, "utf8"))
    except Exception as e:
        print(f"Error parsing {file_path}: {e}")
        return []
    
    methods = []
    
    def traverse(node):
        if node.type == 'method_declaration':
            # Extract method info
            method_name = None
            method_modifier = None
            return_type = None
            parameters = []
            
            # Find method name
            for child in node.children:
                if child.type == 'identifier':
                    method_name = content[child.start_byte:child.end_byte]
                    break
            
            # Extract method details
            method_text = content[node.start_byte:node.end_byte]
            start_line = content[:node.start_byte].count('\n') + 1
            end_line = content[:node.end_byte].count('\n') + 1
            
            # Extract modifiers and return type from method signature
            method_signature = content[node.start_byte:node.end_byte].split('\n')[0]
            
            methods.append({
                'name': method_name,
                'signature': method_signature.strip(),
                'start_line': start_line,
                'end_line': end_line,
                'text': method_text,
                'file_path': file_path
            })
        
        for child in node.children:
            traverse(child)
    
    traverse(tree.root_node)
    return methods

def main():
    cs_files = [
        r"c:\Users\Isuru\code\tax-calc-app\ApiTestClient.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Console\Program.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Tests.Unit\Services\CacheServiceTests.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Tests.Unit\Services\TaxCalculationServiceTests.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Tests.Unit\Services\UserTaxServiceTests.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.TestClient\TaxCalculationEngine.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.StandaloneApi\TaxApiServer.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.StandaloneApi\Program.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.TestClient\Program.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.TestClient\Models.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Services\Services\UserTaxService.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Services\Services\TaxCalculationService.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Services\Services\SimpleLogger.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Services\Services\CacheService.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Services\Interfaces\IUserTaxService.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Services\Interfaces\ITaxCalculationService.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Services\Interfaces\ILogger.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Services\Interfaces\ICacheService.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\TaxCalculationRequest.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\TaxOffset.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\TaxLevy.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\TaxCalculationResult.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\TaxBracketCalculation.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\TaxBracket.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\User.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\OffsetCalculation.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\MonthlyIncomeSummary.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\LevyCalculation.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\UserMonthlyIncome.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Core\Models\UserAnnualTaxSummary.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Data\Repositories\TaxBracketRepository.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Data\Repositories\UserRepository.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Data\Repositories\UserIncomeRepository.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Data\Repositories\SqlConnectionFactory.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Data\Interfaces\IUserRepository.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Data\Interfaces\IUserIncomeRepository.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Data\Interfaces\ITaxBracketRepository.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Data\Interfaces\IConnectionFactory.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Api\Global.asax.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Api\Controllers\TaxController.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Api\App_Start\WebApiConfig.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Api\App_Start\AutofacConfig.cs",
        r"c:\Users\Isuru\code\tax-calc-app\TaxCalculator.Api\Controllers\HealthController.cs"
    ]
    
    all_methods = []
    
    for file_path in cs_files:
        if os.path.exists(file_path):
            print(f"Processing: {file_path}")
            methods = extract_methods_from_file(file_path)
            all_methods.extend(methods)
        else:
            print(f"File not found: {file_path}")
    
    # Save results to JSON
    with open(r'c:\Users\Isuru\code\tax-calc-app\extracted_methods.json', 'w', encoding='utf-8') as f:
        json.dump(all_methods, f, indent=2, ensure_ascii=False)
    
    print(f"\nExtracted {len(all_methods)} methods from {len(cs_files)} files")
    print("Results saved to extracted_methods.json")

if __name__ == "__main__":
    main()
