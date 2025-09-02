using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Folder
    {
        public Guid Id { get; set; }
        public string RootPath { get; set; }
        public string RootName { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
}
