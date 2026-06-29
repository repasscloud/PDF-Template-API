using System.Text.Json;
using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class TableBlockRenderer : IBlockRenderer
{
    public string Type => "table";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var columns = block.Columns ?? [];

        if (columns.Count == 0)
            throw new InvalidOperationException("Table block requires at least one column.");

        container.Table(table =>
        {
            table.ColumnsDefinition(definition =>
            {
                foreach (var column in columns)
                {
                    if (column.Width is > 0)
                        definition.ConstantColumn(column.Width.Value);
                    else
                        definition.RelativeColumn();
                }
            });

            if (block.ShowHeader != false)
            {
                table.Header(header =>
                {
                    foreach (var column in columns)
                    {
                        var cell = header.Cell()
                            .BorderBottom(1)
                            .Padding(4);

                        cell = column.Align?.ToLowerInvariant() switch
                        {
                            "right" => cell.AlignRight(),
                            "center" => cell.AlignCenter(),
                            _ => cell
                        };

                        cell.Text(column.Header).SemiBold();
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(block.DataPath))
            {
                RenderRowsFromDataPath(table, block, columns, context);
                return;
            }

            RenderStaticRows(table, block, context);
        });
    }

    private static void RenderRowsFromDataPath(
        TableDescriptor table,
        PdfBlockDefinition block,
        List<PdfTableColumnDefinition> columns,
        PdfRenderContext context)
    {
        if (!context.Tokens.TryResolveJsonPath(
            context.RootData,
            block.DataPath!,
            out var rowsElement))
        {
            throw new InvalidOperationException(
                $"Table data path not found: {block.DataPath}");
        }

        if (rowsElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                $"Table data path must resolve to an array: {block.DataPath}");
        }

        foreach (var rowElement in rowsElement.EnumerateArray())
        {
            foreach (var column in columns)
            {
                var value = context.Tokens.Apply(
                    column.Value,
                    context.RootData,
                    rowElement);

                RenderCell(table, value, column.Align);
            }
        }
    }

    private static void RenderStaticRows(
        TableDescriptor table,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var rows = block.Rows ?? [];

        foreach (var row in rows)
        {
            foreach (var cell in row)
            {
                var value = context.Tokens.Apply(
                    cell,
                    context.RootData);

                RenderCell(table, value, null);
            }
        }
    }

    private static void RenderCell(
        TableDescriptor table,
        string value,
        string? align)
    {
        var cell = table.Cell()
            .BorderBottom(0.5f)
            .Padding(4);

        cell = align?.ToLowerInvariant() switch
        {
            "right" => cell.AlignRight(),
            "center" => cell.AlignCenter(),
            _ => cell
        };

        cell.Text(value);
    }
}
