using System.Text.Json;
using System.Threading.Tasks;
using DndBoard.ClientCommon.BaseComponents;
using DndBoard.ClientCommon.Helpers;
using DndBoard.ClientCommon.Models;
using DndBoard.ClientCommon.Store;
using DndBoard.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace DndBoard.ClientCommon.Components
{
    public partial class IconsInstancesComponent : CanvasBaseComponent
    {
        private DndIconElem _clickedIcon;
        [Inject]
        private AppState _appState { get; set; }
        [Inject]
        private CanvasMapRenderer _canvasMapRenderer { get; set; }
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }

        private bool _initialized = false;
        protected override async Task OnInitializedAsync()
        {
            if (_initialized)
                return;
            else
                _initialized = true;

            _appState.ChatHubManager.SetCoordsReceivedHandler(CoordsReceivedHandler);
            _appState.ChatHubManager.SetIconInstanceRemovedHandler(IconInstanceRemovedHandler);

            await _jsRuntime.InvokeAsync<object>(
                "initIconsInstancesComponent", DotNetObjectReference.Create(this)
            );
        }

        
        private void IconInstanceRemovedHandler(string iconInstanceId)
        {
            _appState.IconsInstances.RemoveAll(icon => icon.InstanceId == iconInstanceId);
        }

        private void CoordsReceivedHandler(string coordsChangeDataJson)
        {
            CoordsChangeData coordsChangeData = JsonSerializer
                .Deserialize<CoordsChangeData>(coordsChangeDataJson);

            if (!_appState.IconsInstances.Exists(img => img.InstanceId == coordsChangeData.InstanceId)
                && _appState.IconsModels.Exists(x => x.ModelId == coordsChangeData.ModelId))
            {
                _appState.IconsInstances.Add(new DndIconElem
                {
                    InstanceId = coordsChangeData.InstanceId,
                    Coords = coordsChangeData.Coords,
                    ModelId = coordsChangeData.ModelId,
                    Ref = _appState.IconsModels.Find(x => x.ModelId == coordsChangeData.ModelId).Ref,
                });
            }

            if (_appState.IconsInstances.Exists(img => img.InstanceId == coordsChangeData.InstanceId))
            {
                _appState.IconsInstances.Find(img => img.InstanceId == coordsChangeData.InstanceId)
                    .Coords = coordsChangeData.Coords;
            }
        }

        [JSInvokable]
        public async Task Redraw()
        {
            if (_appState.IconsInstances is null)
                return;

            await _canvasMapRenderer.RedrawIconsByCoordsJS(_jsRuntime, _appState.IconsInstances);
        }

        private async Task OnMouseMoveAsync(MouseEventArgs mouseEventArgs)
        {
            DndIconElem clickedIcon = _clickedIcon;
            if (clickedIcon is not null)
            {
                Coords coords = await GetCanvasCoordinatesAsync(mouseEventArgs);

                clickedIcon.Coords = coords;

                CoordsChangeData coordsChangeData = new CoordsChangeData
                {
                    InstanceId = clickedIcon.InstanceId,
                    Coords = coords,
                    ModelId = clickedIcon.ModelId,
                };
                string coordsChangeDataJson = JsonSerializer.Serialize(coordsChangeData);
                await _appState.ChatHubManager.SendCoordsAsync(coordsChangeDataJson);
            }
        }

        private async Task<DndIconElem> GetClickedIcon(MouseEventArgs mouseEventArgs)
        {
            Coords coords = await GetCanvasCoordinatesAsync(mouseEventArgs);

            foreach (DndIconElem icon in _appState.IconsInstances)
            {
                if (coords.X >= icon.Coords.X && coords.X <= icon.Coords.X + 100
                    && coords.Y >= icon.Coords.Y && coords.Y <= icon.Coords.Y + 100)
                {
                    return icon;
                }
            }

            return null;
        }

        private async Task OnMouseDown(MouseEventArgs mouseEventArgs) =>
            _clickedIcon = await GetClickedIcon(mouseEventArgs);
        private void OnMouseUp(MouseEventArgs mouseEventArgs) =>
            _clickedIcon = null;
        private void OnMouseOut(MouseEventArgs mouseEventArgs) =>
            _clickedIcon = null;

        private async Task OnRightClick(MouseEventArgs mouseEventArgs)
        {
            DndIconElem clickedImage = await GetClickedIcon(mouseEventArgs);
            if (clickedImage is not null)
                await _appState.ChatHubManager.SendIconInstanceRemoved(clickedImage.InstanceId);
        }
    }
}
