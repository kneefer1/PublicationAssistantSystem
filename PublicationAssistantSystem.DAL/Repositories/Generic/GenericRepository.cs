﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using PublicationAssistantSystem.DAL.Context;

namespace PublicationAssistantSystem.DAL.Repositories.Generic
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> 
        where TEntity : class
    {
        private readonly IPublicationAssistantContext _context;
        private readonly IDbSet<TEntity> _dbSet;

        public GenericRepository(IPublicationAssistantContext context)
        {
            this._context = context;
            this._dbSet = context.Set<TEntity>();
        }
        
        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params string[] navigationProperties)
        {
            IQueryable<TEntity> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            foreach (var navigationProperty in navigationProperties)
                query = query.Include(navigationProperty);
            
            return orderBy != null 
                ? orderBy(query).ToList()
                : query.ToList();
        }

        public virtual TEntity GetByID(object id)
        {
            return _dbSet.Find(id);
        }

        public virtual void Insert(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        //public virtual void Delete(object id)
        //{
        //    TEntity entityToDelete = _dbSet.Find(id);
        //    Delete(entityToDelete);
        //}

        public virtual void Delete(TEntity entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }
    }
}