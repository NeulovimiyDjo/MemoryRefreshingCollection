using Microsoft.VisualBasic.FileIO;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CsvToSql
{
    public class CsvDataScriptCreator
    {
        public void Create(string sourcePath, string destPath, string schemePath)
        {
            DataTable dataTable = ReadCsvData(sourcePath);
            FileSchemeService fileSchemeService = TableInfoReader.GetFileSchemeSerivce(schemePath);
            string script = CreateInsertScript(dataTable, fileSchemeService);
            File.WriteAllText(destPath, script);
        }


        private static DataTable ReadCsvData(string sourcePath)
        {
            TextFieldParser csvReader = new(sourcePath);
            csvReader.SetDelimiters(new string[] { ";" });
            csvReader.HasFieldsEnclosedInQuotes = true;

            DataTable dataTable = new DataTable();
            string[] colFields = csvReader.ReadFields();
            foreach (string column in colFields)
            {
                DataColumn dataColumn = new DataColumn(column);
                dataColumn.AllowDBNull = true;
                dataTable.Columns.Add(dataColumn);
            }

            while (!csvReader.EndOfData)
                dataTable.Rows.Add(csvReader.ReadFields());
				
			//DataRow dataRow = dataTable.NewRow();
            //dataRow["Col1"] = "asdf";
            //dataTable.Rows.Add(dataRow);

            dataTable.TableName = "Instances";

            dataTable.TableName = Path.GetFileNameWithoutExtension(sourcePath);
            return dataTable;
        }

        private static string CreateInsertScript(DataTable dataTable)
        {
            StringBuilder sb = new();
            foreach (DataRow row in dataTable.Rows)
            {
                sb.Append($"\nINSERT INTO [{dataTable.TableName}]\n(");
                foreach (DataColumn col in dataTable.Columns)
                    sb.Append($"\n\t[{col.ColumnName}],");
                sb.RemoveLastChar();

                sb.Append($"\n)\nVALUES\n(");
                foreach (DataColumn col in dataTable.Columns)
                {
                    string columnValue = row.Field<string>(col);
                    DbType dbType = GetColumnDbType(col.ColumnName);
                    sb.Append($"\n\t{Quote(columnValue, dbType)},");
                }
                sb.RemoveLastChar();
                sb.Append($"\n);\n");
            }
            return sb.ToString();
        }


        private static string Quote(string value, DbType dbType)
        {
            if (value == "NULL")
                return "NULL";

            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.Guid:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                    return $"'{Escape(value)}'";
                case DbType.Decimal:
                case DbType.Double:
                    return $"'{Escape(value.Replace(",", "."))}'";
                case DbType.String:
                case DbType.StringFixedLength:
                    return $"N'{Escape(value)}'";
                case DbType.Boolean:
                    return value.ToLower() switch
                    {
                        "false" => "0",
                        "true" => "1",
                        "0" => "0",
                        "1" => "1",
                        _ => throw new Exception($"Invalid boolean value {value}"),
                    };
                case DbType.Byte:
                case DbType.SByte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                    return long.Parse(value).ToString();
                case DbType.UInt64:
                    return ulong.Parse(value).ToString();
                case DbType.Binary:
                    return ToBinary(value);
                case DbType.Currency:
                case DbType.Object:
                case DbType.Single:
                case DbType.Time:
                case DbType.VarNumeric:
                case DbType.Xml:
                case DbType.DateTimeOffset:
                default:
                    throw new Exception($"Invalid DbType: {dbType}");
            }
        }

        private static string Escape(string usString)
        {
            if (usString == null)
                return null;
            // Escape ' with ''
            return Regex.Replace(usString, @"[\']", @"'$0");
        }
    }
}