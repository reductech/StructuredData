﻿using Reductech.Sequence.Core.Steps;

namespace Reductech.Sequence.Connectors.StructuredData.Tests;

public partial class ToCSVTests : StepTestBase<ToCSV, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Write Simple CSV",
                new Log
                {
                    Value = new ToCSV
                    {
                        Entities = Array(
                            Entity.Create(("Foo", "Hello"),   ("Bar", "World")),
                            Entity.Create(("Foo", "Hello 2"), ("Bar", "World 2"))
                        )
                    }
                },
                Unit.Default,
                $"Foo,Bar{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2{Environment.NewLine}"
            );

            yield return new StepCase(
                "Write Simple CSV with tab delimiter",
                new Log
                {
                    Value = new ToCSV
                    {
                        Entities = Array(
                            Entity.Create(("Foo", "Hello"),   ("Bar", "World")),
                            Entity.Create(("Foo", "Hello 2"), ("Bar", "World 2"))
                        ),
                        Delimiter = Constant("\t")
                    }
                },
                Unit.Default,
                $"Foo\tBar{Environment.NewLine}Hello\tWorld{Environment.NewLine}Hello 2\tWorld 2{Environment.NewLine}"
            );
        }
    }
}
