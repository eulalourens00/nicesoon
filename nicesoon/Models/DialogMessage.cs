
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
    [SQLite.Table("DialogMessages")]
    public class DialogMessage
    {
        [SQLite.PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int RecordId { get; set; }

        public string Role { get; set; } // "user", "assistant"
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }

        [Ignore]
        public bool IsUser => Role == "user";

        [Ignore]
        public string DisplayTime => TimeStamp.ToString("HH:mm");
    }
}
