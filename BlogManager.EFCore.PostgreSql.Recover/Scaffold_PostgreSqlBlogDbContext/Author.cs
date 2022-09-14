using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ProviderMigration.BlogManager.EFCore.PostgreSql
{
    public partial class Author
    {
        public Author()
        {
            AuthorMetrics = new HashSet<AuthorMetrics>();
            BlogAuthor = new HashSet<BlogAuthor>();
            BlogPost = new HashSet<BlogPost>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime Birthdate { get; set; }
        public short Ranking { get; set; }

        public virtual ICollection<AuthorMetrics> AuthorMetrics { get; set; }
        public virtual ICollection<BlogAuthor> BlogAuthor { get; set; }
        public virtual ICollection<BlogPost> BlogPost { get; set; }
    }
}
