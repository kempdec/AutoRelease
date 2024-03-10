namespace KempDec.AutoRelease.SubCommands.Inputs;

/// <summary>
/// Fornece abstração para as entradas globais.
/// </summary>
internal abstract class GlobalInputs : IGlobalInputs
{
    /// <inheritdoc/>
    public string Token { get; init; } = null!;

    /// <inheritdoc/>
    public (string Owner, string Name) Repo { get; init; }

    /// <inheritdoc/>
    public string? Branch { get; init; } = null!;
}
