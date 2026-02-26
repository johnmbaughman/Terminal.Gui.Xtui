using System.Reflection;
using Terminal.Gui.Xtui.Generator;

namespace Terminal.Gui.Xtui.Tests;

/// <summary>
/// Tests to enforce that public generator APIs return string types only (not Roslyn Syntax nodes).
/// This ensures data safety and preserves public contracts per Constitution Principle II and FR-008 clarification.
/// Task T031.
/// </summary>
public class PublicGeneratorApiTests
{
    [Fact]
    public void PublicGeneratorMethods_ShouldReturnStringType_NotSyntaxNodes()
    {
        // Arrange: Get all generator types from the Terminal.Gui.Xtui assembly
        var assembly = typeof(XtuiGenerator).Assembly;
        var generatorTypes = assembly.GetTypes()
            .Where(t => t.Namespace != null &&
                       (t.Namespace.Contains("Generators") || t.Name.Contains("Generator")) &&
                       t.IsClass &&
                       !t.IsAbstract &&
                       t.DeclaringType == null) // ignore nested helper types
            .ToList();

        Assert.NotEmpty(generatorTypes); // Ensure we found generator classes

        // Act & Assert: Check public methods return string, not Syntax types
        var violations = new List<string>();

        foreach (var generatorType in generatorTypes)
        {
            var publicMethods = generatorType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.DeclaringType == generatorType) // Only methods declared in this type
                .ToList();

            foreach (var method in publicMethods)
            {
                var returnType = method.ReturnType;

                // Check if return type is a Roslyn Syntax type
                if (returnType.FullName != null &&
                    (returnType.FullName.Contains("Microsoft.CodeAnalysis.CSharp.Syntax") ||
                     returnType.FullName.Contains("Microsoft.CodeAnalysis.SyntaxNode")))
                {
                    violations.Add($"{generatorType.Name}.{method.Name}() returns {returnType.Name} (Roslyn Syntax type)");
                }

                // Public API methods should return string for generated code
                // Allow void, bool, and collection types for non-generation methods
                // Flag non-string generation methods
                if (method.Name.Contains("Generate") &&
                    returnType != typeof(string) &&
                    returnType != typeof(void))
                {
                    // If it's not a string, it might be a Syntax type (which we flag above)
                    // or another type we should document
                    if (returnType.FullName == null || !returnType.FullName.Contains("Microsoft.CodeAnalysis"))
                    {
                        violations.Add($"{generatorType.Name}.{method.Name}() returns {returnType.Name} (expected string for generation methods)");
                    }
                }
            }
        }

        // Assert: No violations found
        if (violations.Any())
        {
            var violationMessage = string.Join(Environment.NewLine, violations);
            Assert.Fail($"Public generator methods must return string type, not Roslyn Syntax nodes:{Environment.NewLine}{violationMessage}");
        }
    }

    [Fact]
    public void GenerateClass_Methods_MustReturnString()
    {
        // Arrange: Find all types with GenerateClass method
        var assembly = typeof(XtuiGenerator).Assembly;
        var typesWithGenerateClass = assembly.GetTypes()
            .Where(t => t.GetMethod("GenerateClass", BindingFlags.Public | BindingFlags.Instance) != null)
            .ToList();

        Assert.NotEmpty(typesWithGenerateClass); // Ensure we found types with GenerateClass

        // Act & Assert: All GenerateClass methods must return string
        foreach (var type in typesWithGenerateClass)
        {
            var method = type.GetMethod("GenerateClass", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);

            Assert.True(
                method.ReturnType == typeof(string),
                $"{type.Name}.GenerateClass() must return string, but returns {method.ReturnType.Name}"
            );
        }
    }
}
