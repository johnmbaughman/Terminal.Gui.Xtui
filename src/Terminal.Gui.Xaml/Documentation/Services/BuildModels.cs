// <copyright file="BuildModels.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Terminal.Gui.Xaml.Documentation.Models;

namespace Terminal.Gui.Xaml.Documentation.Services;

/// <summary>
/// Represents a build target request.
/// </summary>
public class BuildTargetRequest
{
    /// <summary>
    /// Gets or sets the target name.
    /// </summary>
    public string Target { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the project file path.
    /// </summary>
    public string ProjectFile { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the build properties.
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Represents a build target response.
/// </summary>
public class BuildTargetResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the build was successful.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Gets or sets the exit code.
    /// </summary>
    public int ExitCode { get; set; }
    
    /// <summary>
    /// Gets or sets the output.
    /// </summary>
    public string Output { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the errors.
    /// </summary>
    public string Errors { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the build duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Gets or sets the validation results.
    /// </summary>
    public ValidateDocumentationResponse? ValidationResults { get; set; }
    
    /// <summary>
    /// Gets or sets the list of files that were deleted.
    /// </summary>
    public IList<string> FilesDeleted { get; set; } = new List<string>();
}

/// <summary>
/// Represents a build target definition.
/// </summary>
public class BuildTargetDefinition
{
    /// <summary>
    /// Gets or sets the target name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the command.
    /// </summary>
    public string Command { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the arguments.
    /// </summary>
    public string Arguments { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the working directory.
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the properties.
    /// </summary>
    public Dictionary<string, BuildPropertyDefinition> Properties { get; set; } = new();
}

/// <summary>
/// Represents a build property definition.
/// </summary>
public class BuildPropertyDefinition
{
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether the property is required.
    /// </summary>
    public bool Required { get; set; }
}

/// <summary>
/// Represents build progress information.
/// </summary>
public class BuildProgress
{
    /// <summary>
    /// Gets or sets the build phase.
    /// </summary>
    public BuildPhase Phase { get; set; }
    
    /// <summary>
    /// Gets or sets the progress message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the percentage complete.
    /// </summary>
    public double PercentComplete { get; set; }
}

/// <summary>
/// Represents the different phases of a build.
/// </summary>
public enum BuildPhase
{
    /// <summary>
    /// Build is starting.
    /// </summary>
    Starting,
    
    /// <summary>
    /// Build is preparing.
    /// </summary>
    Preparing,
    
    /// <summary>
    /// Build is building.
    /// </summary>
    Building,
    
    /// <summary>
    /// Build is validating.
    /// </summary>
    Validating,
    
    /// <summary>
    /// Build is completing.
    /// </summary>
    Completing,
    
    /// <summary>
    /// Build has completed.
    /// </summary>
    Completed,
    
    /// <summary>
    /// Build has failed.
    /// </summary>
    Failed
}

// ValidateDocumentationResponse moved to Documentation.Models.GenerationAndValidationModels