using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TestWeb.Controllers
{
    [Controller]
    [Route("SrvTwo")]
    public class SrvTwoController : Controller
    {
        private static string connectionString;
        private const string ContentFlag = "filename*=UTF-8''";

        private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
        .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
        .AddJsonFile(
             path: "appsettings.json",
             optional: false,
             reloadOnChange: true)
        .Build();

        public SrvTwoController()
        {
            connectionString = _configuration.GetSection("AppSettings").GetValue<string>("ConnectionString");
        }

        [HttpGet("files/{fileID::guid}")]
        [Produces(typeof(File))]
        public async Task<IActionResult> GetFile([FromRoute] Guid fileID)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
"SELECT file_name,file_content FROM _testweb_files_srvTwo where file_id=@id", conn)
            {
                Parameters =
                {
                    new() { ParameterName = "@id", Value = fileID },
                }
            };
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                string fileName = reader.GetString(0);
                byte[] fileContent = reader.GetFieldValue<byte[]>(1);
                return File(fileContent, "application/octet-stream", fileName);
            }
            else
            {
                return NotFound();
            }
        }

        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        [HttpPost("files")]
        [Produces(typeof(SrvTwoResponse))]
        [Consumes("application/octet-stream")]
        public async Task<ActionResult<SrvTwoResponse>> SaveFile(
            [FromRoute] bool archive = true,
            [FromRoute] string folder = null)
        {
            SrvTwoResponse srvTwoResponse = new();
            srvTwoResponse.Uuid = Guid.NewGuid();
            if (Request.Headers.TryGetValue("Content-Disposition", out var val))
            {
                string fileNameRaw = val.First();
                string fileName = fileNameRaw[(fileNameRaw.IndexOf(ContentFlag) + ContentFlag.Length)..];
                srvTwoResponse.Name = Uri.UnescapeDataString(fileName);
            }
            srvTwoResponse.UploadDate = DateTime.UtcNow;
            byte[] fileContent;
            using (var memoryStream = new MemoryStream())
            {
                Request.BodyReader.AsStream().CopyTo(memoryStream);
                fileContent = memoryStream.ToArray();
            }

            if (fileContent.Length == 0)
                return BadRequest($"File can not be empty");

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
"INSERT INTO _testweb_files_srvTwo(file_id,file_name,file_content) VALUES(@id,@name,@content)", conn)
            {
                Parameters =
                {
                    new() { ParameterName = "@id", Value = srvTwoResponse.Uuid },
                    new() { ParameterName = "@name", Value = srvTwoResponse.Name },
                    new() { ParameterName = "@content", Value = fileContent },
                }
            };
            await cmd.ExecuteNonQueryAsync();

            return srvTwoResponse;
        }

        [HttpDelete("files/{fileID::guid}")]
        public async Task<IActionResult> DeleteFile([FromRoute] Guid fileID)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
"DELETE FROM _testweb_files_srvTwo where file_id=@id", conn)
            {
                Parameters =
                {
                    new() { ParameterName = "@id", Value = fileID },
                }
            };

            int count = await cmd.ExecuteNonQueryAsync();
            if (count == 1)
                return NoContent();
            else
                return NotFound();
        }

        public class SrvTwoResponse
        {
            public Guid Uuid { get; set; }
            public string Name { get; set; }
            public DateTime UploadDate { get; set; }
        }
    }
}
