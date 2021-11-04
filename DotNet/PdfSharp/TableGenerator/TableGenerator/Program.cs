using System;
using System.Diagnostics;
using System.Text;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using PdfSharp.Pdf;

namespace TableGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            Document document = CreateDocument();

            const bool unicode = false;
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode)
            {
                Document = document
            };

            pdfRenderer.RenderDocument();

            const string filename = "HelloWorld.pdf";
            pdfRenderer.PdfDocument.Save(filename);
            Process.Start("cmd.exe", "/c " + filename);
        }

        static Document CreateDocument()
        {
            Document document = new Document();
            Section section = document.AddSection();

            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.LeftMargin = 0;
            section.PageSetup.RightMargin = 0;

            Table table = section.AddTable();
            table.Borders.Color = Color.Empty;
            table.Borders.Width = 0.25;
            table.Rows.Alignment = RowAlignment.Center;


            Column column = table.AddColumn("5cm");
            column = table.AddColumn("2.5cm");
            column = table.AddColumn("3cm");
            column = table.AddColumn("3.5cm");
            column = table.AddColumn("2cm");
            column = table.AddColumn("4cm");


            Row row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Font.Bold = true;
            row.Cells[0].AddParagraph("Item");
            row.Cells[0].Format.Font.Bold = false;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Bottom;
            row.Cells[0].MergeDown = 1;
            row.Cells[1].AddParagraph("Title and Author");
            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[1].MergeRight = 3;
            row.Cells[5].AddParagraph("Extended Price");
            row.Cells[5].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[5].VerticalAlignment = VerticalAlignment.Bottom;
            row.Cells[5].MergeDown = 1;

            row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Font.Bold = true;
            row.Shading.Color = Color.Empty;
            row.Cells[1].AddParagraph("Quantity");
            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[2].AddParagraph("Unit Price");
            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[3].AddParagraph("Discount (%)");
            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[4].AddParagraph("Taxable");
            row.Cells[4].Format.Alignment = ParagraphAlignment.Left;

            table.SetEdge(0, 0, 6, 2, Edge.Box, BorderStyle.Single, 0.75, Color.Empty);


            for (int i = 0; i < 3200; i++)
            {
                // Each item fills two rows
                Row row1 = table.AddRow();
                Row row2 = table.AddRow();
                row1.TopPadding = 1.5;
                row1.Cells[0].VerticalAlignment = VerticalAlignment.Center;
                row1.Cells[0].MergeDown = 1;
                row1.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                row1.Cells[1].MergeRight = 3;
                row1.Cells[5].MergeDown = 1;

                row1.Cells[0].AddParagraph("itemNumber");
                Paragraph paragraph = row1.Cells[1].AddParagraph();
                paragraph.AddFormattedText("title", TextFormat.Bold);
                paragraph.AddFormattedText(" by ", TextFormat.Italic);
                paragraph.AddText("author");
                row2.Cells[1].AddParagraph("quantity");
                row2.Cells[2].AddParagraph("0.00");
                row2.Cells[3].AddParagraph("0.0");
                row2.Cells[4].AddParagraph();
                row2.Cells[5].AddParagraph("0.00");
                row1.Cells[5].AddParagraph("0.00");
                row1.Cells[5].VerticalAlignment = VerticalAlignment.Bottom;

                table.SetEdge(0, table.Rows.Count - 2, 6, 2, Edge.Box, BorderStyle.Single, 0.75);
            }


            return document;
        }
    }
}
