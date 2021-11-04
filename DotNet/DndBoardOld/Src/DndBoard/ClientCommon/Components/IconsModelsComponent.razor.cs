using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DndBoard.ClientCommon.BaseComponents;
using DndBoard.ClientCommon.Helpers;
using DndBoard.ClientCommon.Models;
using DndBoard.ClientCommon.Store;
using DndBoard.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace DndBoard.ClientCommon.Components
{
    public partial class IconsModelsComponent : CanvasBaseComponent
    {
        private string _boardId;

        [Inject]
        private IFilesClient _Client { get; set; }
        [Inject]
        private CanvasMapRenderer _canvasMapRenderer { get; set; }
        [Inject]
        private AppState _appState { get; set; }


        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            int maxAllowedFiles = 777;

            List<UploadedFile> files = new();
            foreach (IBrowserFile imageFile in e.GetMultipleFiles(maxAllowedFiles))
            {
                UploadedFile uploadedFile = await ConvertToUploadedFile(imageFile);
                files.Add(uploadedFile);
            }

            await SendFilesToServer(files);
        }

        private static async Task<UploadedFile> ConvertToUploadedFile(IBrowserFile imageFile)
        {
            string format = "image/png";

            IBrowserFile resizedImageFile = await imageFile
                .RequestImageFileAsync(format, 100, 100);

            Stream stream = resizedImageFile.OpenReadStream();
            MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            stream.Close();

            UploadedFile uploadedFile = new();
            uploadedFile.FileName = resizedImageFile.Name;
            uploadedFile.FileContent = ms.ToArray();
            ms.Close();

            return uploadedFile;
        }

        private async Task SendFilesToServer(List<UploadedFile> files)
        {
            UploadedFiles uploadedFiles = new();
            uploadedFiles.BoardId = _boardId;
            uploadedFiles.Files = files.ToArray();

            await _Client.PostFilesAsJsonAsync(uploadedFiles);
        }

        private async Task OnRightClick(MouseEventArgs mouseEventArgs)
        {
            Coords coords = await GetCanvasCoordinatesAsync(mouseEventArgs);
            DndIconElem clickedIcon = GetClickedIcon(coords);
            if (clickedIcon is null)
                return;

            await _Client.DeleteFilesAsync(_boardId, clickedIcon.InstanceId);
        }

        private async Task OnClick(MouseEventArgs mouseEventArgs)
        {
            Coords coords = await GetCanvasCoordinatesAsync(mouseEventArgs);
            DndIconElem clickedIcon = GetClickedIcon(coords);
            if (clickedIcon is null)
                return;
 
            CoordsChangeData coordsChangeData = new CoordsChangeData
            {
                InstanceId = Guid.NewGuid().ToString(),
                Coords = new Coords { X = 10, Y = 10 },
                ModelId = clickedIcon.ModelId,
            };
            string coordsChangeDataJson = JsonSerializer.Serialize(coordsChangeData);
            await _appState.ChatHubManager.SendCoordsAsync(coordsChangeDataJson);
            await _appState.InvokeFilesRefsChanged();
        }

        private DndIconElem GetClickedIcon(Coords coords)
        {
            foreach (DndIconElem icon in _appState.IconsModels)
            {
                if (coords.X >= icon.Coords.X && coords.X <= icon.Coords.X + 100
                    && coords.Y >= icon.Coords.Y && coords.Y <= icon.Coords.Y + 100)
                {
                    return icon;
                }
            }

            return null;
        }

        protected override void OnInitialized()
        {
            _appState.BoardIdChanged += OnBoardIdChanged;
            _appState.ChatHubManager.SetNofifyIconsModelsUpdateHandler(OnIconsModelsUpdated);
        }

        private void OnIconsModelsUpdated(string boardId)
        {
            _ = RefreshIconsModelsAsync();
            async Task RefreshIconsModelsAsync()
            {
                await ReloadIconsModels();
                await Redraw();
                await _appState.ChatHubManager.RequestAllCoords();
            }
        }

        private async Task Redraw()
        {
            await _canvasMapRenderer.RedrawIconsByCoords(Canvas, _appState.IconsModels);
        }

        private async Task OnBoardIdChanged(string boardId)
        {
            _boardId = boardId;
            OnIconsModelsUpdated(_boardId);
        }

        private async Task ReloadIconsModels()
        {
            List<string> modelsIds = await _Client.GetIconsModelsListAsJsonAsync(_boardId);

            _appState.IconsInstances = new(); // Old image refs become invalid, so recreate.
            _appState.IconsModels = new(); // Otherwise existing refs don't get updated.
            StateHasChanged();

            _appState.IconsModels = modelsIds.Select(id => new DndIconElem { ModelId = id }).ToList();
            for (int i = 0; i < _appState.IconsModels.Count; i++)
                _appState.IconsModels[i].Coords = new Coords { X = 50, Y = 50 + i * 110 };

            StateHasChanged();
            await Task.Delay(300); // Wait for images to get downloaded. Use loaded event?
        }
    }
}
