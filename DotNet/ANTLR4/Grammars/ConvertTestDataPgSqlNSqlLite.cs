using System.Text.RegularExpressions;
using System.Text;

const string RepoDir = "../../../../..";
string originalTestDataRepos = $"{RepoDir}/../grammars_test_data";
string postgreSQLTestDataDir = $"{originalTestDataRepos}/postgres-REL_15_STABLE/src/test/regress/sql";
string sqliteTestDataDir = $"{originalTestDataRepos}/sqlite-REL_3_40_1/test";
string postgreSQLOutDir = $"{RepoDir}/out/examples.new";
string sqliteOutDir = $"{RepoDir}/out/examples.new";

Action convertPostgreSQL = () =>
{
    if (!Directory.Exists(postgreSQLOutDir))
        Directory.CreateDirectory(postgreSQLOutDir);

    foreach (string filePath in Directory.EnumerateFiles(postgreSQLTestDataDir, "*.sql", SearchOption.TopDirectoryOnly))
    {
        string fileName = Path.GetFileName(filePath);
        if (fileName == "psql.sql")
            continue;

        string input = File.ReadAllText(filePath);
        input = input.Replace("\r\n", "\n");

        input = Regex.Replace(input, @"^\s*\\crosstabview", "\\crosstabview", RegexOptions.Multiline);
        if (fileName == "transactions.sql")
            input = Regex.Replace(input, @"\\;", ";");

        input = Regex.Replace(input, @"(?<!--|--\s|\n) *\\gset", "; --\\gset");
        input = Regex.Replace(input, @"(?<!--|--\s|\n) *\\gdesc", "; --\\gdesc");
        input = Regex.Replace(input, @"(?<!--|--\s|\n) *\\gexec", "; --\\gexec");
        input = Regex.Replace(input, @"(?<!--|--\s|\n) *\\crosstabview", "; --\\crosstabview");
        input = Regex.Replace(input, @"\s*\n\\gset", ";\n--\\gset");
        input = Regex.Replace(input, @"\s*\n\\gdesc", ";\n--\\gdesc");
        input = Regex.Replace(input, @"\s*\n\\gexec", ";\n--\\gexec");
        input = Regex.Replace(input, @"\s*\n\\crosstabview", ";\n--\\crosstabview");
        input = Regex.Replace(input, @";.*\n\\\.", ";\n--\\.", RegexOptions.Singleline);
        input = Regex.Replace(input, @"^\\", "--\\", RegexOptions.Multiline);
        input = Regex.Replace(input, @" :'regresslib'", " 'regresslib'");
        input = Regex.Replace(input, @" :'filename'", " 'filename'");
        input = Regex.Replace(input, @"\n:show_data;", "\n--:show_data;");
        input = Regex.Replace(input, @"\n:init_range_parted;", "\n--:init_range_parted;");

        string res = input.ToString().Trim().Replace("\r\n", "\n").Replace("\n", "\r\n");
        if (!string.IsNullOrEmpty(res))
            File.WriteAllText($"{postgreSQLOutDir}/{fileName}", res);
    }
};

Action convertSQLite = () =>
{
    if (!Directory.Exists(sqliteOutDir))
        Directory.CreateDirectory(sqliteOutDir);

    Dictionary<string, StringBuilder> filesData = new();
    foreach (string filePath in Directory
        .EnumerateFiles(sqliteTestDataDir, "*.test", SearchOption.TopDirectoryOnly)
        .OrderBy(x => x))
    {
        string originalFileNameWE = Path.GetFileNameWithoutExtension(filePath);
        string fileNameWE = originalFileNameWE;
        if (fileNameWE.StartsWith("fts1", StringComparison.InvariantCultureIgnoreCase))
            fileNameWE = "fts1";
        else if (fileNameWE.StartsWith("fts2", StringComparison.InvariantCultureIgnoreCase))
            fileNameWE = "fts2";
        else if (fileNameWE.StartsWith("fts3", StringComparison.InvariantCultureIgnoreCase))
            fileNameWE = "fts3";
        else if (fileNameWE.StartsWith("fts4", StringComparison.InvariantCultureIgnoreCase))
            fileNameWE = "fts4";
        else if (fileNameWE.StartsWith("fts", StringComparison.InvariantCultureIgnoreCase))
            fileNameWE = "fts";
        else if (fileNameWE.StartsWith("tkt-", StringComparison.InvariantCultureIgnoreCase))
            fileNameWE = "tkt-hash";
        else if (fileNameWE.StartsWith("tkt", StringComparison.InvariantCultureIgnoreCase))
            fileNameWE = "tkt-num";
        else if (char.IsDigit(fileNameWE.Last()) || filesData.ContainsKey(fileNameWE.Substring(0, fileNameWE.Length - 1)))
            fileNameWE = fileNameWE.Substring(0, fileNameWE.Length - 1);

        string input = File.ReadAllText(filePath);
        if (!filesData.ContainsKey(fileNameWE))
            filesData.Add(fileNameWE, new StringBuilder());
        StringBuilder sb = filesData[fileNameWE];

        MatchCollection matches = Regex.Matches(
            input,
@"^\s*[^\n]*(?: eval\s*|catchsql\s*| execsql[^\n${]*|execsql_test[^\n${]*|catchsql_test[^\n${]*){[\r\n]*(?<ws> *)(?<sql>[^$]*?)\s*}",
            RegexOptions.Multiline | RegexOptions.Singleline);
        if (matches.Count > 0)
            sb.AppendLine($"-- ==={originalFileNameWE}.test===");

        foreach (Match match in matches)
        {
            string testCaseSql = match.Groups["sql"].Value;
            string ws = match.Groups["ws"].Value;
            testCaseSql = (ws + testCaseSql).Replace("\r\n", "\n");
            int indent = ws.Length;
            foreach (string line in testCaseSql.Split("\n"))
                sb.AppendLine(TrimLeftIfWs(line, indent));
            sb.AppendLine();
        }
    }

    foreach (string fileNameWE in filesData.Keys)
    {
        string res = filesData[fileNameWE]
            .ToString().Trim()
            .Replace("\r\n", "\n").Replace("\n", "\r\n");

        if (!string.IsNullOrEmpty(res))
            File.WriteAllText($"{sqliteOutDir}/{fileNameWE}.sql", res);
    }
};

static string TrimLeftIfWs(string line, int indent)
{
    if (line.Length > indent && string.IsNullOrWhiteSpace(line.Substring(0, indent)))
        return line.Substring(indent, line.Length - indent);
    else
        return line;
}

convertPostgreSQL();
convertSQLite();
