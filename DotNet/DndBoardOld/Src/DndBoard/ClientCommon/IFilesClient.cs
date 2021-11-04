using System.Collections.Generic;
using System.Threading.Tasks;
using DndBoard.Shared.Models;

namespace DndBoard.ClientCommon
{
    public interface IFilesClient
    {
        public Task PostFilesAsJsonAsync(UploadedFiles value);
        public Task DeleteFilesAsync(string boardId, string fileId);
        Task<List<string>> GetIconsModelsListAsJsonAsync(string boardId);
    }
}
