using System.Collections.Concurrent;

namespace DndBoard.ServerCommon.Hubs
{
    public class BoardsManager
    {
        private static readonly ConcurrentDictionary<string, Board> _boards =
            new ConcurrentDictionary<string, Board>();


        public bool BoardExists(string boardId)
        {
            return _boards.ContainsKey(boardId);
        }

        public Board GetBoard(string boardId)
        {
            return _boards[boardId];
        }

        public void AddBoard(Board board)
        {
            _boards.TryAdd(board.BoardId, board);
        }
    }
}
