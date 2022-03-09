using System.ComponentModel.DataAnnotations;

namespace Abstractions.Models
{
    public class UserData
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string PhoneNumber { get; set; }

    }
}
