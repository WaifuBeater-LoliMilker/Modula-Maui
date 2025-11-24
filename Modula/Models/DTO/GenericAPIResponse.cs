using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Modula.Models.DTO
{
    public class GenericAPIResponse<T>
    {
        public int status { get; set; } = 0;
        public string message { get; set; } = "";
        public List<T>? data { get; set; }
        public string? error { get; set; }
    }
}
