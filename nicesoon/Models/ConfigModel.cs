using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nicesoon.Models
{
    public class ConfigModel
    {
        public ApiKeysModel ApiKeys { get; set; }
    }

    public class ApiKeysModel
    {
        public string OpenRouter { get; set; }
    }
}
