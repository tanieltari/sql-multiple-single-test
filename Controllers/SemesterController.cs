using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class SemesterController : ControllerBase
    {
        private readonly string _connectionString;

        public SemesterController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [HttpGet("{semesterId:guid}/Multiple")]
        [ProducesResponseType(typeof(Semester), 200)]
        public async Task<IActionResult> GetSemesterWithMultipleQueries(Guid semesterId)
        {
            var watch = new Stopwatch();
            watch.Start();
            var semester = new Semester();
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using (var command =
                new SqlCommand("SELECT [Id], [Name] FROM [dbo].[Semesters] WHERE [Id] = @SemesterId", connection))
            {
                command.Parameters.AddWithValue("@SemesterId", semesterId);
                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    semester.Id = reader.GetGuid(reader.GetOrdinal(nameof(Semester.Id)));
                    semester.Name = reader.GetString(reader.GetOrdinal(nameof(Semester.Name)));
                    semester.Courses = new LinkedList<Course>();
                }
                else
                {
                    throw new ArgumentException($"No semester in database with id {semesterId.ToString()}");
                }
            }

            await using (var command =
                new SqlCommand("SELECT [Id], [Code], [Name] FROM [dbo].[Courses] WHERE [SemesterId] = @SemesterId",
                    connection))
            {
                command.Parameters.AddWithValue("@SemesterId", semesterId);
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    semester.Courses.Add(new Course
                    {
                        Id = reader.GetGuid(reader.GetOrdinal(nameof(Course.Id))),
                        Code = reader.GetString(reader.GetOrdinal(nameof(Course.Code))),
                        Name = reader.GetString(reader.GetOrdinal(nameof(Course.Name)))
                    });
                }
            }

            watch.Stop();
            Console.WriteLine($"Elapsed time {watch.ElapsedTicks} ticks");
            return Ok(semester);
        }

        [HttpGet("{semesterId:guid}/Single")]
        [ProducesResponseType(typeof(Semester), 200)]
        public async Task<IActionResult> GetSemesterWithSingleQuery(Guid semesterId)
        {
            var watch = new Stopwatch();
            watch.Start();
            Semester? semester = null;
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command =
                new SqlCommand(
                    "SELECT s.[Id], s.[Name], c.[Id] AS CourseId, c.[Code], c.[Name] AS CourseName FROM [dbo].[Semesters] AS s LEFT JOIN [dbo].[Courses] AS c ON s.[Id] = c.[SemesterId] WHERE s.[Id] = @SemesterId",
                    connection);
            command.Parameters.AddWithValue("@SemesterId", semesterId);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                semester ??= new Semester
                {
                    Id = reader.GetGuid(reader.GetOrdinal(nameof(Semester.Id))),
                    Name = reader.GetString(reader.GetOrdinal(nameof(Semester.Name))),
                    Courses = new LinkedList<Course>()
                };
                if (!await reader.IsDBNullAsync(reader.GetOrdinal("CourseId")))
                {
                    semester.Courses.Add(new Course
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("CourseId")),
                        Code = reader.GetString(reader.GetOrdinal(nameof(Course.Code))),
                        Name = reader.GetString(reader.GetOrdinal("CourseName"))
                    });
                }
            }

            watch.Stop();
            Console.WriteLine($"Elapsed time {watch.ElapsedTicks} ticks");
            return Ok(semester ??
                      throw new ArgumentException($"No semester in database with id {semesterId.ToString()}"));
        }
    }
}