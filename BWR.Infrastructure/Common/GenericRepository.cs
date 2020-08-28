using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Infrastructure.Common
{
    public class GenericRepository<T> : IGenericRepository<T>, IDisposable where T : class
    {
        private IDbSet<T> _entities;
        private string _errorMessage = string.Empty;
        private bool _isDisposed;
        public GenericRepository(IUnitOfWork<MainContext> unitOfWork)
        : this(unitOfWork.Context)
        {
        }
        public GenericRepository(MainContext context)
        {
            _isDisposed = false;
            Context = context;
        }
        public MainContext Context { get; set; }
        public virtual IQueryable<T> Table
        {
            get { return Entities; }
        }
        protected virtual IDbSet<T> Entities
        {
            get { return _entities ?? (_entities = Context.Set<T>()); }
        }
        public void Dispose()
        {
            if (Context != null)
                Context.Dispose();
            _isDisposed = true;
        }
        public virtual IEnumerable<T> GetAll()
        {
            return Entities.ToList();
        }
        public virtual T GetById(object id)
        {
            return Entities.Find(id);
        }
        public virtual void Insert(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                Entities.Add(entity);

                if (Context == null || _isDisposed)
                    Context = new MainContext();

            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        _errorMessage += string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage) + Environment.NewLine;
                throw new Exception(_errorMessage, dbEx);
            }
        }
        public void BulkInsert(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                {
                    throw new ArgumentNullException("entities");
                }
                Context.Configuration.AutoDetectChangesEnabled = false;
                Context.Set<T>().AddRange(entities);
                Context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        _errorMessage += string.Format("Property: {0} Error: {1}", validationError.PropertyName,
                        validationError.ErrorMessage) + Environment.NewLine;
                    }
                }
                throw new Exception(_errorMessage, dbEx);
            }
        }
        public virtual void Update(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");
                if (Context == null || _isDisposed)
                    Context = new MainContext();
                SetEntryModified(entity);

            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        _errorMessage += Environment.NewLine + string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                throw new Exception(_errorMessage, dbEx);
            }
        }
        public virtual void Delete(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");
                if (Context == null || _isDisposed)
                    Context = new MainContext();
                Entities.Remove(entity);

            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        _errorMessage += Environment.NewLine + string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                throw new Exception(_errorMessage, dbEx);
            }
        }
        public virtual void SetEntryModified(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        public object Save()
        {
            return this.Context.SaveChanges();
        }

        public IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            return Entities.Where(predicate);
        }

        public void RefershEntity(T entity)
        {
            Context.Entry(entity).Reload();
        }

      
    }
}
