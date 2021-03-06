﻿using System;
using System.Linq;
using System.Reflection;
using log4net;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class TagRepository : ITagRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public TagRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public Tag Create(Tag model)
        {
            try
            {
                this.Context.Tag.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }

        public Tag Get(int tagId)
        {
            try
            {
                return this.Context.Tag.FirstOrDefault(x => x.TagId == tagId);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public Tag Get(string key)
        {
            try
            {
                return this.Context.Tag.FirstOrDefault(x => x.Key == key);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Update(Tag model)
        {
            try
            {
                this.Context.Tag.Update(model);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Delete(int tagId)
        {
            try
            {
                var entry = this.Context.Tag.FirstOrDefault(x => x.TagId == tagId);

                this.Context.Tag.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                return false;
            }
        }
    }
}
