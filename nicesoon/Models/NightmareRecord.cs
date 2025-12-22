
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
    [SQLite.Table("NightmareRecord")]
    public class NightmareRecord
    {
        [SQLite.PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int UserId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime RecordDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EmotionsJson { get; set; }

        public bool IsAnalyzed { get; set; }

        //  для работы с эмоциями
        [Ignore]
        public List<string> Emotions
        {
            get => string.IsNullOrEmpty(EmotionsJson) ?
                new List<string>() :
                System.Text.Json.JsonSerializer.Deserialize<List<string>>(EmotionsJson);
            set => EmotionsJson = System.Text.Json.JsonSerializer.Serialize(value ?? new List<string>());
        }

        [Ignore]
        public string ShortContent => Content?.Length > 100 ?
            Content.Substring(0, 100) + "..." :
            Content;

        [Ignore]
        public string FormattedDate => RecordDate.ToString("dd.MM.yyyy HH:mm");
    }
}
