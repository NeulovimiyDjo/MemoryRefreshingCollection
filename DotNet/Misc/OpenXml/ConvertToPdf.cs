using NetOffice.ExcelApi;
using Excel = NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;

using (Excel.Application app = new Excel.Application())
{
	try
	{
		app.Visible = false;
		app.DisplayAlerts = false;

		using (Excel.Workbook wkb = app.Workbooks.Open(tmpPath))
		{
			var worksheets = wkb.Worksheets.OfType<Worksheet>();
			foreach (Worksheet ws in worksheets)
			{
				ws.PageSetup.Orientation = XlPageOrientation.xlLandscape;
				ws.PageSetup.Zoom = false;
				ws.PageSetup.FitToPagesWide = 1;
				ws.PageSetup.FitToPagesTall = 1;

				//ws.PageSetup.PrintArea = "a1-e50";
			}

			wkb.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, pdfPath,
				XlFixedFormatQuality.xlQualityStandard, true, false);
		}
	}
	finally
	{
		app.Quit();
	}
}