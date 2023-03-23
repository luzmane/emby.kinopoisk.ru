namespace EmbyKinopoiskRu.Api
{
    public class ApiResult<TItem>
    {
        public TItem Item { get; init; }
        public bool HasError { get; set; }

        public ApiResult(TItem item)
        {
            Item = item;
        }
    }
}
