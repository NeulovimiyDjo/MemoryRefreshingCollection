using System;
using System.Threading.Tasks;
using DndBoard.Shared;
using DndBoard.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace DndBoard.ClientCommon.Helpers
{
    public class BoardHubManager
    {
        private string _boardId;
        private HubConnection _hubConnection;
        private readonly NavigationManager _navigationManager;

        public event ModelsAddedHandlerAsync ModelsAddedAsync;
        public event ModelDeletedHandler ModelDeleted;
        public event CoordsChangedHandler CoordsChanged;
        public event IconInstanceRemovedHandler IconInstanceRemoved;
        public event ConnectedHandlerAsync ConnectedAsync;


        public BoardHubManager(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public void SetupConnectionAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri(BoardsHubContract.BaseAddress))
                .Build();
        }

        public void SetupEventHandlers()
        {
            _hubConnection.On(BoardsHubContract.ModelsAdded, (Func<ModelsFiles, Task>)InvokeModelsAddedAsync);
            _hubConnection.On(BoardsHubContract.ModelDeleted, (Action<string>)InvokeModelDeleted);
            _hubConnection.On(BoardsHubContract.CoordsChanged, (Action<CoordsChangeData>)InvokeCoordsChanged);
            _hubConnection.On(BoardsHubContract.IconInstanceRemoved, (Action<string>)InvokeIconInstanceRemoved);
            _hubConnection.On(BoardsHubContract.Connected, (Func<string, Task>)InvokeConnectedAsync);
        }

        public async Task StartConnectionAsync() =>
            await _hubConnection.StartAsync();

        public async Task CloseConnectionAsync() =>
            await _hubConnection.DisposeAsync();


        public async Task RequestAllModelsAsync() =>
            await _hubConnection.SendAsync(BoardsHubContract.RequestAllModels, _boardId);

        public async Task RequestAllCoordsAsync() =>
            await _hubConnection.SendAsync(BoardsHubContract.RequestAllCoords, _boardId);


        public async Task AddModelsAsync(ModelsFiles modelsFiles) =>
            await _hubConnection.SendAsync(BoardsHubContract.AddModels, modelsFiles);

        public async Task DeleteModelAsync(string modelId) =>
            await _hubConnection.SendAsync(BoardsHubContract.DeleteModel, _boardId, modelId);


        public async Task SendCoordsAsync(CoordsChangeData coordsChangeData) =>
            await _hubConnection.SendAsync(BoardsHubContract.CoordsChanged, _boardId, coordsChangeData);

        public async Task SendIconInstanceRemovedAsync(string instanceId) =>
            await _hubConnection.SendAsync(BoardsHubContract.IconInstanceRemoved, _boardId, instanceId);


        public async Task ConnectAsync(string boardId)
        {
            _boardId = boardId;
            await _hubConnection.SendAsync(BoardsHubContract.Connect, _boardId);
        }


        private async Task InvokeModelsAddedAsync(ModelsFiles modelsFiles)
        {
            if (ModelsAddedAsync is not null)
                await ModelsAddedAsync.Invoke(modelsFiles);
        }

        private void InvokeModelDeleted(string modelId)
        {
            if (ModelDeleted is not null)
                ModelDeleted.Invoke(modelId);
        }

        private void InvokeCoordsChanged(CoordsChangeData coordsChangeData)
        {
            if (CoordsChanged is not null)
                CoordsChanged.Invoke(coordsChangeData);
        }

        private void InvokeIconInstanceRemoved(string iconInstanceId)
        {
            if (IconInstanceRemoved is not null)
                IconInstanceRemoved.Invoke(iconInstanceId);
        }

        private async Task InvokeConnectedAsync(string boardId)
        {
            if (ConnectedAsync is not null)
                await ConnectedAsync.Invoke(boardId);
        }
    }
}
