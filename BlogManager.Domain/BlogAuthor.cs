namespace BlogManager.Domain
{
    public class BlogAuthor
    {
        public int Id { get; set; }

        public long AuthorId { get; set; }
        public Author Author { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
