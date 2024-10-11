using StudentPortal.Web.ViewModels;

namespace StudentPortal.Web.HelperClasses
{
    public static class FileHelper
    {
        private static readonly string PhotosFolder = Path.Join(Environment.CurrentDirectory, "wwwroot", "images", "uploaded");

        public static IEnumerable<IFormFile> CreateFormFiles(string[] fileNames, bool ommitNotExistingFiles = true)
        {
            List<IFormFile> formFilesToReturn = new List<IFormFile>();

            foreach (string fileName in fileNames)
            {
                string photoPathToRead = Path.Join(PhotosFolder, fileName);

                if (!Path.Exists(photoPathToRead))
                {
                    if (!ommitNotExistingFiles)
                    {
                        return Enumerable.Empty<IFormFile>();
                    }
                    continue;
                }

                using (Stream fileStream = File.Open(photoPathToRead, FileMode.Open))
                {
                    formFilesToReturn.Add(
                        new FormFile(
                            fileStream,
                            0,
                            fileStream.Length,
                            nameof(EditStudentViewModel.Photos),
                            fileName
                        )
                    );
                }
            }

            return formFilesToReturn;
        }
    }
}
