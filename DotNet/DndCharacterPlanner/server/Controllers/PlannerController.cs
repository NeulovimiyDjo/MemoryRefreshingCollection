using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;
using System.Linq;

namespace server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PlannerController : ControllerBase
    {
        private CharactersContext _db;
        private readonly DndDatabase _dndDb;

        public PlannerController(CharactersContext context, DndDatabase dndDb)
        {
            _db = context;
            _dndDb = dndDb;
        }


        [HttpGet]
        public ActionResult<string> GetDndDatabase()
        {
            Response.ContentType = "text/plain";
            return _dndDb.Data;
        }


        [HttpGet]
        public ActionResult<string> GetCharacter(long id)
        {
            Response.ContentType = "text/plain";

            var character = _db.Characters.FirstOrDefault(c => c.Id == id);
            if (character == null) return NotFound("Character with this guid doesn't exits!");

            return character.Config;
        }


        [HttpPost]
        public ActionResult<long> SaveCharacter([FromBody] string config)
        {
            var character = new Character { Config = config };
            _db.Characters.Add(character);
            _db.SaveChanges();

            Response.ContentType = "application/json";
            return character.Id;
        }
    }
}