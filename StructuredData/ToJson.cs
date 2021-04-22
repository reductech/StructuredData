﻿using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.StructuredData
{

/// <summary>
/// Writes an entity to a stream in JSON format
/// </summary>
public sealed class ToJson : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entity = await Entity.Run(stateMonad, cancellationToken);

        if (entity.IsFailure)
            return entity.ConvertFailure<StringStream>();

        var formatResult = await FormatOutput.Run(stateMonad, cancellationToken);

        if (formatResult.IsFailure)
            return formatResult.ConvertFailure<StringStream>();

        var formatting = formatResult.Value ? Formatting.Indented : Formatting.None;

        var jsonString = JsonConvert.SerializeObject(
            entity.Value,
            formatting,
            EntityJsonConverter.Instance
        );

        return new StringStream(jsonString);
    }

    /// <summary>
    /// The entity to write.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <summary>
    /// Whether to indent to the Json output
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("true")]
    public IStep<bool> FormatOutput { get; set; } = new BoolConstant(true);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<ToJson, StringStream>();
}

}
