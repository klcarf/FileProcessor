using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Exceptions
{
    public static class ErrorCodes
    {
        public const string ProviderRequired = "ERR_PROVIDER_REQUIRED";
        public const string FileNotFound = "ERR_FILE_NOT_FOUND";
        public const string ChunkCountMismatch = "ERR_CHUNK_COUNT_MISMATCH";
        public const string ProviderNotFound = "ERR_PROVIDER_NOT_FOUND";
        public const string ChunkHashMismatch = "ERR_CHUNK_HASH_MISMATCH";
        public const string FileHashMismatch = "ERR_FILE_HASH_MISMATCH";
    }
}
