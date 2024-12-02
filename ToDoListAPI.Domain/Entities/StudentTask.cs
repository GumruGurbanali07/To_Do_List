﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListAPI.Domain.Entities
{
	public class StudentTask
	{
		public Guid StudentId { get; set; }
		public Student Student { get; set; }
		public Guid TaskId { get; set; }
		public Task Task { get; set; }
		public DateTime DeadLine { get; set; }
		public bool IsCompleted { get; set; }
	}
}
