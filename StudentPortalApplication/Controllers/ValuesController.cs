using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Security.Claims;
//using System.Web.Mvc;



namespace StudentPortalApplication.Controllers
{
    public class ValuesController : ApiController
    {
        StudentPortalEntities context = new StudentPortalEntities();

        [HttpGet]

        // GET api/values
        public List<StudentCredential> Get()
        {
            //extract studentID from claims
            var studentIdClaim = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier);
            if (studentIdClaim == null)
            {
                //handle case where studentID isn't found
                throw new UnauthorizedAccessException("StudentID claim not found.");
            }
            int studentId = int.Parse(studentIdClaim.Value);

            //query database for items specific to the logged in user
            var data = context.StudentCredential.Where(item => item.studentID ==  studentId).ToList();
            
            return data;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        //POST api/values
        [Route("AddCredentials")]
        [HttpPost]
        public IHttpActionResult Post(StudentCredential newItem)
        {
            using (var newPost = new StudentPortalEntities())
            {
                if (newItem == null)
                {
                    return BadRequest("No data");
                }

                try
                {
                    var claimsIdentity = (ClaimsIdentity)this.RequestContext.Principal.Identity;
                    var userIdClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                    if (userIdClaim == null)
                    {
                        return BadRequest("User not authenticated");
                    }

                    newItem.studentID = long.Parse(userIdClaim.Value);

                    newPost.StudentCredential.Add(newItem);
                    newPost.SaveChanges();   

                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Ok(newItem);
            }

        }

        [Route("AddInformation")]
        [HttpPost]
        // public IHttpActionResult Post(StudentInformation newItem)
        public IHttpActionResult Post()
        {
            var newItem = 1;
            using (var dbContext = new StudentPortalEntities())
            {
                if (newItem == null)
                {
                    return BadRequest("No data");
                }
                //dbContext.StudentInformation.Add(newItem);
                dbContext.SaveChanges();

                return Ok(newItem);
            }

        }

        
        // PUT api/values/5
        public IHttpActionResult Put(StudentCredential updatedStudentCredential)
        {
            if (updatedStudentCredential == null)
            {
                return BadRequest("Invalid student data.");
            }

            using (var context = new StudentPortalEntities())
            {
                context.Configuration.LazyLoadingEnabled = false;
                //find existing student by ID
                var existingStudent = context.StudentCredential.FirstOrDefault(student => student.studentID == updatedStudentCredential.studentID);

                if (existingStudent == null)
                {
                    return NotFound();
                }
                else
                {
                    //update existing student
                    existingStudent.studentUsername = updatedStudentCredential.studentUsername;

                    //save changes to database
                    context.SaveChanges();

                }

                return Ok(existingStudent);

            }
        }

        // DELETE api/values/5
        [Route("api/data/StudentCredential")]
        public IHttpActionResult Delete(StudentCredential deletedStudentCredential)
        {
            try
            {
                using (var context = new StudentPortalEntities())
                {
                    var deletedStudent = context.StudentCredential.Find(deletedStudentCredential.studentID);
                    var deletedInfor = context.StudentInformation.Find(deletedStudentCredential.studentID);
                    var studentInfo = context.StudentInformation.FirstOrDefault(s => s.studentID == deletedStudentCredential.studentID);
                    if (deletedStudent != null)
                    {
                        context.StudentCredential.Remove(deletedStudent);
                        context.StudentInformation.Remove(studentInfo);
                        context.SaveChanges();
                        return Ok();
                    }
                    else
                    {
                        return NotFound();
                    }

                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}    

