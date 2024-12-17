﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListAPI.Application.DTOs.Teacher;
using ToDoListAPI.Application.Services;
using ToDoListAPI.Application.Token;
using ToDoListAPI.Domain.Entities;
using T = ToDoListAPI.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using ToDoListAPI.Domain.Entities.Identity;
using ToDoListAPI.Application.Exceptions;
using System.Web.Mvc;
using ToDoListAPI.Application.DTOs;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ToDoListAPI.Application.Repository;
using Microsoft.EntityFrameworkCore;
using ToDoListAPI.Persistence.Context;
using Task = ToDoListAPI.Domain.Entities.Task;
using ToDoListAPI.Domain.Entities.Role;
using ToDoListAPI.Application.DTOs.Teacher;
using ToDoListAPI.Application.DTOs.Student;
using ToDoListAPI.Application.DTOs.StudentTeacher;
using ToDoListAPI.Application.DTOs.Classroom;

namespace ToDoListAPI.Persistence.Services
{
	public class TeacherService : ITeacherService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly IAppUserService _appUserService;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly ITokenHandler _tokenHandler;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ITeacherReadRepository _teacherReadRepository;
		private readonly ITeacherWriteRepository _teacherWriteRepository;
		private readonly IStudentReadRepository _studentReadRepository;
		private readonly IStudentWriteRepository _studentWriteRepository;
		private readonly IStudentTeacherWriteRepository _studentTeacherWriteRepository;
		private readonly IStudentTeacherReadRepository _studentTeacherReadRepository;
		private readonly IStudentClassroomWriteRepository _studentClassroomWriteRepository;
		private readonly IStudentClassroomReadRepository _studentClassroomReadRepository;
		private readonly AppDbContext _context;
		private readonly RoleManager<AppRole> _roleManager;

		public TeacherService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenHandler tokenHandler, IHttpContextAccessor httpContextAccessor, ITeacherReadRepository teacherReadRepository, ITeacherWriteRepository teacherWriteRepository, AppDbContext context, RoleManager<AppRole> roleManager, IAppUserService appUserService, IStudentReadRepository studentReadRepository, IStudentWriteRepository studentWriteRepository, IStudentTeacherWriteRepository studentTeacherWriteRepository, IStudentTeacherReadRepository studentTeacherReadRepository, IStudentClassroomWriteRepository studentClassroomWriteRepository, IStudentClassroomReadRepository studentClassroomReadRepository)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_tokenHandler = tokenHandler;
			_httpContextAccessor = httpContextAccessor;
			_teacherReadRepository = teacherReadRepository;
			_teacherWriteRepository = teacherWriteRepository;
			_context = context;
			_roleManager = roleManager;
			_appUserService = appUserService;
			_studentReadRepository = studentReadRepository;
			_studentWriteRepository = studentWriteRepository;
			_studentTeacherWriteRepository = studentTeacherWriteRepository;
			_studentTeacherReadRepository = studentTeacherReadRepository;
			_studentClassroomWriteRepository = studentClassroomWriteRepository;
			_studentClassroomReadRepository = studentClassroomReadRepository;
		}

		public async Task<bool> UpdateTeacherAsync(UpdateTeacher updateTeacher)
		{
			var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
			if (string.IsNullOrEmpty(token))
			{
				throw new UnauthorizedAccessException("Authorization token is missing.");
			}

			var currentTeacherId = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
			if (string.IsNullOrEmpty(currentTeacherId))
			{
				throw new UnauthorizedAccessException("You are not authorized to update this teacher's information");
			}
			var teacher = await _teacherReadRepository.GetByIdAsync(updateTeacher.Id.ToString());
			if (teacher == null)
			{
				throw new UserNotFoundException("Teacher not found");
			}
			teacher.Id = Guid.Parse(updateTeacher.Id);
			teacher.Subject = updateTeacher.Subject;
			teacher.UserId = teacher.UserId;

			var result = _teacherWriteRepository.Update(teacher);
			await _teacherWriteRepository.SaveAsync();

			return result;
		}
		public async Task<IEnumerable<object>> GetAllTeachersAsync()
		{
			var users = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
			if (string.IsNullOrEmpty(users))
			{
				throw new UnauthorizedAccessException("User is not authenticated");
			}

			AppUser user = await _userManager.Users.Include(a => a.Teacher).FirstOrDefaultAsync(a => a.UserName == users);
			if (user == null)
			{
				throw new UserNotFoundException("User not found");
			}

			var teachers = await _teacherReadRepository.GetAll()
			.Include(a => a.User)
			 .Select(a => new
			 {
				 Username = a.User.Name,
				 Subject = a.Subject
			 })
			  .ToListAsync();

			return teachers;
		}
		public async Task<GetByIdTeacher> GetTeacherByUserIdAsync(string userId)
		{
			var users = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
			if (string.IsNullOrEmpty(users))
			{
				throw new UnauthorizedAccessException("User is not authenticated");
			}
			AppUser user = await _userManager.Users.Include(x => x.Teacher).FirstOrDefaultAsync(x => x.UserName == users);
			if (user == null)
			{
				throw new Exception("User not found");
			}
			var teacher = await _teacherReadRepository.GetByUserIdAsync(userId);
			if (teacher == null)
			{
				throw new Exception("Teacher  not found");
			}
			var relatedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == teacher.UserId);
			if (relatedUser == null)
			{
				throw new Exception("Related user not found");
			}
			return new GetByIdTeacher
			{
				Username = teacher.User.Name,
				Gmail = teacher.User.Email,
				Subject = teacher.Subject
			};


		}
		public async Task<bool> AddStudentToTeacherAsync(CreateTeacherStudent createTeacherStudent)
		{
			var isStudentTeacher = await _studentTeacherReadRepository.GetAll().AnyAsync(a => a.TeacherId == Guid.Parse(createTeacherStudent.teacherId) && a.StudentId != Guid.Parse(createTeacherStudent.studentId));

			if (isStudentTeacher)
			{

				StudentTeacher studentTeacher = new StudentTeacher()
				{

					StudentId = Guid.Parse(createTeacherStudent.studentId),
					TeacherId = Guid.Parse(createTeacherStudent.teacherId)
				};


				await _studentTeacherWriteRepository.AddAsnyc(studentTeacher);
				await _studentTeacherWriteRepository.SaveAsync();

				return true;
			}
			throw new UserNotFoundException("The logged in user already exists.");

		}
		public async Task<IEnumerable<object>> GetAllStudentsForTeacherAsync(string teacherId)
		{
			var users = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
			if (string.IsNullOrEmpty(users))
			{
				throw new Exception("User Not Found");
			}
			AppUser user = await _userManager.Users.Include(x => x.Teacher).FirstOrDefaultAsync(x => x.UserName == users);
			if (user == null)
			{
				throw new Exception("User Not Found");
			}

			var students = await _teacherReadRepository.GetAll().Where(a => a.Id == Guid.Parse(teacherId)).Include(a => a.User).ThenInclude(a => new AppUser()
			{
				UserName = a.UserName,
				Email = a.Email
			}).ToListAsync();


			return students;
		}
		public async Task<bool> RemoveStudentFromTeacherAsync(RemoveTeacherStudent removeTeacherStudent)
		{
			var teacherGuid = Guid.Parse(removeTeacherStudent.teacherId);
			var studentGuid = Guid.Parse(removeTeacherStudent.studentId);


			var studentTeacher = await _studentTeacherReadRepository
				.GetAll()
				.FirstOrDefaultAsync(x => x.TeacherId == teacherGuid && x.StudentId == studentGuid);


			if (studentTeacher == null)
			{
				throw new UserNotFoundException("Student not found for the specified teacher.");
			}


			await _studentTeacherWriteRepository.RemoveAsync(studentTeacher.Id.ToString());
			await _studentTeacherWriteRepository.SaveAsync();

			return true;
		}

		public async Task<bool> CreateClassroomAsync(CreateClassroom createClassroom)
		{
			var teacher = await _teacherReadRepository.GetByIdAsync(createClassroom.TeacherId.ToString());
			if (teacher == null)
			{
				throw new UserNotFoundException("Teacher not found");
			}
			var classroom = new Classroom
			{
				Id = Guid.NewGuid(),
				Name = createClassroom.Name,
				Description = createClassroom.Description,
				TeacherId = teacher.Id,
			};

			if (createClassroom.StudentEmails != null && createClassroom.StudentEmails.Any())
			{
				classroom.StudentClassrooms = new List<StudentClassroom>();
				foreach (var email in createClassroom.StudentEmails)
				{
					var student = await _context.Users
						.Include(x => x.Student)
						.FirstOrDefaultAsync(x => x.Email == email);

					if (student?.Student != null)
					{
						var studentClassroom = new StudentClassroom
						{
							StudentId = student.Student.Id,
							ClassroomId = classroom.Id,
						};
						await _studentClassroomWriteRepository.AddAsnyc(studentClassroom);
					}
				}
				await _context.Classrooms.AddAsync(classroom);
				await _context.SaveChangesAsync();
			}

			return true;

		}

		public async Task<bool> AddStudentToClassroomAsync(StudentClass studentClass)
		{
			var classroom = await _context.Classrooms.FirstOrDefaultAsync(x => x.Id.ToString() == studentClass.classroomId);
			if (classroom == null)
			{
				throw new NotFoundException("Classroom not found");
			}
			var student = await _context.Users
				.Include(x => x.Student)
				.FirstOrDefaultAsync(x => x.Email == studentClass.studentEmail);
			if (student?.Student == null)
			{
				throw new UserNotFoundException("Student not found");
			}
			var exist = await _context.StudentClassrooms
				.AnyAsync(x => x.ClassroomId == classroom.Id && x.StudentId == student.Student.Id);
			if (exist)
			{
				throw new InvalidOperationException("Student is already added to the classroom");
			}
			var studentClassroom = new StudentClassroom
			{
				StudentId = student.Student.Id,
				ClassroomId=classroom.Id
			};
			await _studentClassroomWriteRepository.AddAsnyc(studentClassroom);
			await _context.SaveChangesAsync();
			return true;

		}

		public async Task<IEnumerable<object>> GetAllStudentsInClassroomAsync(string classroomId)
		{
		   var classroom=await _context.Classrooms.FirstOrDefaultAsync(x=>x.Id.ToString() == classroomId);
			if(classroom == null)
			{
				throw new NotFoundException("Classroom not found");
			}
			var studentinclassroom = await _context.StudentClassrooms
				.Where(x => x.ClassroomId == classroom.Id)
				.Include(x => x.Student).ToListAsync();

			if (!studentinclassroom.Any())
			{
				throw new UserNotFoundException("Students not found");
			}

			var result = studentinclassroom
				.Where(x => x.Student != null)
				.Select(x => new
				{
					x.Student.Id,
					x.Student.Username,
					x.Student.User.Email
				});
			return result;
		}

		public Task<bool> RemoveStudentFromClassroomAsync(string classroomId, string studentId)
		{
			throw new NotImplementedException();
		}

		public Task<bool> AddCommentToAssignmentAsync(string classroomId, string studentId, string assignmentId, string comment)
		{
			throw new NotImplementedException();
		}
	}
}
