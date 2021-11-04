using System.Threading.Tasks;
using DndBoard.Shared.Models;

namespace DndBoard.ClientCommon
{
    public delegate Task ModelsAddedHandlerAsync(ModelsFiles modelsFiles);
    public delegate void ModelDeletedHandler(string modelId);
    public delegate void CoordsChangedHandler(CoordsChangeData coordsChangeData);
    public delegate void IconInstanceRemovedHandler(string iconInstanceId);
    public delegate Task ConnectedHandlerAsync(string boardId);

    public delegate Task RedrawRequestedHandlerAsync();
    public delegate Task BoardIdChangedHandlerAsync(string boardId);
}
