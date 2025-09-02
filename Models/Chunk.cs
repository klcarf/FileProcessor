using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Chunk
    {
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public int Index { get; set; }
        public int Size  { get; set; }
        public string HashSha256 { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
    }
}
