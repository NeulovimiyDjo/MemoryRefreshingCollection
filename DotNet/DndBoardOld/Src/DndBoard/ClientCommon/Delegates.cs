using System.Threading.Tasks;

namespace DndBoard.ClientCommon
{
    public delegate Task FilesRefsChangedHandler();
    public delegate Task BoardIdChangedHandler(string boardId);
}
