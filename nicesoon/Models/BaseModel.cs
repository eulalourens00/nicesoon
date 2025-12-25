using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;
using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using nicesoon.Services;
namespace nicesoon.Models
{
    public abstract class BaseModel<T> where T : BaseModel<T>, new()
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public static List<T> GetAll(DatabaseService dbService)
        {
            return dbService.GetAll<T>().Result;
        }

        public static T GetById(DatabaseService dbService, int id)
        {
            return dbService.GetById<T>(id).Result;
        }

        public int Save(DatabaseService dbService)
        {
            return dbService.SaveAsync((T)this).Result;
        }

        public bool Delete(DatabaseService dbService)
        {
            return dbService.DeleteAsync((T)this).Result > 0;
        }
    }
}
