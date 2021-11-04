using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DndBoard.ServerCommon.Hubs;

namespace DndBoard.ServerCommon.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ImagesController : ControllerBase
    {
        private readonly BoardsManager _boardManager;

        public ImagesController(BoardsManager boardManager)
        {
            _boardManager = boardManager;
        }


        [HttpGet("{boardId}/{fileId}")]
        public async Task<IActionResult> GetFile(string boardId, string fileId)
        {
            Board board = _boardManager.GetBoard(boardId);
            byte[] file = board.GetFile(fileId);
            return File(file, "image/png");
        }

        [HttpGet("{boardId}")]
        public async Task<IEnumerable<string>> GetFilesIds(
            [FromRoute(Name = "boardId")] string boardId)
        {
            Board board = _boardManager.GetBoard(boardId);
            return board.GetFilesIds();
        }


        //[HttpPost]
        //public async Task<IActionResult> PostFiles(
        //    [FromForm(Name = "boardId")] string boardId,
        //    [FromForm(Name = "file")] IEnumerable<IFormFile> files)
        //{
        //    _logger.LogInformation($"---------------------{files}-----------------------");

        //    //check if file was fully uploaded
        //    if (files == null || files.Count() == 0)
        //        return BadRequest("Upload a new File");

        //    foreach (IFormFile file in files)
        //    {
        //        using MemoryStream ms = new MemoryStream();
        //        await file.CopyToAsync(ms);

        //        Board board = _boardManager.GetBoard(boardId);
        //        board.AddFile(ms.ToArray());
        //    }
        //    await _boardsHubContext.Clients.Group(boardId).SendAsync(BoardsHubContract.NotifyFilesUpdate, boardId);

        //    return Ok("do something with this data....");
        //}
    }
}
