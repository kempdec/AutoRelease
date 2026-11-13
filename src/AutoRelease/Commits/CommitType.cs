namespace KempDec.AutoRelease.Commits;

/// <summary>
/// Representa um tipo de mensagem de commit e o título do grupo dele nas notas.
/// </summary>
/// <param name="Key">A chave do tipo, como <c>feat</c>, em minúsculas.</param>
/// <param name="Label">O título do grupo nas notas, como <c>Novidades</c>.</param>
/// <param name="Order">A posição do grupo nas notas: os menores vêm primeiro.</param>
internal sealed record CommitType(string Key, string Label, int Order);
