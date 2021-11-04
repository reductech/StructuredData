﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.StructuredData.Util.IDX
{

/// <summary>
/// Contains methods for serializing entities to IDX
/// </summary>
public static class IDXSerializer
{
    /// <summary>
    /// Convert this entity to an IDX Document.
    /// Will fail if the entity contains nested entities or doubly nested lists.
    /// </summary>
    public static Result<string, IErrorBuilder> TryConvertToIDXDocument(this Entity entity)
    {
        var stringBuilder = new StringBuilder();

        var r = TryAppendValues(entity, stringBuilder);

        if (r.IsFailure)
            return r.ConvertFailure<string>();

        stringBuilder.AppendLine($"#{DREEndDoc}");
        stringBuilder.AppendLine($"#{DREEndDataReference}");

        return stringBuilder.ToString().Trim();
    }

    /// <summary>
    /// Convert this entity to IDX Data.
    /// Will fail if the entity contains nested entities or doubly nested lists.
    /// </summary>
    public static Result<string, IErrorBuilder> TryConvertToIDXData(this Entity entity)
    {
        var stringBuilder = new StringBuilder();

        var r = TryAppendValues(entity, stringBuilder);

        if (r.IsFailure)
            return r.ConvertFailure<string>();

        stringBuilder.AppendLine($"#{DREEndData}");

        return stringBuilder.ToString().Trim();
    }

    private static Result<Unit, IErrorBuilder> TryAppendValues(
        Entity entity,
        StringBuilder stringBuilder)
    {
        foreach (var dreField in OrderedDREFields)
        {
            var value = entity.TryGetValue(dreField.Name);

            if (value.HasValue)
            {
                var v = TryAppendValue(
                    stringBuilder,
                    value.Value,
                    dreField
                );

                if (v.IsFailure)
                    return v;
            }
            else if (dreField.Mandatory)
                return ErrorCode.SchemaViolation.ToErrorBuilder(
                    dreField.Name,
                    entity
                );
        }

        foreach (var entityProperty in entity)
        {
            if (!DREFieldsSet.Contains(entityProperty.Name))
            {
                var fieldValue = new SpecialField(
                    $"{DREField} {entityProperty.Name}",
                    StringHandling.Quote,
                    false,
                    true,
                    true
                );

                var v = TryAppendValue(
                    stringBuilder,
                    entityProperty.Value,
                    fieldValue
                );

                if (v.IsFailure)
                    return v;
            }
        }

        return Unit.Default;
    }

    private static Result<Unit, IErrorBuilder> TryAppendValue(
        StringBuilder sb,
        EntityValue entityValue,
        SpecialField field)
    {
        if (entityValue is EntityValue.Null)
            return Unit.Default;

        void AppendString(string s1) //TODO escape value
        {
            if (s1.Contains('\n') || field.StringHandling == StringHandling.Paragraph)
            {
                sb.AppendLine("#" + field.Name);
                sb.AppendLine(s1);
            }
            else
            {
                var maybeQuotedString =
                    field.StringHandling == StringHandling.Quote ? $"\"{s1}\"" : s1;

                AppendField(maybeQuotedString);
            }
        }

        void AppendField(string s2)
        {
            var equalsMaybe = field.UseEquals ? "=" : "";
            var line        = $"#{field.Name}{equalsMaybe} {s2}";
            sb.AppendLine(line);
        }

        if (entityValue is EntityValue.String s)
        {
            AppendString(s.Value);
            return Unit.Default;
        }

        if (entityValue is EntityValue.Integer i)
        {
            AppendField(i.Value.ToString());
            return Unit.Default;
        }

        if (entityValue is EntityValue.Double d)
        {
            AppendField(d.Value.ToString(Constants.DoubleFormat));
            return Unit.Default;
        }

        if (entityValue is EntityValue.Boolean b)
        {
            AppendField(b.Value.ToString());
            return Unit.Default;
        }

        if (entityValue is EntityValue.EnumerationValue ev)
        {
            AppendField(ev.Value.ToString());
            return Unit.Default;
        }

        if (entityValue is EntityValue.DateTime date)
        {
            AppendField(date.Value.ToString("yyyy/MM/dd"));
            return Unit.Default;
        }

        if (entityValue is EntityValue.NestedEntity)
            return ErrorCode.CannotConvertNestedEntity.ToErrorBuilder("IDX");

        if (entityValue is EntityValue.NestedList list)
        {
            if (!field.AllowList)
                return ErrorCode.CannotConvertNestedList.ToErrorBuilder("IDX");

            for (var j = 0; j < list.Value.Count; j++)
            {
                var member       = list.Value[j];
                var newFieldName = $"{field.Name}{j + 1}";
                TryAppendValue(sb, member, field with { AllowList = false, Name = newFieldName });
            }
        }

        return Unit.Default;
    }

    private enum StringHandling
    {
        Quote,
        Paragraph,
        InlineUnquoted
    }

    private record SpecialField(
        string Name,
        StringHandling StringHandling,
        bool Mandatory,
        bool AllowList,
        bool UseEquals);

    private const string DREEndData = "DREENDDATA";

    private const string DREField = "DREFIELD";
    private const string DREEndDoc = "DREENDDOC";
    private const string DREEndDataReference = "DREENDDATAREFERENCE";

    private static readonly IReadOnlyList<SpecialField> OrderedDREFields = new List<SpecialField>()
    {
        // ReSharper disable StringLiteralTypo
        new("DREREFERENCE", StringHandling.InlineUnquoted, true, false, false),
        new("DREDATE", StringHandling.InlineUnquoted, false, false, false),
        new("DRETITLE", StringHandling.Paragraph, false, false, false),
        new("DRECONTENT", StringHandling.Paragraph, false, false, false),
        new("DREDBNAME", StringHandling.InlineUnquoted, false, false, false)
        // ReSharper restore StringLiteralTypo
    };

    private static readonly IReadOnlyCollection<string> DREFieldsSet =
        OrderedDREFields.Select(x => x.Name).ToHashSet();
}

}
