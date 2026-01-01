using System.Collections.Generic;
using System.Text;
using LinqToDB.Tools;

namespace UpsertHelperProject;

public static class UpsertHelper
{
    public static string GenerateUpsertScript(
        string tableName,
        List<string> tableFields,
        List<string> pkFields,
        List<string> excludeUpdateFields,
        bool updateChangedOnly,
        List<string> extraExcludeUpdateConditionFields = null)
    {
        StringBuilder fields = new();
        StringBuilder queryParams = new();
        StringBuilder updateMappings = new();
        StringBuilder conditions = new();
        foreach (string field in tableFields)
        {
            fields.AppendLine(@$"    ""{field}"",");
            queryParams.AppendLine(@$"    @{field},");
            if (!field.In(excludeUpdateFields))
                updateMappings.AppendLine(@$"    ""{field}"" = @{field},");
            if (updateChangedOnly && !field.In(excludeUpdateFields) && !field.In(extraExcludeUpdateConditionFields))
                conditions.AppendLine(@$"    OR NOT EXISTS(SELECT t.""{field}"" INTERSECT SELECT @{field})");
        }

        StringBuilder query = new();
        query
            .AppendLine(@$"INSERT INTO ""{tableName}"" AS t")
            .AppendLine("(")
            .AppendLine(ToStringWithRemovedLastComma(fields))
            .AppendLine(")")
            .AppendLine("VALUES")
            .AppendLine("(")
            .AppendLine(ToStringWithRemovedLastComma(queryParams))
            .AppendLine(")")
            .AppendLine(@$"ON CONFLICT (""{string.Join(", ", pkFields)}"") DO UPDATE SET")
            .AppendLine(ToStringWithRemovedLastComma(updateMappings))
            ;

        if (updateChangedOnly)
        {
            query
                .AppendLine(@$"WHERE (FALSE")
                .Append(conditions)
                .AppendLine(@$")")
                ;
        }

        return query.ToString();

        static string ToStringWithRemovedLastComma(StringBuilder sb)
        {
            string trimmedStr = sb.ToString().TrimEnd();
            return trimmedStr[..^1];
        }
    }
}
