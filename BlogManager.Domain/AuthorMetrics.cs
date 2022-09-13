namespace BlogManager.Domain
{
    public class AuthorMetrics
    {
        public int Id { get; set; }
        public decimal AverageWordsPerPost { get; set; }
        public decimal AveragePostsPerMonth { get; set; }
        public float StarRating { get; set; }

        public long AuthorId { get; set; }
        public Author Author { get; set; }
    }
}
