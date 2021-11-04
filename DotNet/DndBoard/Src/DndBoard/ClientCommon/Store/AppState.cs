using System.Collections.Generic;
using System.Threading.Tasks;
using DndBoard.ClientCommon.Helpers;
using DndBoard.ClientCommon.Models;

namespace DndBoard.ClientCommon.Store
{
    public class AppState
    {
        public BoardRenderer BoardRenderer { get; set; }
        public BoardHubManager BoardHubManager { get; set; }
        public List<DndIconElem> IconsInstances { get; set; }
        public List<DndIconElem> IconsModels { get; set; }

        public event BoardIdChangedHandlerAsync BoardIdChangedAsync;


        public async Task InvokeBoardIdChangedAsync(string boardId)
        {
            if (BoardIdChangedAsync is not null)
                await BoardIdChangedAsync.Invoke(boardId);
        }
    }
}
