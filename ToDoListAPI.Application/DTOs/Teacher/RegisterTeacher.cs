﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListAPI.Application.DTOs.Teacher
{
	public class RegisterTeacher
	{
		public string Name { get; set; }
		public string Surname { get; set; }
		public string Subject { get; set; }
		public string Gmail { get; set; }
		public string Password { get; set; }
	}
}