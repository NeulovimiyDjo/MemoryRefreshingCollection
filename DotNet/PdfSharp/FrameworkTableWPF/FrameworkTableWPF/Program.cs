using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace FrameworkTableWPF
{
    class Program
    {
        static void Main(string[] args)
        {
            const bool unicode = false;
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode);

            //var files = new List<string>();
            //for (int i = 0; i < 5; i++)
            //{
            //    pdfRenderer.Document = CreateDocument(1000);
            //    pdfRenderer.RenderDocument();

            //    string file = "HelloWorld"+i.ToString()+".pdf";
            //    pdfRenderer.PdfDocument.Save(file);
            //    files.Add(file);
            //}

            //string filename = Concantenate(files.ToArray());


            // Open the output document
            PdfDocument outputDocument = new PdfDocument();

            // Iterate files
            for (int i = 0; i < 5; i++)
            {
                pdfRenderer.Document = CreateDocument(12);
                pdfRenderer.RenderDocument();

                //// Open the document to import pages from it.
                //var s = new MemoryStream();
                //pdfRenderer.PdfDocument.Save(s, false);
                //PdfDocument inputDocument = PdfReader.Open(s, PdfDocumentOpenMode.Import);


                //// Iterate pages
                //int count = inputDocument.PageCount;
                //for (int idx = 0; idx < count; idx++)
                //{
                //    // Get the page from the external document...
                //    PdfPage page = inputDocument.Pages[idx];
                //    // ...and add it to the output document.
                //    outputDocument.AddPage(page);
                //}
            }

            string filename = "HelloWorld.pdf";
            //outputDocument.Save(filename);
            pdfRenderer.PdfDocument.Save(filename);
            Process.Start("cmd.exe", "/c " + filename);
        }

        /// <summary>
        /// Imports all pages from a list of documents.
        /// </summary>
        static string Concantenate(string[] files)
        {
            // Open the output document
            PdfDocument outputDocument = new PdfDocument();

            // Iterate files
            foreach (string file in files)
            {
                // Open the document to import pages from it.
                PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                // Iterate pages
                int count = inputDocument.PageCount;
                for (int idx = 0; idx < count; idx++)
                {
                    // Get the page from the external document...
                    PdfPage page = inputDocument.Pages[idx];
                    // ...and add it to the output document.
                    outputDocument.AddPage(page);
                }
            }

            // Save the document...
            const string filename = "ConcatenatedDocument1_tempfile.pdf";
            outputDocument.Save(filename);

            return filename;
        }

        static Document CreateDocument(int count)
        {
            Document document = new Document();
            Section section = document.AddSection();

            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.LeftMargin = "1cm";
            section.PageSetup.RightMargin = "1cm";
            section.PageSetup.TopMargin = "1.5cm";
            section.PageSetup.BottomMargin = "0cm";
            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.HeaderDistance = "0.5cm";

            var p = section.Headers.Primary.AddParagraph("page 1");
            p.Format.Alignment = ParagraphAlignment.Right;




            Table table = section.AddTable();
            table.Borders.Color = Color.Empty;
            table.Borders.Width = 0.25;
            table.Rows.Alignment = RowAlignment.Center;

            Column column = table.AddColumn("6cm");
            column = table.AddColumn("4cm");
            column = table.AddColumn("4cm");
            column = table.AddColumn("4cm");
            column = table.AddColumn("4cm");
            column = table.AddColumn("5cm");


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


            table.Rows.Height = "1cm";
            Random r = new Random();
            for (int i = 0; i < count; i++)
            {
                Row row1 = table.AddRow();
                row1.VerticalAlignment = VerticalAlignment.Bottom;
                row1.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                row1.Cells[1].MergeRight = 3;

                row1.Cells[0].AddParagraph("itemNumber1" + r.Next());
                row1.Cells[1].AddParagraph("itemNumber2" + r.Next());
                row1.Cells[5].AddParagraph("0.00" + r.Next());
            }


            {
                Table table2 = section.Footers.Primary.AddTable();
                table2.Borders.Color = Color.Empty;
                table2.Borders.Width = 0;
                table2.Rows.Height = "1cm";

                table2.AddColumn("5cm").Format.Alignment = ParagraphAlignment.Left;
                table2.AddColumn("16cm").Format.Alignment = ParagraphAlignment.Left;
                table2.AddColumn("6cm").Format.Alignment = ParagraphAlignment.Left;

                Row row1 = table2.AddRow();
                row1.VerticalAlignment = VerticalAlignment.Bottom;


                row1.Cells[0].AddParagraph("bsadfsdf sdfsdf");
                row1.Cells[1].AddParagraph(r.Next().ToString());
                row1.Cells[2].AddParagraph("zzzzz xxxxxxx");

                table2.SetEdge(1, 0, 1, 1, Edge.Bottom, BorderStyle.Single, 0.75);
            }
            {
                Table table2 = section.Footers.Primary.AddTable();
                table2.Borders.Color = Color.Empty;
                table2.Borders.Width = 0;
                table2.Rows.Height = "1cm";

                table2.AddColumn("3cm").Format.Alignment = ParagraphAlignment.Left;
                table2.AddColumn("20cm").Format.Alignment = ParagraphAlignment.Left;
                table2.AddColumn("4cm").Format.Alignment = ParagraphAlignment.Left;

                Row row1 = table2.AddRow();
                row1.VerticalAlignment = VerticalAlignment.Bottom;


                row1.Cells[0].AddParagraph("summa");
                row1.Cells[1].AddParagraph(r.Next().ToString());
                row1.Cells[2].AddParagraph("rub");

                table2.SetEdge(1, 0, 1, 1, Edge.Bottom, BorderStyle.Single, 0.75);
            }


            return document;
        }
    }
}
