using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class DBStorage
    {
        public Guid Id { get; set; }

        public String FileId { get; set; }

        public int ChunkOrder { get; set; }

        public byte[] ChunkData { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
