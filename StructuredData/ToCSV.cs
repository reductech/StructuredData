﻿using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.StructuredData.Util;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.StructuredData
{

/// <summary>
/// Write entities to a stream in CSV format.
/// The same as ToConcordance but with different default values.
/// </summary>
[Alias("ConvertEntityToCSV")]
public sealed class ToCSV : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await CSVWriter.WriteCSV(
            stateMonad,
            Entities,
            Delimiter,
            Encoding,
            QuoteCharacter,
            AlwaysQuote,
            MultiValueDelimiter,
            DateTimeFormat,
            new ErrorLocation(this),
            cancellationToken
        );

        return result;
    }

    /// <summary>
    /// The entities to write.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> Entities { get; set; } = null!;

    /// <summary>
    /// How the stream should be encoded.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("UTF8 no BOM")]
    public IStep<EncodingEnum> Encoding { get; set; } =
        new EnumConstant<EncodingEnum>(EncodingEnum.UTF8);

    /// <summary>
    /// The delimiter to use to separate fields.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation(",")]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> Delimiter { get; set; } = new StringConstant(",");

    /// <summary>
    /// The quote character to use.
    /// Should be a single character or an empty string.
    /// If it is empty then strings cannot be quoted.
    /// </summary>
    [StepProperty(4)]
    [DefaultValueExplanation("\"")]
    [SingleCharacter]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> QuoteCharacter { get; set; } =
        new StringConstant("\"");

    /// <summary>
    /// Whether to always quote all fields and headers.
    /// </summary>
    [StepProperty(5)]
    [DefaultValueExplanation("false")]
    public IStep<bool> AlwaysQuote { get; set; } = new BoolConstant(false);

    /// <summary>
    /// The multi value delimiter character to use.
    /// Should be a single character or an empty string.
    /// If it is empty then fields cannot have multiple fields.
    /// </summary>
    [StepProperty(6)]
    [DefaultValueExplanation("")]
    [SingleCharacter]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> MultiValueDelimiter { get; set; } =
        new StringConstant("|");

    /// <summary>
    /// The format to use for DateTime fields.
    /// </summary>
    [StepProperty(7)]
    [DefaultValueExplanation("O - ISO 8601 compliant - e.g. 2009-06-15T13:45:30.0000000-07:00")]
    [Example("yyyy/MM/dd HH:mm:ss")]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> DateTimeFormat { get; set; } =
        new StringConstant("O");

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<ToCSV, StringStream>();
}

}
