using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ProviderMigration.BlogManager.EFCore.PostgreSql
{
    public partial class BlogPost
    {
        public BlogPost()
        {
            BlogPostMetrics = new HashSet<BlogPostMetrics>();
        }

        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public TimeSpan? ReadTime { get; set; }
        public bool? IsPublic { get; set; }
        public int BlogId { get; set; }
        public long AuthorId { get; set; }

        public virtual Author Author { get; set; }
        public virtual Blog Blog { get; set; }
        public virtual ICollection<BlogPostMetrics> BlogPostMetrics { get; set; }
    }
}
