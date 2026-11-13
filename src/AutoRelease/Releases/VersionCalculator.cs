using Semver;

namespace KempDec.AutoRelease.Releases;

/// <summary>
/// Classe com métodos auxiliares para calcular a próxima versão a partir dos commits.
/// </summary>
internal static class VersionCalculator
{
    /// <summary>
    /// Calcula a próxima versão.
    /// </summary>
    /// <remarks>
    /// Sem versão anterior, vale a primeira versão. Os commits ignorados não contam. Um commit que quebra a
    /// compatibilidade sobe a <c>major</c>; um do tipo das novidades sobe a <c>minor</c>; o resto sobe a <c>patch</c>.
    /// Depois de uma pre-release (como <c>2.0.0-beta.1</c>), a próxima versão é a estável dela (<c>2.0.0</c>), salvo
    /// quando os commits pedem um salto maior que o que ela já deu.
    /// </remarks>
    /// <param name="previous">A versão anterior, ou <see langword="null"/> na primeira release.</param>
    /// <param name="commits">Os commits desde a versão anterior.</param>
    /// <param name="rules">As regras de leitura dos commits.</param>
    /// <param name="firstVersion">A primeira versão do projeto.</param>
    /// <returns>A próxima versão.</returns>
    public static SemVersion Next(SemVersion? previous, IEnumerable<ReleaseCommit> commits, ReleaseRules rules,
        SemVersion firstVersion)
    {
        if (previous is null)
        {
            return firstVersion;
        }

        ReleaseBump bump = GetBump(commits, rules);
        var stable = new SemVersion(previous.Major, previous.Minor, previous.Patch);

        if (previous.IsPrerelease && bump <= GetPrereleaseBump(previous))
        {
            return stable;
        }

        return bump switch
        {
            ReleaseBump.Major => new SemVersion(stable.Major + 1, 0, 0),
            ReleaseBump.Minor => new SemVersion(stable.Major, stable.Minor + 1, 0),
            _ => new SemVersion(stable.Major, stable.Minor, stable.Patch + 1)
        };
    }

    /// <summary>
    /// Obtém o salto que os commits pedem.
    /// </summary>
    /// <param name="commits">Os commits.</param>
    /// <param name="rules">As regras de leitura dos commits.</param>
    /// <returns>O salto.</returns>
    public static ReleaseBump GetBump(IEnumerable<ReleaseCommit> commits, ReleaseRules rules)
    {
        ReleaseBump bump = ReleaseBump.Patch;

        foreach (ReleaseCommit commit in commits.Where(commit => !rules.IsIgnored(commit.Message)))
        {
            if (commit.Message.IsBreakingChange)
            {
                return ReleaseBump.Major;
            }

            if (rules.Types.Resolve(commit.Message) == rules.Types.MinorType)
            {
                bump = ReleaseBump.Minor;
            }
        }

        return bump;
    }

    /// <summary>
    /// Obtém o salto que uma pre-release já deu em relação à versão estável anterior a ela.
    /// </summary>
    /// <param name="prerelease">A pre-release.</param>
    /// <returns>O salto: <c>major</c> em <c>2.0.0-beta</c>, <c>minor</c> em <c>2.1.0-beta</c> e <c>patch</c> em
    /// <c>2.1.1-beta</c>.</returns>
    private static ReleaseBump GetPrereleaseBump(SemVersion prerelease)
    {
        if (!prerelease.Patch.IsZero)
        {
            return ReleaseBump.Patch;
        }

        return prerelease.Minor.IsZero ? ReleaseBump.Major : ReleaseBump.Minor;
    }
}

/// <summary>
/// Representa o salto de versão que os commits pedem.
/// </summary>
internal enum ReleaseBump
{
    /// <summary>
    /// Sobe a versão <c>patch</c>.
    /// </summary>
    Patch,

    /// <summary>
    /// Sobe a versão <c>minor</c>.
    /// </summary>
    Minor,

    /// <summary>
    /// Sobe a versão <c>major</c>.
    /// </summary>
    Major
}
