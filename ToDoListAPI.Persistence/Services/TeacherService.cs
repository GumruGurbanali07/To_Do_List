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

namespace ToDoListAPI.Persistence.Services
{
	public class TeacherService : ITeacherService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly ITokenHandler _tokenHandler;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ITeacherReadRepository _teacherReadRepository;
		private readonly ITeacherWriteRepository _teacherWriteRepository;
		public TeacherService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenHandler tokenHandler, IHttpContextAccessor httpContextAccessor, ITeacherReadRepository teacherReadRepository, ITeacherWriteRepository teacherWriteRepository)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_tokenHandler = tokenHandler;
			_httpContextAccessor = httpContextAccessor;
			_teacherReadRepository = teacherReadRepository;
			_teacherWriteRepository = teacherWriteRepository;
		}

		public async Task<RegisterTeacherResponse> RegisterAsTeacherAsync(RegisterTeacher registerTeacher)
		{
			if (registerTeacher.Password != registerTeacher.ResetPassword)
			{
				return new RegisterTeacherResponse
				{
					Message = "Password and ResetPassword do not match",
					Succeeded = false
				};
			}

			var user = new AppUser
			{
				UserName = new string((registerTeacher.Name + registerTeacher.Surname)
				.Replace(" ", "")
				.Where(x => char.IsLetterOrDigit(x))
				.ToArray()),
				Email = registerTeacher.Gmail,
				Subject = registerTeacher.Subject,
			};

			IdentityResult result = await _userManager.CreateAsync(user, registerTeacher.Password);
			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				throw new FailedRegisterException($"User could not register: {errors}");
			}
			else
			{
				return new RegisterTeacherResponse
				{
					Message = "User registered successfully",
					Succeeded = true
				};
			}

		}
		public async Task<T.Token> LoginAsTeacherAsync(LoginTeacher loginTeacher, int tokenLifetime)
		{
			AppUser user = await _userManager.FindByEmailAsync(loginTeacher.Gmail);
			if (user == null)
			{
				throw new UserNotFoundException("User could not found");
			}
			SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, loginTeacher.Password, false);

			if (result.Succeeded)
			{
				T.Token token = _tokenHandler.CreateAccessToken(tokenLifetime, user);
				return token;
			}

			else
			{
				throw new UserNotFoundException("User could not found");
			}


		}
		public Task<IEnumerable<Teacher>> GetAllTeachersAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Domain.Entities.Task>> GetAssignedTasksForTeacherAsync(string teacherId)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Student>> GetStudentForTeacherAsync(string teacherId)
		{
			throw new NotImplementedException();
		}

		public Task<IDictionary<string, TaskStatus>> GetStudentTasksStatusAsync(string teacherId)
		{
			throw new NotImplementedException();
		}

		public Task<Teacher> GetTeacherByIdAsync(string id)
		{
			throw new NotImplementedException();
		}



		public Task<bool> LogOut()
		{
			throw new NotImplementedException();
		}



		public Task<bool> RemoveStudentFromTeacherAsync(string teacherId, string studentId)
		{
			throw new NotImplementedException();
		}

		public async Task<bool> UpdateTeacherAsync(UpdateTeacher updateTeacher)
		{
			var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
			if (string.IsNullOrEmpty(token))
			{
				throw new UnauthorizedAccessException("Authorization token is missing.");
			}

			var currentTeacherId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
			if (currentTeacherId == null || currentTeacherId != updateTeacher.Id.ToString())
			{
				throw new UnauthorizedAccessException("You are not authorized to update this teacher's information");
			}
			var teacher = await _teacherReadRepository.GetByIdAsync(updateTeacher.Id.ToString());
			if(teacher == null)
			{
				throw new UserNotFoundException("Teacher not found");
			}
			teacher.Name = updateTeacher.Name ?? teacher.Name;
			teacher.Surname=updateTeacher.Surname?? teacher.Surname;
			teacher.Subject=updateTeacher.Subject??teacher.Subject;
			teacher.Gmail=updateTeacher.Gmail?? teacher.Gmail;
			teacher.Password=updateTeacher.Password?? teacher.Password;

			var result= _teacherWriteRepository.Update(teacher);
			return result;

		}
	}
}
