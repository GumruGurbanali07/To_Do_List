﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListAPI.Application.DTOs.Student
{
	public class UpdateStudent
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Surname { get; set; }
		public string Gmail { get; set; }
		public string Password { get; set; }
	}
}
