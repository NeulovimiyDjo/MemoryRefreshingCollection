namespace DndBoard.Shared.Models
{
    public class UploadedFiles
    {
        public string BoardId { get; set; }
        public UploadedFile[] Files { get; set; }
    }
}
