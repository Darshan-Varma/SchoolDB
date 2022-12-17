using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SchoolDB.Models;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Diagnostics;

namespace SchoolDB.Controllers
{
    public class TeacherDataController : ApiController
    {
        // The database context class which allows us to access our MySQL Database.
        private SchoolDbContext School = new SchoolDbContext();

        //This Controller Will access the Teachers table of our school database.
        /// <summary>
        /// Returns a list of Teachers in the system
        /// </summary>
        /// <example>GET api/TeacherData/ListTeachers</example>
        /// <returns>
        /// A list of Teachers (first names and last names)
        /// </returns>
        [HttpGet]
        [Route("api/TeacherData/ListTeachers/{SearchKey?}")]
        public IEnumerable<Teacher> ListTeachers(string SearchKey = null)
        {
            //Create an instance of a connection
            MySqlConnection Conn = School.AccessDatabase();

            //Open the connection between the web server and database
            Conn.Open();

            //Establish a new command (query) for our database
            MySqlCommand cmd = Conn.CreateCommand();

            //SQL QUERY
            
            cmd.CommandText = "Select * FROM Teachers WHERE teacherfname like lower('%"+SearchKey+"%') or teacherlname like lower('%"+SearchKey+"%') or teacherid like lower('%"+SearchKey+"%') or lower(concat(teacherfname,' ',teacherlname)) like lower('%"+SearchKey+"%')";
            cmd.Parameters.AddWithValue("@key", "%" + SearchKey + "%");
            cmd.Prepare();

            //Gather Result Set of Query into a variable
            MySqlDataReader ResultSet = cmd.ExecuteReader();

            //Create an empty list of Teachers
            List<Teacher> Teachers = new List<Teacher> { };

            //Loop Through Each Row the Result Set
            while (ResultSet.Read())
            {
                //Access Column information by the DB column name as an index
                int TeacherId = (int)ResultSet["teacherid"];
                string TeacherFname = ResultSet["teacherfname"].ToString();
                string TeacherLname = ResultSet["teacherlname"].ToString();
                string EmployeeNumber = ResultSet["employeenumber"].ToString();
                string HireDate = ResultSet["hiredate"].ToString();
                string TeacherSalary = ResultSet["salary"].ToString();

                Teacher NewTeacher = new Teacher();
                NewTeacher.TeacherId = TeacherId;
                NewTeacher.TeacherFname = TeacherFname;
                NewTeacher.TeacherLname = TeacherLname;
                NewTeacher.EmployeeNumber = EmployeeNumber;
                NewTeacher.HireDate = HireDate;
                NewTeacher.TeacherSalary = TeacherSalary;

                //Add the Teacher Name to the List
                Teachers.Add(NewTeacher);
            }

            //Close the connection between the MySQL Database and the WebServer
            Conn.Close();

            //Return the final list of Teacher names
            return Teachers;
        }


        /// <summary>
        /// Finds an Teacher in the system given an ID
        /// </summary>
        /// <param name="id">The Teacher primary key</param>
        /// <example>GET api/TeacherData/FindTeacher/3</example>
        /// <returns>An Teacher object</returns>
        [HttpGet]
        public Teacher FindTeacher(int id)
        {
            Teacher NewTeacher = new Teacher();

            //Create an instance of a connection
            MySqlConnection Conn = School.AccessDatabase();

            //Open the connection between the web server and database
            Conn.Open();

            //Establish a new command (query) for our database
            MySqlCommand cmd = Conn.CreateCommand();

            //SQL QUERY
            cmd.CommandText = "SELECT t.*,c.* from teachers t left join classes c on t.teacherid = c.teacherid where t.teacherid = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Prepare();

            //Gather Result Set of Query into a variable
            MySqlDataReader ResultSet = cmd.ExecuteReader();

            while (ResultSet.Read())
            {
                //Access Column information by the DB column name as an index
                int TeacherId = (int)ResultSet["teacherid"];
                string TeacherFname = ResultSet["teacherfname"].ToString();
                string TeacherLname = ResultSet["teacherlname"].ToString();
                string EmployeeNumber = ResultSet["employeenumber"].ToString();
                string HireDate = ResultSet["hiredate"].ToString();
                string TeacherSalary = ResultSet["salary"].ToString();
                string courseName = ResultSet["classname"].ToString();


                NewTeacher.TeacherId = TeacherId;
                NewTeacher.TeacherFname = TeacherFname;
                NewTeacher.TeacherLname = TeacherLname;
                NewTeacher.EmployeeNumber = EmployeeNumber;
                NewTeacher.HireDate = Convert.ToDateTime(HireDate).ToString("yyyy/MM/dd");
                NewTeacher.TeacherSalary = TeacherSalary;
                if (courseName == String.Empty)
                    NewTeacher.CourseName = "No subject";
                else
                    NewTeacher.CourseName = courseName;
            }


            return NewTeacher;
        }

        /// <summary>
        /// Deletes an Teacher from the connected MySQL Database if the ID of that teacher exists.
        /// </summary>
        /// <param name="id">The ID of the teacher.</param>
        /// <example>POST /api/TeacherData/DeleteTeacher/3</example>
        [HttpPost]
        public void DeleteTeacher(int id)
        {
            MySqlConnection Conn = School.AccessDatabase();

            //Open the connection between the web server and database
            Conn.Open();

            //Establish a new command (query) for our database
            MySqlCommand cmd = Conn.CreateCommand();

            //SQL QUERY
            cmd.CommandText = "Delete from teachers where teacherid=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Prepare();

            cmd.ExecuteNonQuery();
            
            //Deleting all records from classes table of deleted teacher
            cmd.CommandText = "Delete from classes where teacherid = @teacherid";
            cmd.Parameters.AddWithValue("@teacherid", id);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Conn.Close();


        }

        /// <summary>
        /// Adds an Teacher to the MySQL Database.
        /// </summary>
        /// <param name="NewTeacher">An object with fields that map to the columns of the teacher's table.</param>
        /// <example>
        /// POST api/TeacherData/AddTeacher
        /// FORM DATA / POST DATA / REQUEST BODY 
        /// {
        ///	"TeacherFname":"Sean",
        ///	"TeacherLname":"Doyle",
        ///	"EmployeeNumber":"T606",
        ///	"Salary":"75.43"
        /// }
        /// </example>
        [HttpPost]
        public void AddTeacher([FromBody] Teacher NewTeacher)
        {
            //Create an instance of a connection
            MySqlConnection Conn = School.AccessDatabase();
            if (!string.IsNullOrEmpty(NewTeacher.TeacherFname) && !string.IsNullOrEmpty(NewTeacher.TeacherLname))
            {
                //Open the connection between the web server and database
                Conn.Open();

                //Establish a new command (query) for our database
                MySqlCommand cmd = Conn.CreateCommand();

                //SQL QUERY
                cmd.CommandText = "insert into teachers (teacherfname, teacherlname, employeenumber, hiredate, salary) values (@TeacherFname,@TeacherLname,@EmployeeNumber, @HireDate, @Salary)";
                cmd.Parameters.AddWithValue("@TeacherFname", NewTeacher.TeacherFname);
                cmd.Parameters.AddWithValue("@TeacherLname", NewTeacher.TeacherLname);
                cmd.Parameters.AddWithValue("@EmployeeNumber", NewTeacher.EmployeeNumber);
                cmd.Parameters.AddWithValue("@HireDate", NewTeacher.HireDate);
                cmd.Parameters.AddWithValue("@Salary", NewTeacher.TeacherSalary);

                cmd.Prepare();


                cmd.ExecuteNonQuery();
            }
            Conn.Close();



        }

        /// <summary>
        /// Updates an Teacher on the MySQL Database. Non-Deterministic.
        /// </summary>
        /// <param name="TeacherInfo">An object with fields that map to the columns of the teacher's table.</param>
        /// <example>
        /// POST api/TeacherData/UpdateTeacher/208 
        /// FORM DATA / POST DATA / REQUEST BODY 
        /// {
        ///	"TeacherFname":"Christine",
        ///	"TeacherLname":"Bittle",
        ///	"EmployeeNumber":"T606",
        ///	"HireDate":"2016-04-03",
        ///	"TeacherSalary":"74.34"
        /// }
        /// </example>
        [HttpPost]
        public void UpdateTeacher(int id, [FromBody] Teacher TeacherInfo)
        {
            //Create an instance of a connection
            MySqlConnection Conn = School.AccessDatabase();

            //Using server side validation. Block update if any data is missing.
            if (!string.IsNullOrEmpty(TeacherInfo.TeacherFname) && !string.IsNullOrEmpty(TeacherInfo.TeacherLname) && !string.IsNullOrEmpty(TeacherInfo.EmployeeNumber) && !string.IsNullOrEmpty(TeacherInfo.HireDate) && !string.IsNullOrEmpty(TeacherInfo.TeacherSalary)) {

                //Open the connection between the web server and database
                Conn.Open();

                //Establish a new command (query) for our database
                MySqlCommand cmd = Conn.CreateCommand();

                //SQL QUERY
                cmd.CommandText = "update teachers set teacherfname=@TeacherFname, teacherlname=@TeacherLname, employeenumber=@EmployeeMumber, hiredate=@HireDate,salary=@TeacherSalary where teacherid=@TeacherId";
                cmd.Parameters.AddWithValue("@TeacherFname", TeacherInfo.TeacherFname);
                cmd.Parameters.AddWithValue("@TeacherLname", TeacherInfo.TeacherLname);
                cmd.Parameters.AddWithValue("@EmployeeMumber", TeacherInfo.EmployeeNumber);
                cmd.Parameters.AddWithValue("@HireDate", TeacherInfo.HireDate);
                cmd.Parameters.AddWithValue("@TeacherSalary", TeacherInfo.TeacherSalary);
                cmd.Parameters.AddWithValue("@TeacherId", id);
                cmd.Prepare();

                cmd.ExecuteNonQuery();

                Conn.Close();
            }


        }
    }
}
