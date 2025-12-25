
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
    public class User
    {
        public int Id { get; set; }

        public string Phone { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        [Ignore]
        public bool IsAuthenticated { get; set; }

        [Ignore]
        public string DisplayPhone => Phone?.Length > 10 ?
            $"+7 ({Phone.Substring(2, 3)}) {Phone.Substring(5, 3)}-{Phone.Substring(8, 2)}-{Phone.Substring(10, 2)}" :
            Phone;
    }
    
}
