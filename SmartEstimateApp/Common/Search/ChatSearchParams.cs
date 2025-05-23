namespace Common.Search
{
    public class ChatSearchParams : BaseSearchParams
    {
        public long? UserId { get; set; }

        public ChatSearchParams(long? userId = null)
        {
            UserId = userId;
        }
    }
}
