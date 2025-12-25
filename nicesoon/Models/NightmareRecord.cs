
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SQLite;

using nicesoon.Models;
using Microsoft.EntityFrameworkCore.Storage;
namespace nicesoon.Models
{ 
    public class NightmareRecord : BaseModel<NightmareRecord>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime RecordDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Emotions { get; set; }
        public AnxietyLevel RecordAnxietyLevel { get; set; }
        public string ImagePath { get; set; }
        public bool IsAnalyzed { get; set; }

        [Ignore]
        public string ShortContent => Content?.Length > 100
            ? Content.Substring(0, 100) + "..."
            : Content ?? string.Empty;

        [Ignore]
        public string FormattedDate => RecordDate.ToString("dd.MM.yyyy HH:mm");

        [Ignore]
        public string AnxietyText => RecordAnxietyLevel switch
        {
            AnxietyLevel.Low => "Низкая интенсивность",
            AnxietyLevel.Medium => "Средняя интенсивность",
            AnxietyLevel.High => "Высокая интенсивность",
            _ => "Не указана"
        };

    }
}
