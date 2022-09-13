using System;

namespace BlogManager.Domain
{
    public class BlogPost
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public TimeSpan? ReadTime { get; set; }
        public bool IsPublic { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }

        public long AuthorId { get; set; }
        public Author Author { get; set; }
    }
}
