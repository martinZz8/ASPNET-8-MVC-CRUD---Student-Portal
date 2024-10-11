using Microsoft.AspNetCore.Mvc;
using StudentPortal.Web.DbContexts;
using StudentPortal.Web.HelperClasses;
using StudentPortal.Web.Models.Entities;
using StudentPortal.Web.Services;
using StudentPortal.Web.ViewModels;

namespace StudentPortal.Web.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly StudentService _studentService;

        public StudentsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _studentService = new StudentService(_dbContext);
        }

        // -- Create student page --
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddStudentViewModel viewModel)
        {
            Student responseStudent = await _studentService.AddStudent(viewModel);

            if (responseStudent == null)
            {
                return View("ErrorAdd");
            }

            // Note: We use here "RedirectToAction", because we need to acquire data by call of action method (to fill view with data)
            // We can also pass second argument as string name of controller (here we use current one "Students" as default)
            return RedirectToAction("List");
        }

        // -- Read students page --
        [HttpGet]
        public async Task<IActionResult> List()
        {
            IEnumerable<Student> students = await _studentService.GetStudents();
            return View(students);
        }

        // -- Update student page --
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            Student foundStudent = await _studentService.GetStudentById(id);

            if (foundStudent == null)
            {
                return View("ErrorEdit", id);
            }

            return View(new EditStudentViewModel()
            {
                Id = foundStudent.Id,
                Name = foundStudent.Name,
                Email = foundStudent.Email,
                Phone = foundStudent.Phone,
                Subscribed = foundStudent.Subscribed,
                Photos = Enumerable.Empty<IFormFile>() //old (doesn't fill the input properly, doesn't know why): FileHelper.CreateFormFiles(foundStudent.Photos.Select(it => it.FileName).ToArray())
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditStudentViewModel viewModel)
        {
            Student foundStudent = await _studentService.EditStudent(viewModel);

            if (foundStudent == null)
            {
                return View("ErrorEdit", viewModel.Id);
            }

            return RedirectToAction("List");
        }

        // -- Delete student page
        // Note: We use "HttpPost" type when we want to use delete button in "Edit" page
        // "HttpGet" is used for calling it from "List" (all students) page

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            bool studentDeleted = await _studentService.DeleteStudentById(id);

            if (!studentDeleted)
            {
                return View("ErrorDelete", id);
            }

            return RedirectToAction("List");
        }
    }
}
