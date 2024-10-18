using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using StudentPortal.Web.DbContexts;
using StudentPortal.Web.Models.Entities;
using StudentPortal.Web.ViewModels;

namespace StudentPortal.Web.Services
{
    public interface IStudentService
    {
        Task<Student> AddStudent(AddStudentViewModel viewModel);
        Task<IEnumerable<Student>> GetStudents();
        Task<Student> GetStudentById(Guid id);
        Task<Student> EditStudent(EditStudentViewModel viewModel);
        Task<bool> DeleteStudentById(Guid id);
    }

    public class StudentService: IStudentService
    {
        private readonly ApplicationDbContext _dbContext;

        public StudentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Student> AddStudent(AddStudentViewModel viewModel)
        {
            // Check if mandatory fields are filled
            if (viewModel.Name == null || viewModel.Email == null)
            {
                return null;
            }

            // Check if student with given email already exists (we cannot have multiple users with same email)
            Student studentWithEmail = await _dbContext.Students.Where(it => it.Email.Equals(viewModel.Email)).FirstOrDefaultAsync();

            if (studentWithEmail != null)
            {
                return null;
            }

            // Begin database transaction (for student and its photos)
            // based on: https://learn.microsoft.com/en-us/ef/ef6/saving/transactions
            using (var dbContextTransaction = _dbContext.Database.BeginTransaction())
            {
                // Create student to be saved
                Student newStudent = new Student()
                {
                    Name = viewModel.Name,
                    Email = viewModel.Email,
                    Phone = viewModel.Phone,
                    Subscribed = viewModel.Subscribed
                };

                // Save student to db
                _dbContext.Students.Add(newStudent);
                await _dbContext.SaveChangesAsync();

                // Save image (if exists) to local file and db
                if (viewModel.Photos?.Count() > 0)
                {
                    foreach (IFormFile photo in viewModel.Photos)
                    {
                        Photo newPhoto = new Photo()
                        {
                            FileName = await SaveLocalPhotoWithGeneratedName(photo),
                            OriginalFileName = photo.FileName,
                            Student = newStudent
                        };

                        _dbContext.Photos.Add(newPhoto);
                    }
                    await _dbContext.SaveChangesAsync();
                }

                dbContextTransaction.Commit();
                return newStudent;
            }
        }

        public async Task<IEnumerable<Student>> GetStudents()
        {
            return await _dbContext.Students.Include(it => it.Photos).ToListAsync();
        }

        public async Task<Student> GetStudentById(Guid id)
        {
            return await _dbContext.Students.Include(it => it.Photos).FirstOrDefaultAsync(it => it.Id.Equals(id));
        }

        public async Task<Student> EditStudent(EditStudentViewModel viewModel)
        {
            // Check if mandatory fields are filled
            if (viewModel.Name == null || viewModel.Email == null)
            {
                return null;
            }

            // Check if student with given id exists
            Student foundStudent = await _dbContext.Students.Include(it => it.Photos).FirstOrDefaultAsync(it => it.Id.Equals(viewModel.Id));

            if (foundStudent == null)
            {
                return null;
            }

            // Change user parameters
            foundStudent.Name = viewModel.Name;
            foundStudent.Phone = viewModel.Phone;
            foundStudent.Subscribed = viewModel.Subscribed;

            // Check, wheter the photos were provided (and checkbox "viewModel.ChangePhotos" was checked)
            // If yes, check for names:
            // - add new photos (with new names),
            // - discard photos which names are not more present,
            // - don't do anything with photos with same name (as previously added)
            if (viewModel.ChangePhotos)
            {
                // Get photos that are removed (doesn't exist in NEWLY provided photo list "viewModel.Photos")
                IEnumerable<Photo> photosToRemove = foundStudent.Photos.Where(it => !viewModel.Photos.Select(it2 => it2.FileName).Contains(it.FileName));
                foreach (Photo photoToRemove in photosToRemove)
                {
                    // Delete old photo (if exists)
                    DeleteLocalPhotoWithGivenName(photoToRemove.FileName);
                    _dbContext.Photos.Remove(photoToRemove);
                }

                // Get photos that are added (doesn't exist in OLD provided photo list "foundStudent.Photos" from db)
                IEnumerable<IFormFile> photosToAdd = viewModel.Photos.Where(it => !foundStudent.Photos.Select(it2 => it2.FileName).Contains(it.FileName));
                foreach (IFormFile photoToAddd in photosToAdd)
                {
                    // Add new photo
                    Photo newPhoto = new Photo()
                    {
                        FileName = await SaveLocalPhotoWithGeneratedName(photoToAddd),
                        OriginalFileName = photoToAddd.FileName,
                        Student = foundStudent
                    };
                    _dbContext.Photos.Add(newPhoto);
                }
            }

            await _dbContext.SaveChangesAsync();            
            return foundStudent;
        }

        public async Task<bool> DeleteStudentById(Guid id)
        {
            // Check if student with given id exists
            Student foundStudent = await _dbContext.Students.Include(it => it.Photos).FirstOrDefaultAsync(it => it.Id.Equals(id));

            if (foundStudent == null)
            {
                return false;
            }

            // Delete all locally saved photos that belogns to student
            foreach (Photo photoToDelete in foundStudent.Photos)
            {
                DeleteLocalPhotoWithGivenName(photoToDelete.FileName);
            }

            // Remove student (with assigned photos)
            _dbContext.Students.Remove(foundStudent);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Method to save photo to local directory of this project "wwwroot\images\uploaded".
        /// It also generates guid of this image.
        /// </summary>
        /// <returns>
        /// Returns generated file name based on generated guid and given extensions of the file.
        /// </returns>
        private async Task<string> SaveLocalPhotoWithGeneratedName(IFormFile photo)
        {
            string uploadFilesDirectory = Path.Join(Environment.CurrentDirectory, "wwwroot", "images", "uploaded");

            string idOfPhoto = Guid.NewGuid().ToString();
            string extensionOfCurrentPhoto = photo.FileName.Split(".").Last();
            string photoFileNameToSave = $"{idOfPhoto}.{extensionOfCurrentPhoto}";
            string photoPathToSave = Path.Join(uploadFilesDirectory, photoFileNameToSave);

            using (Stream stream = File.Open(photoPathToSave, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            return photoFileNameToSave;
        }

        /// <summary>
        /// Method to delete locally photo with given name.
        /// </summary>
        /// <returns>
        /// Returns true if file was deleted successfully, otherwise false.
        /// </returns>
        private bool DeleteLocalPhotoWithGivenName(string photoName)
        {
            string uploadFilesDirectory = Path.Join(Environment.CurrentDirectory, "wwwroot", "images", "uploaded");
            string photoPathToDelete = Path.Join(uploadFilesDirectory, photoName);

            if (File.Exists(photoPathToDelete))
            {
                File.Delete(photoPathToDelete);
                return true;
            }

            return false;
        }
    }
}
