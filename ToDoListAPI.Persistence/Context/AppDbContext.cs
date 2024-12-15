﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ToDoListAPI.Domain.Entities;
using ToDoListAPI.Domain.Entities.Common;
using ToDoListAPI.Domain.Entities.Identity;
using Task = ToDoListAPI.Domain.Entities.Task;

namespace ToDoListAPI.Persistence.Context
{
	public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
	{
		public AppDbContext(DbContextOptions options) : base(options)
		{

		}
		public DbSet<Teacher> Teachers { get; set; }
		public DbSet<Student> Students { get; set; }
		public DbSet<Task> Tasks { get; set; }
		public DbSet<Grade> Grades { get; set; }
		public DbSet<StudentTask> StudentTasks { get; set; }
		public DbSet<StudentTeacher> StudentTeachers { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);


			builder.Entity<StudentTeacher>()
				.HasKey(st => new { st.StudentId, st.TeacherId});

			builder.Entity<StudentTeacher>()
				.HasOne(x => x.Student)
				.WithMany(x => x.StudentTeachers)
				.HasForeignKey(x => x.StudentId);

			builder.Entity<StudentTeacher>()
				.HasOne(y => y.Teacher)
				.WithMany(y => y.StudentTeachers)
				.HasForeignKey(y => y.TeacherId);

		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			var entries = ChangeTracker.Entries();
			foreach (var entry in entries)
			{
				if (entry.Entity is BaseEntity entity)
				{
					switch (entry.State)
					{
						case EntityState.Added:
							entity.CreatedDate = DateTime.UtcNow;
							break;
						case EntityState.Modified:
							entity.UpdatedDate = DateTime.UtcNow;
							break;
					}
				}
			}
			return await base.SaveChangesAsync(cancellationToken);
		}
	}
}
