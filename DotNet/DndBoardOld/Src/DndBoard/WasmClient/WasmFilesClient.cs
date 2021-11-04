using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DndBoard.Shared.Models;
using DndBoard.ClientCommon;

namespace DndBoard.WasmClient
{
    public class WasmFilesClient : IFilesClient
    {
        private readonly HttpClient _httpClient;

        public WasmFilesClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task PostFilesAsJsonAsync(UploadedFiles value)
        {
            await _httpClient.PostAsJsonAsync("/api/fileupload/PostFiles", value);
        }

        public async Task DeleteFilesAsync(string boardId, string fileId)
        {
            await _httpClient.PostAsync(
                $"/api/FileUpload/DeleteFile/{boardId}/{fileId}",
                null
            );
        }

        public async Task<List<string>> GetIconsModelsListAsJsonAsync(string boardId)
        {
            return await _httpClient.GetFromJsonAsync<List<string>>(
                $"/api/images/getfilesids/{boardId}"
            );
        }
    }
}
