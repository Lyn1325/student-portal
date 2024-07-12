using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using System.Collections;

namespace StudentPortalApplication.Controllers
{
    public class UserController : ApiController
    {
        public List<StudentCredential> Credentials = new List<StudentCredential>();

        [HttpPost]
        [Route("api/data/register")]
        public IHttpActionResult RegisterStudent(StudentCredential studentCredential)
        {
            if (studentCredential == null)
            {
                return BadRequest("Invalid course credential.");
            }
            using (var dbContext = new StudentPortalEntities())
            {
                dbContext.StudentCredential.Add(studentCredential);
                dbContext.SaveChanges();
                return Ok($"{studentCredential.studentUsername} registered successfully.");
            }

        }






        [AllowAnonymous]
        [HttpGet]
        [Route("api/data/forall")]

        //for anonymous users, no authentication or token is needed for this.
        public IHttpActionResult Get()
        {
            return Ok("Now server time is: " + DateTime.Now.ToString());
        }

        //allows authorized user to access it
        [Authorize]
        [HttpGet]
        [Route("api/data/authenticate")]
        public IHttpActionResult GetForAuthenticate()
        {
            var identity = (ClaimsIdentity)User.Identity;
            return Ok("Hello " + identity.Name);
        }

        //checks authorization and allows only admins to access it.
        [Authorize(Roles = "admin")]
        [HttpGet]
        [Route("api/data/authorize")]
        public IHttpActionResult GetForAdmin()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var roles = identity.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

            return Ok("Hello " + identity.Name + " Role: " + string.Join(",", roles.ToList()));
        }

        [Authorize]
        [HttpGet]
        [Route("api/UserInfo")]
        public IHttpActionResult GetUserData()
        {
            StudentPortalEntities context = new StudentPortalEntities();
            //get identity claims
            var identity = (ClaimsIdentity)User.Identity;

            //extract specific claims
            var studentIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            int studentId = int.Parse(studentIdClaim.Value);

            var nameClaim = identity.FindFirst(ClaimTypes.Name);
            var name = nameClaim.Value;

            //create a response with user claims
            var data = context.StudentInformation.Where(item => item.studentID == studentId).ToList();
            

            return Ok(data);
        }

        //[Authorize]
        [Route("AddCourseInfo")]
        [HttpPost]
        public IHttpActionResult Post(StudentCourse newCourseItem)
        {
            if (newCourseItem == null)
            {
                return BadRequest("Invalid studentCourse");

            }
            using (var dbContext = new StudentPortalEntities())
            {
                var existingStudent = dbContext.StudentCourse.FirstOrDefault(student => student.studentID == newCourseItem.studentID);
                if (existingStudent == null)
                {
                    return BadRequest("No data");
                }
                /*if (courseItemInput.courseID == 0)
                {
                    return BadRequest("No courseID");
                }
               */

                //bool containsItem = dbContext.StudentCourse.Contains(courseItemInput);
                bool containsItem = dbContext.Courses.Any(sc => sc.Id == newCourseItem.courseID);

                if (!containsItem) 
                {
                    return BadRequest("No course Id");

                }
                newCourseItem.createdBy = existingStudent.studentID;
                newCourseItem.createDate = DateTime.Now;
                dbContext.StudentCourse.Add(newCourseItem);
                dbContext.SaveChanges();

                return Ok("new course was added to user");
            }

        }

        [Route("AddCourse")]
        [HttpPost]
        public IHttpActionResult Post(Courses newCourse)
        {
            if (newCourse == null)
            {
                return BadRequest("Invalid Course");

            }
            using (var coursePost = new StudentPortalEntities())
            {
                var existingStudent = coursePost.StudentCourse.FirstOrDefault(student => student.studentID == newCourse.Id);
                if (existingStudent == null)
                {
                    return BadRequest("No data");
                }
                coursePost.Courses.Add(newCourse);
                coursePost.SaveChanges();

                return Ok("new course added");
            }

        }

        [Route("GetStudentCourses/{studentID}")]
        [HttpGet]
        public async Task<List<string>> GetStudentCourses(long studentID)

        {
            using (var studentCourses = new StudentPortalEntities())
            {
                
                List<Courses> courses = await (from studentCourse in studentCourses.StudentCourse
                                               join Courses in studentCourses.Courses
                                               on studentCourse.Id equals Courses.Id
                                               where studentCourse.studentID == studentID
                                               select Courses).ToListAsync();

                List<string> courseTitle = courses.Select(c => c.title).ToList();
                return (courseTitle);
            }

        }

        

        [Route("GetCourses")]
        [HttpGet]
        public async Task<List<Courses>> GetCourses()
        {
            //Query for all the course IDs in the course table and put it into a list
            //check if courseItemInput.courseID is contained in courseID list
            using (var dbContext = new StudentPortalEntities())
            {
                // List<Courses> courses = dbContext.Courses.ToList();
                dbContext.Configuration.LazyLoadingEnabled = false;
                List<Courses> courses = await dbContext.Courses.ToListAsync();
                
                return (courses);
            }
        }



        [Route("UpdateCourseInfo")]
        [HttpPut]
        public IHttpActionResult UpdateCourseInfo(StudentCourse courseItemInput)
        {
            if (courseItemInput == null)
            {
                return BadRequest("Invalid studentCourse");

            }

            using (var dbContext = new StudentPortalEntities())
            {

                var existingCourse = dbContext.StudentCourse.FirstOrDefault(course => course.studentID == courseItemInput.studentID && course.Id == courseItemInput.Id);
                if (existingCourse == null)
                {
                    return BadRequest("No data");
                }
                
                bool containsItem = dbContext.Courses.Any(sc => sc.Id == courseItemInput.courseID);

                if (!containsItem)
                {
                    return BadRequest("No course Id");

                }
                existingCourse.changedBy = courseItemInput.studentID;
                existingCourse.changeDate = DateTime.Now;
                existingCourse.completed = true;
               
                dbContext.SaveChanges();

                return Ok("course was updated by user");
            }

        }
        

        //finish up this method and check if it's working
        [Route("deleteStudentCourse")]
        [HttpDelete]

        public IHttpActionResult DeleteStudentCourse(StudentCourse deleteStudentCourse)
        {
            try
            {
                using (var context = new StudentPortalEntities())
                {
                    var studentCourseToDelete = context.StudentCourse.Find(deleteStudentCourse.Id);
                    if (studentCourseToDelete == null)
                    {
                        return NotFound();
                    }

                    context.StudentCourse.Remove(studentCourseToDelete);
                    context.SaveChanges();
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                // Log the exception here if necessary
                return InternalServerError(ex);
            }
        }
    }








}


