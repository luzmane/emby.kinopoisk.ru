namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model
{
    internal sealed class KpErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
    }
}
