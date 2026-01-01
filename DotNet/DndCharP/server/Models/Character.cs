using System.ComponentModel.DataAnnotations;

namespace server.Models
{
    public class Character
    {
        [Key]
        public long Id { get; set; }

        public string Config { get; set; }
    }
}