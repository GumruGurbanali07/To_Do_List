﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListAPI.Application.DTOs.Teacher
{
	public class UpdateTeacher
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Surname { get; set; }
		public string Subject { get; set; }
		public string? NewGmail { get; set; }
		public string? NewPassword { get; set; }
	}
}
