﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListAPI.Application.Repository;
using ToDoListAPI.Domain.Entities.Common;
using ToDoListAPI.Persistence.Context;

namespace ToDoListAPI.Persistence.Repository
{
	public class ReadRepository<T> : IReadRepository<T> where T : BaseEntity
	{
		private readonly AppDbContext _context;

		public ReadRepository(AppDbContext context)
		{
			_context = context;
		}

		public DbSet<T> Table => _context.Set<T>();
		public IQueryable<T> GetAll()
		{
			return Table.AsNoTracking();
		}

		public async Task<T> GetByIdAsync(string id)
		{
			return await Table.FirstOrDefaultAsync(entity => entity.Id == Guid.Parse(id));
		}
	}
}
