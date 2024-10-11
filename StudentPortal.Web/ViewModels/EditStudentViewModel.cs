namespace StudentPortal.Web.ViewModels
{
    public class EditStudentViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool Subscribed { get; set; }
        public bool ChangePhotos { get; set; }

        public IEnumerable<IFormFile> Photos { get; set; }
    }
}
