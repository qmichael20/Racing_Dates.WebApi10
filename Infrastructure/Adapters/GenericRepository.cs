using Application.Abstractions.Data;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Adapters
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        readonly ApplicationDbContext Context;
        readonly DbSet<T> _dataset;

        public GenericRepository(ApplicationDbContext context)
        {
            Context = context;
            _dataset = Context.Set<T>();
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken )
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity), "Entity can not be null");
            await _dataset.AddAsync(entity, cancellationToken);
            return entity;
        }
    }
}
