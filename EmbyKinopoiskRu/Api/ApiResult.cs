namespace EmbyKinopoiskRu.Api
{
    internal sealed class ApiResult<TItem>
    {
        internal TItem Item { get; set; }
        internal bool HasError { get; set; }

        internal ApiResult(TItem item)
        {
            Item = item;
        }
    }
}
