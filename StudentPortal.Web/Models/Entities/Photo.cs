using StudentPortal.Web.Models.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPortal.Web.Models.Entities
{
    public class Photo
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string OriginalFileName { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }

        public Guid StudentId { get; set; }
    }
}
