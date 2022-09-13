namespace BlogManager.Domain
{
    public class BlogRating
    {
        public int Id { get; set; }
        public decimal StarRating { get; set; }

        public int BlogId { get; set; }
    }
}
