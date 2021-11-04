using System.Collections.Generic;
using System.Text.Json;
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

        [HubMethodName(BoardsHubContract.IconInstanceRemoved)]
        public async Task SendIconInstanceRemoved(string boardId, string imageId)
        {
            Board board = _boardsManager.GetBoard(boardId);
            board.IconsInstances.RemoveAll(img => img.InstanceId == imageId);
            await Clients.Group(boardId).SendAsync(BoardsHubContract.IconInstanceRemoved, imageId);
        }

        [HubMethodName(BoardsHubContract.RequestAllCoords)]
        public async Task RequestAllCoords(string boardId)
        {
            Board board = _boardsManager.GetBoard(boardId);
            foreach (DndIcon icon in board.IconsInstances)
            {
                CoordsChangeData coordsChangeData = new CoordsChangeData
                {
                    InstanceId = icon.InstanceId,
                    Coords = icon.Coords,
                    ModelId = icon.ModelId,
                };

                string coordsChangeDataJson = JsonSerializer.Serialize(coordsChangeData);
                await Clients.Caller.SendAsync(BoardsHubContract.CoordsChanged, coordsChangeDataJson);
            }
        }

        [HubMethodName(BoardsHubContract.CoordsChanged)]
        public async Task SendCoords(string boardId, string coordsChangeDataJson)
        {
            Board board = _boardsManager.GetBoard(boardId);
            CoordsChangeData coordsChangeData = JsonSerializer
                .Deserialize<CoordsChangeData>(coordsChangeDataJson);

            if (!board.IconsInstances.Exists(icon => icon.InstanceId == coordsChangeData.InstanceId))
                board.IconsInstances.Add(new DndIcon
                {
                    InstanceId = coordsChangeData.InstanceId,
                    Coords = coordsChangeData.Coords,
                    ModelId = coordsChangeData.ModelId,
                });
            else
                board.IconsInstances.Find(icon => icon.InstanceId == coordsChangeData.InstanceId)
                    .Coords = coordsChangeData.Coords;

            await Clients.Group(boardId).SendAsync(BoardsHubContract.CoordsChanged, coordsChangeDataJson);
        }

        [HubMethodName(BoardsHubContract.Connect)]
        public async Task Connect(string boardId)
        {
            if (!_boardsManager.BoardExists(boardId))
                _boardsManager.AddBoard(new Board
                {
                    BoardId = boardId,
                    IconsInstances = new List<DndIcon>()
                });

            await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
            await Clients.Caller.SendAsync(BoardsHubContract.Connected, boardId);
        }
    }
}
