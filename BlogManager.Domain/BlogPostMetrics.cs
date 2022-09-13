using System;

namespace BlogManager.Domain
{
    public class BlogPostMetrics
    {
        public int Id { get; set; }
        public long ViewCount { get; set; }
        public decimal AverageViewCountPerDay { get; set; }

        public Guid PostId { get; set; }
        public BlogPost Post { get; set; }
    }
}
