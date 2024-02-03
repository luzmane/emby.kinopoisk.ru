using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model
{
    internal sealed class KpErrorResponse
    {
        public int StatusCode { get; set; }
        public List<string> Message { get; set; }
        public string Error { get; set; }
    }
}
