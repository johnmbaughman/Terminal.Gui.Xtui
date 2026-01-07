namespace Terminal.Gui.Xtui.Generator.Helpers;

/// <summary>
/// Factory interface for creating generator instances based on element names.
/// Provides a unified abstraction for both GeneratorsXml and Generators implementations.
/// </summary>
internal interface IGeneratorFactory
{
    /// <summary>
    /// Gets a generator instance for the specified element name.
    /// </summary>
    /// <param name="elementName">The name of the element to generate code for</param>
    /// <param name="elementName">The name of the element to generate code for</param>
    /// <returns>A generator instance capable of generating code for the specified element</returns>
    Terminal.Gui.Xtui.Generator.Helpers.Generator GetGenerator(string elementName);
}
