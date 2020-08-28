using BWR.ShareKernel.Common;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Threading.Tasks;

namespace BWR.Infrastructure.Common
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext>, IDisposable
where TContext : DbContext, new()
    {
        private readonly TContext _context;
        private bool _disposed;
        private string _errorMessage = string.Empty;
        private DbContextTransaction _objTran;
        private Dictionary<string, object> _repositories;

        public UnitOfWork()
        {
            _context = new TContext();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public TContext Context { get { return _context; } }
        

        public void CreateTransaction()
        {
            _objTran = _context.Database.BeginTransaction();
        }

        public void Commit()
        {
            _objTran.Commit();
        }

        public void Rollback()
        {
            _objTran.Rollback();
            _objTran.Dispose();
        }

        public void Save()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        _errorMessage += string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage) + Environment.NewLine;
                throw new Exception(_errorMessage, dbEx);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    _context.Dispose();
            _disposed = true;
        }

        IGenericRepository<T> IUnitOfWork<TContext>.GenericRepository<T>()
        {
            var type = typeof(T);
            if (_repositories == null)
                _repositories = new Dictionary<string, object>();
            
            if (!_repositories.ContainsKey(type.Name))
            {
                var repositoryType = typeof(GenericRepository<T>);
                var repositoryInstance = Activator.CreateInstance(repositoryType, _context);
                _repositories.Add(type.Name, repositoryInstance);
            }
            return (GenericRepository<T>)_repositories[type.Name];
        }

        public async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        _errorMessage += string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage) + Environment.NewLine;
                throw new Exception(_errorMessage, dbEx);
            }
        }
    }
}