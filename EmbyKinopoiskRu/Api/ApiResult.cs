namespace EmbyKinopoiskRu.Api
{
    internal sealed class ApiResult<TItem>
    {
        public TItem Item { get; set; }
        public bool HasError { get; set; }

        public ApiResult(TItem item)
        {
            Item = item;
        }
    }
}
