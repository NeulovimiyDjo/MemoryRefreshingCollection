using System.Linq;
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
        [Inject] private AppState _appState { get; set; }
        [Inject] private CanvasMapRenderer _canvasMapRenderer { get; set; }
        [Inject] private IJSRuntime _jsRuntime { get; set; }


        private bool _initialized = false;
        protected override void OnInitialized()
        {
            if (_initialized)
                return;
            else
                _initialized = true;

            _appState.BoardIdChangedAsync += OnBoardIdChangedAsync;
            _appState.BoardRenderer.RedrawRequestedAsync += OnRedrawAsync;
            _appState.BoardHubManager.CoordsChanged += OnCoordsReceived;
            _appState.BoardHubManager.IconInstanceRemoved += OnIconInstanceRemoved;
        }


        private async Task OnBoardIdChangedAsync(string boardId)
        {
            await _appState.BoardHubManager.RequestAllCoordsAsync();
        }

        private void OnIconInstanceRemoved(string iconInstanceId)
        {
            _appState.IconsInstances.RemoveAll(icon => icon.InstanceId == iconInstanceId);
        }

        private void OnCoordsReceived(CoordsChangeData coordsChangeData)
        {
            if (!_appState.IconsInstances.Exists(img => img.InstanceId == coordsChangeData.InstanceId))
            {
                _appState.IconsInstances.Add(new DndIconElem
                {
                    InstanceId = coordsChangeData.InstanceId,
                    Coords = coordsChangeData.Coords,
                    ModelId = coordsChangeData.ModelId,
                });
            }

            _appState.IconsInstances.Find(img => img.InstanceId == coordsChangeData.InstanceId)
                .Coords = coordsChangeData.Coords;
        }

        private async Task OnRedrawAsync()
        {
            if (_appState.IconsInstances is null)
                return;

            _appState.IconsInstances.ForEach(x =>
                x.Ref = _appState.IconsModels.FirstOrDefault(y => y.ModelId == x.ModelId)?.Ref
            );

            await _canvasMapRenderer.RedrawIconsByCoordsAsync(
                "IconsInstancesCanvasDiv", _jsRuntime, _appState.IconsInstances
            );
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
                await _appState.BoardHubManager.SendCoordsAsync(coordsChangeData);
            }
        }

        private async Task<DndIconElem> GetClickedIcon(MouseEventArgs mouseEventArgs)
        {
            if (_appState.IconsModels is null)
                return null;

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
            DndIconElem clickedIcon = await GetClickedIcon(mouseEventArgs);
            if (clickedIcon is not null)
                await _appState.BoardHubManager.SendIconInstanceRemovedAsync(clickedIcon.InstanceId);
        }
    }
}
