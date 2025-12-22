
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SQLite;

namespace nicesoon.Models
{
    
        [SQLite.Table("Users")]
        public class User
        {
            [SQLite.PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Phone { get; set; }
            public string Username { get; set; }
            public string PasswordHash { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? LastLogin { get; set; }

            // Для интерфейса с телефоном
            [Ignore]
            public string DisplayPhone => Phone?.Length > 10 ?
                $"+7 ({Phone.Substring(2, 3)}) {Phone.Substring(5, 3)}-{Phone.Substring(8, 2)}-{Phone.Substring(10, 2)}" :
                Phone;
        }
    
}
