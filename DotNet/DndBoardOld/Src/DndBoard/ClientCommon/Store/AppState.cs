using System.Collections.Generic;
using System.Threading.Tasks;
using DndBoard.ClientCommon.Helpers;
using DndBoard.ClientCommon.Models;

namespace DndBoard.ClientCommon.Store
{
    public class AppState
    {
        public ChatHubManager ChatHubManager;
        public List<DndIconElem> IconsInstances;
        public List<DndIconElem> IconsModels;

        public event FilesRefsChangedHandler FilesRefsChanged;
        public event BoardIdChangedHandler BoardIdChanged;

        public async Task InvokeFilesRefsChanged()
        { // null conditional operator doesn't work. GG.
            if (FilesRefsChanged is not null)
                await FilesRefsChanged.Invoke();
        }
        public async Task InvokeBoardIdChanged(string boardId)
            => await BoardIdChanged?.Invoke(boardId);
    }
}
