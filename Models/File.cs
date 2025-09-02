using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class File
    {
        public Guid Id { get; set; }
        public Guid? FolderId { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public int ChunkCount { get; set; }
        public string HashSha256 { get; set; } = string.Empty;
        public DateTimeOffset UpdateDate { get; set; }

    }
}
