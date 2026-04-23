namespace Api_blog.Model
{
    public class FacebookToken
    {
        public string UserToken { get; set; }
        public string PageToken { get; set; }
        public DateTime Expiration { get; set; }
    }
}
