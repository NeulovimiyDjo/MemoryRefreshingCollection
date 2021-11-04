using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DndBoard.Shared;
using DndBoard.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace DndBoard.ServerCommon.Hubs
{
    public class BoardsHub : Hub
    {
        private readonly BoardsManager _boardsManager;

        public BoardsHub(BoardsManager boardsManager)
        {
            _boardsManager = boardsManager;
        }


        [HubMethodName(BoardsHubContract.RequestAllModels)]
        public async Task RequestAllModels(string boardId)
        {
            Board board = _boardsManager.GetBoard(boardId);
            ModelFile[] files = board.ModelsFiles.Values.ToArray();
            ModelsFiles modelsFiles = new() { BoardId = boardId, Files = files };
            await Clients.Group(boardId).SendAsync(BoardsHubContract.ModelsAdded, modelsFiles);
        }

        [HubMethodName(BoardsHubContract.RequestAllCoords)]
        public async Task RequestAllCoords(string boardId)
        {
            Board board = _boardsManager.GetBoard(boardId);
            foreach (DndIcon icon in board.IconsInstances.Values)
            {
                CoordsChangeData coordsChangeData = CreateCoordsChangeData(icon);
                await Clients.Caller.SendAsync(BoardsHubContract.CoordsChanged, coordsChangeData);
            }
        }

        private static CoordsChangeData CreateCoordsChangeData(DndIcon icon) =>
            new CoordsChangeData
            {
                InstanceId = icon.InstanceId,
                Coords = icon.Coords,
                ModelId = icon.ModelId,
            };

        [HubMethodName(BoardsHubContract.AddModels)]
        public async Task AddModels(ModelsFiles modelsFiles)
        {
            string boardId = modelsFiles.BoardId;
            Board board = _boardsManager.GetBoard(boardId);
            foreach (ModelFile file in modelsFiles.Files)
                file.ModelId = board.AddModelFile(file);
            await Clients.Group(boardId).SendAsync(BoardsHubContract.ModelsAdded, modelsFiles);
        }

        [HubMethodName(BoardsHubContract.DeleteModel)]
        public async Task DeleteModel(string boardId, string modelId)
        {
            Board board = _boardsManager.GetBoard(boardId);
            board.DeleteModelFile(modelId);
            await Clients.Group(boardId).SendAsync(BoardsHubContract.ModelDeleted, modelId);
        }


        [HubMethodName(BoardsHubContract.CoordsChanged)]
        public async Task SendCoords(string boardId, CoordsChangeData coordsChangeData)
        {
            Board board = _boardsManager.GetBoard(boardId);
            AddNewIconInstanceOrUpdateCoords(coordsChangeData, board);
            await Clients.Group(boardId).SendAsync(BoardsHubContract.CoordsChanged, coordsChangeData);
        }

        private static void AddNewIconInstanceOrUpdateCoords(CoordsChangeData coordsChangeData, Board board)
        {
            if (coordsChangeData.InstanceId is null)
                coordsChangeData.InstanceId = board.AddIconInstance(
                    new DndIcon
                    {
                        Coords = coordsChangeData.Coords,
                        ModelId = coordsChangeData.ModelId,
                    }
                );
            else
                board.ChangeIconInstanceCoords(coordsChangeData.InstanceId, coordsChangeData.Coords);
        }

        [HubMethodName(BoardsHubContract.IconInstanceRemoved)]
        public async Task SendIconInstanceRemoved(string boardId, string instanceId)
        {
            Board board = _boardsManager.GetBoard(boardId);
            board.IconsInstances.Remove(instanceId, out DndIcon _);
            await Clients.Group(boardId).SendAsync(BoardsHubContract.IconInstanceRemoved, instanceId);
        }


        [HubMethodName(BoardsHubContract.Connect)]
        public async Task Connect(string boardId)
        {
            _boardsManager.CreateBoardIfDoesntExist(boardId);
            await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
            await Clients.Caller.SendAsync(BoardsHubContract.Connected, boardId);
        }
    }
}
