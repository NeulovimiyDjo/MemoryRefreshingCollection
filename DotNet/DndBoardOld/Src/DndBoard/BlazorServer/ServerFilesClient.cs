using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DndBoard.ClientCommon;
using DndBoard.ServerCommon.Controllers;
using DndBoard.Shared.Models;

namespace DndBoard.BlazorServer
{
    public class ServerFilesClient : IFilesClient
    {
        private readonly FileUploadController _fileUploadController;
        private readonly ImagesController _imagesController;

        public ServerFilesClient(
            FileUploadController fileUploadController,
            ImagesController imagesController)
        {
            _fileUploadController = fileUploadController;
            _imagesController = imagesController;
        }


        public async Task PostFilesAsJsonAsync(UploadedFiles value)
        {
            await _fileUploadController.PostFiles(value);
        }

        public async Task DeleteFilesAsync(string boardId, string fileId)
        {
            await _fileUploadController.DeleteFile(boardId, fileId);
        }

        public async Task<List<string>> GetIconsModelsListAsJsonAsync(string boardId)
        {
            return (await _imagesController.GetFilesIds(boardId)).ToList();
        }
    }
}
