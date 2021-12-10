﻿using Reductech.EDR.Connectors.StructuredData.Util;
using Reductech.EDR.Core.Internal.Errors;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.StructuredData;

/// <summary>
/// Extracts entities from a Concordance stream.
/// The same as FromCSV but with different default values.
/// </summary>
[Alias("ConvertConcordanceToEntity")]
public sealed class FromConcordance : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await CSVReader.ReadCSV(
            stateMonad,
            Stream,
            Delimiter,
            new StringConstant(""),
            QuoteCharacter,
            MultiValueDelimiter,
            new ErrorLocation(this),
            cancellationToken
        );

        return result;
    }

    /// <summary>
    /// Stream containing the CSV data.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> Stream { get; set; } = null!;

    /// <summary>
    /// The delimiter to use to separate fields.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("\\u0014 - DC4")]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> Delimiter { get; set; } =
        new StringConstant("\u0014");

    /// <summary>
    /// The quote character to use.
    /// Should be a single character or an empty string.
    /// If it is empty then strings cannot be quoted.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("\u00FE")]
    [SingleCharacter]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> QuoteCharacter { get; set; } =
        new StringConstant("\u00FE");

    /// <summary>
    /// The multi value delimiter character to use.
    /// Should be a single character or an empty string.
    /// If it is empty then fields cannot have multiple fields.
    /// </summary>
    [StepProperty(4)]
    [DefaultValueExplanation("|")]
    [SingleCharacter]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> MultiValueDelimiter { get; set; } =
        new StringConstant("|");

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<FromConcordance, Array<Entity>>();
}
