﻿using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Extensions;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção dos tipos das mensagens de commit.
/// </summary>
internal class TypesOption : Option<List<CommitMessageType>>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="TypesOption"/>.
    /// </summary>
    public TypesOption() : base(name: "--types",
        description: "Os tipos das mensagens de commit.",
        parseArgument: result =>
        {
            var types = new List<CommitMessageType>();

            for (byte i = 0; i < result.Tokens.Count; i++)
            {
                Token token = result.Tokens[i];
                (string? key, string? label) = token.Value.Split('=');

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(label))
                {
                    result.ErrorMessage = """
                    Não foi possível definir os tipos das mensagens de commit.

                    Os tipos das mensagens de commit devem estar no formato:

                    type1=value1 type2=value2
                    """;

                    return default!;
                }

                types.Add(new CommitMessageType
                {
                    Key = key,
                    Label = label,
                    Ordering = i
                });
            }

            return types;
        }) => AllowMultipleArgumentsPerToken = true;
}
