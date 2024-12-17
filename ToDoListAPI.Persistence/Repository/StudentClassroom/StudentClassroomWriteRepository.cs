﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListAPI.Application.Repository;
using ToDoListAPI.Domain.Entities;
using ToDoListAPI.Persistence.Context;

namespace ToDoListAPI.Persistence.Repository
{
	public class StudentClassroomWriteRepository : WriteRepository<StudentClassroom>, IStudentClassroomWriteRepository
	{
		public StudentClassroomWriteRepository(AppDbContext context) : base(context)
		{
		}
	}
}