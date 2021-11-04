using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;


using (SpreadsheetDocument xl = SpreadsheetDocument.Open(finalContentStream, true))
{
	this.sharedStringTable = xl.WorkbookPart.SharedStringTablePart.SharedStringTable;
	this.wbp = xl.WorkbookPart;
	this.ws = (xl.WorkbookPart.WorksheetParts.FirstOrDefault()).Worksheet;
	this.mergeCells = ws.Elements<MergeCells>().First();
	this.columns = ws.Descendants<Columns>().FirstOrDefault();
	this.definedNames = xl.WorkbookPart.Workbook.Descendants<DefinedNames>().FirstOrDefault();

	if (definedNames == null)
	{
		throw new Exception("Failed to find definedNames in " + this.GetType().Name);
	}


	// do work


	CalculationChainPart calChainPart = xl.WorkbookPart.CalculationChainPart;
	xl.WorkbookPart.DeletePart(calChainPart);


	xl.WorkbookPart.Workbook.Save();
}

// Column_Indexes_Names {
private static int GetColumnIndex(Cell cell)
{
	string cellReference = cell.CellReference;
	//remove digits
	string columnReference = Regex.Replace(cellReference.ToUpper(), @"[\d]", string.Empty);

	int columnNumber = -1;
	int mulitplier = 1;

	//working from the end of the letters take the ASCII code less 64 (so A = 1, B =2...etc)
	//then multiply that number by our multiplier (which starts at 1)
	//multiply our multiplier by 26 as there are 26 letters
	foreach (char c in columnReference.ToCharArray().Reverse())
	{
		columnNumber += mulitplier * ((int)c - 64);

		mulitplier = mulitplier * 26;
	}

	//the result is zero based so return columnnumber + 1 for a 1 based answer
	//this will match Excel's COLUMN function
	return columnNumber + 1;
}


private (string, int) ParseDefineName(string text)
{
	MatchCollection matches = Regex.Matches(text, @",?'?((?:[^']|'{2})+)'?!\$?([A-Z]+)\$?(\d+)(?:\:\$?([A-Z]+)\$?(\d+))?");
	if (matches.Count == 0 || matches.Count > 1)
	{
		throw new Exception("Failed to parse definedName in " + this.GetType().Name);
	}

	Match match = matches[0];

	// Заменяем '' на ' в имени страницы
	string worksheetName = match.Groups[1].Value.Replace("''", "'");

	string left = match.Groups[2].Value;
	int top = int.Parse(match.Groups[3].Value);

	//right и bottom могут отсутствовать. В таком случае в них записывается значение Left и Bottom соответственно.
	string right = match.Groups[4].Success ? match.Groups[4].Value : left;
	int bottom = match.Groups[5].Success ? int.Parse(match.Groups[5].Value) : top;

	return (left, top);
}
// Column_Indexes_Names }


// Border_Functions {
public void SetBorder(WorkbookPart workbookPart, WorksheetPart workSheetPart, Cell cell)
{
	CellFormat cellFormat = cell.StyleIndex != null ? GetCellFormat(workbookPart, cell.StyleIndex).CloneNode(true) as CellFormat : new CellFormat();
	cellFormat.BorderId = InsertBorder(workbookPart, GenerateBorder());

	cell.StyleIndex = InsertCellFormat(workbookPart, cellFormat);
}

public CellFormat GetCellFormat(WorkbookPart workbookPart, uint styleIndex)
{
	return workbookPart.WorkbookStylesPart.Stylesheet.Elements<CellFormats>().First().Elements<CellFormat>().ElementAt((int)styleIndex);
}

public uint InsertBorder(WorkbookPart workbookPart, Border border)
{
	Borders borders = workbookPart.WorkbookStylesPart.Stylesheet.Elements<Borders>().First();
	borders.Append(border);
	return (uint)borders.Count++;
}

public uint InsertCellFormat(WorkbookPart workbookPart, CellFormat cellFormat)
{
	CellFormats cellFormats = workbookPart.WorkbookStylesPart.Stylesheet.Elements<CellFormats>().First();
	cellFormats.Append(cellFormat);
	return (uint)cellFormats.Count++;
}

public Border GenerateBorder()
{
	Border border2 = new Border();

	LeftBorder leftBorder2 = new LeftBorder() { Style = BorderStyleValues.Medium };

	//RightBorder rightBorder2 = new RightBorder() { Style = BorderStyleValues.Medium };


	//TopBorder topBorder2 = new TopBorder() { Style = BorderStyleValues.Medium };

	BottomBorder bottomBorder2 = new BottomBorder() { Style = BorderStyleValues.Medium };



	border2.Append(leftBorder2);
	//border2.Append(rightBorder2);
	//border2.Append(topBorder2);
	border2.Append(bottomBorder2);

	return border2;
}
// Border_Functions }


// Working_With_Cells {
private Cell GetCellByValue(WorksheetPart wsp, string cellValue)
{
	var sd = wsp.Worksheet.GetFirstChild<SheetData>();

	var rows = sd.Elements<Row>();
	foreach (var row in rows)
	{
		var cell = row.Elements<Cell>().FirstOrDefault(c => GetCellText(c) == cellValue);
		if (cell != null)
		{
			return cell;
		}
	}

	return null;
}


private string GetCellText(Cell cell)
{
	return cell.DataType != null
		&& cell.DataType.HasValue
		&& cell.DataType.Value == CellValues.SharedString
			? this.GetSharedString(int.Parse(cell.InnerText))
			: cell.InnerText;
}

private string GetSharedString(int id)
{
	return this.sharedStringTable.Elements<SharedStringItem>().ElementAt(id).InnerText;
}

private void ReplaceElementsInCell(Cell cell, object value, Type valueType = null)
{
	string newCellValue;

	if (valueType == null)
	{
		if (value == null)
		{
			valueType = typeof(DBNull);
		}
		else
		{
			valueType = value.GetType();
		}
	}


	if (valueType != typeof(string)
		&& valueType != typeof(DBNull)
		)
	{
		if (value == null)
		{
			newCellValue = "";
		}
		else if (valueType == typeof(DateTime))
		{
			newCellValue = ((DateTime)value).ToOADate().ToString(new NumberFormatInfo { NumberDecimalSeparator = "." });
		}
		else if ((valueType == typeof(decimal) || valueType == typeof(double) || valueType == typeof(float))
			&& value is IFormattable)
		{
			newCellValue = ((IFormattable)value).ToString(null, new NumberFormatInfo { NumberDecimalSeparator = "." });
		}
		else
		{
			newCellValue = value.ToString();
		}

		cell.DataType = null;

		if (cell.CellValue == null)
		{
			cell.CellValue = new CellValue();
		}
		cell.CellValue.Text = newCellValue;
	}
	else if (value != DBNull.Value && value != null) // Иначе заменяем значение в SharedString
	{
		cell.DataType = CellValues.InlineString;

		var inlineString = new InlineString();
		var t = new Text
		{
			Text = (string)value
		};
		inlineString.AppendChild(t);

		cell.AppendChild(inlineString);
	}
}
// Working_With_Cells }


// Copying_Worksheets {
private void FixupTableParts(WorksheetPart worksheetPart, int tableId)
{
	foreach (TableDefinitionPart tableDefPart in worksheetPart.TableDefinitionParts)
	{
		tableId++;
		tableDefPart.Table.Id = (uint)tableId;
		tableDefPart.Table.DisplayName = "CopiedTable" + tableId;
		tableDefPart.Table.Name = "CopiedTable" + tableId;
	}
}

private void CopySheet(SpreadsheetDocument mySpreadsheet, string sheetId)
{
	WorkbookPart workbookPart = mySpreadsheet.WorkbookPart;
	IEnumerable<Sheet> source = workbookPart.Workbook.Descendants<Sheet>();
	Sheet sheet = Enumerable.First<Sheet>(source, (Func<Sheet, bool>)(s => (string)s.Id == sheetId));
	string sheetWorkbookPartId = (string)sheet.Id;

	WorksheetPart sourceSheetPart = (WorksheetPart)workbookPart.GetPartById(sheetWorkbookPartId);
	SpreadsheetDocument tempSheet = SpreadsheetDocument.Create(new MemoryStream(), mySpreadsheet.DocumentType);
	WorkbookPart tempWorkbookPart = tempSheet.AddWorkbookPart();
	WorksheetPart tempWorksheetPart = tempWorkbookPart.AddPart<WorksheetPart>(sourceSheetPart);


	WorksheetPart clonedSheet = workbookPart.AddPart<WorksheetPart>(tempWorksheetPart);

	int numTableDefParts = sourceSheetPart.GetPartsCountOfType<TableDefinitionPart>();
	if (numTableDefParts != 0)
	{
		FixupTableParts(clonedSheet, numTableDefParts);
	}


	Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
	Sheet copiedSheet = new Sheet();

	copiedSheet.Name = sheet.Name + " (" + (sheets.ChildElements.Count + 1 - this.firstTableSheetIndex - this.sheetsAfterTable) + ")";
	copiedSheet.Id = workbookPart.GetIdOfPart(clonedSheet);

	sheets.InsertAfter(copiedSheet, sheets.ChildElements[sheets.ChildElements.Count - 1 - this.sheetsAfterTable]);

	for (int i = 0; i <= this.sheetsAfterTable; i++)
	{
		((Sheet)sheets.ChildElements[sheets.ChildElements.Count - 1 - i]).SheetId = (uint)(sheets.ChildElements.Count - i);
	}
}
// Copying_Worksheets }


// Moving_Rows {
private static void SetTableRowHeight(WorksheetPart wsp, uint rowIndexFirst, uint rowIndexLast)
{
	var sd = wsp.Worksheet.GetFirstChild<SheetData>();

	Row tableRow = wsp.Worksheet.Descendants<Row>().FirstOrDefault(r => r.RowIndex.Value == rowIndexFirst - 2);
	Row rowBefore = wsp.Worksheet.Descendants<Row>().FirstOrDefault(r => r.RowIndex.Value == rowIndexFirst - 1);
	double newHeight = tableRow.Height ?? wsp.Worksheet.SheetFormatProperties.DefaultRowHeight;

	for (uint i = rowIndexFirst; i <= rowIndexLast; i++)
	{
		var row = new Row { RowIndex = i };
		row.Height = newHeight;
		row.CustomHeight = true;
		sd.InsertAfter(row, rowBefore);
		rowBefore = row;
	}
}

private static void MoveRows(WorksheetPart worksheetPart, uint rowIndex, int step)
{
	UpdateRowIndexes(worksheetPart, rowIndex, step);
	UpdateMergedCellReferences(worksheetPart, rowIndex, step);
}

private static void UpdateRowIndexes(WorksheetPart worksheetPart, uint rowIndex, int step)
{
	IEnumerable<Row> rows = worksheetPart.Worksheet.Descendants<Row>().Where(r => r.RowIndex.Value >= rowIndex);

	foreach (Row row in rows)
	{
		uint newIndex = (uint)(row.RowIndex + step);
		string curRowIndex = row.RowIndex.ToString();
		string newRowIndex = newIndex.ToString();

		foreach (Cell cell in row.Elements<Cell>())
		{
			cell.CellReference = new StringValue(cell.CellReference.Value.Replace(curRowIndex, newRowIndex));
		}

		row.RowIndex = newIndex;
	}
}

private static void UpdateMergedCellReferences(WorksheetPart worksheetPart, uint rowIndex, int step)
{
	if (worksheetPart.Worksheet.Elements<MergeCells>().Count() > 0)
	{
		MergeCells mergeCells = worksheetPart.Worksheet.Elements<MergeCells>().FirstOrDefault();

		if (mergeCells != null)
		{
			List<MergeCell> mergeCellsList = mergeCells.Elements<MergeCell>()
				.Where(r => r.Reference.HasValue)
				.Where(r =>
					GetRowIndex(r.Reference.Value.Split(':').ElementAt(0)) >= Math.Min(rowIndex, rowIndex + step)
					|| GetRowIndex(r.Reference.Value.Split(':').ElementAt(1)) >= Math.Min(rowIndex, rowIndex + step))
				.ToList();


			// Удалить смердженные ячейки выше rowIndex при сдвиге вверх
			if (step < 0)
			{
				List<MergeCell> mergeCellsToDelete = mergeCellsList
			   .Where(r =>
				   GetRowIndex(r.Reference.Value.Split(':').ElementAt(0)) < rowIndex
				   || GetRowIndex(r.Reference.Value.Split(':').ElementAt(1)) < rowIndex)
			   .ToList();


				foreach (MergeCell cellToDelete in mergeCellsToDelete)
				{
					cellToDelete.Remove();
				}

				mergeCellsList = mergeCells.Elements<MergeCell>()
					.Where(r => r.Reference.HasValue)
					.Where(r =>
						GetRowIndex(r.Reference.Value.Split(':').ElementAt(0)) >= rowIndex + step
						|| GetRowIndex(r.Reference.Value.Split(':').ElementAt(1)) >= rowIndex + step)
					.ToList();
			}


			foreach (MergeCell mergeCell in mergeCellsList)
			{
				string[] cellReference = mergeCell.Reference.Value.Split(':');

				if (GetRowIndex(cellReference.ElementAt(0)) >= rowIndex)
				{
					string columnName = GetColumnName(cellReference.ElementAt(0));
					cellReference[0] = columnName + (GetRowIndex(cellReference.ElementAt(0)) + step).ToString();
				}

				if (GetRowIndex(cellReference.ElementAt(1)) >= rowIndex)
				{
					string columnName = GetColumnName(cellReference.ElementAt(1));
					cellReference[1] = columnName + (GetRowIndex(cellReference.ElementAt(1)) + step).ToString();
				}

				mergeCell.Reference = new StringValue(cellReference[0] + ":" + cellReference[1]);
			}
		}
	}
}


public static uint GetRowIndex(string cellReference)
{
	Regex regex = new Regex(@"\d+");
	Match match = regex.Match(cellReference);

	return uint.Parse(match.Value);
}

private static string GetColumnName(string cellName)
{
	Regex regex = new Regex("[A-Za-z]+");
	Match match = regex.Match(cellName);

	return match.Value;
}
// Moving_Rows }