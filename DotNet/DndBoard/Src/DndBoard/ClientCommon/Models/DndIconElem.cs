using DndBoard.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace DndBoard.ClientCommon.Models
{
    public class DndIconElem : DndIcon
    {
        public string Url { get; init; }
        public ElementReference? Ref { get; set; }
    }
}
