using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ProviderMigration.BlogManager.EFCore.PostgreSql
{
    public partial class Blog
    {
        public Blog()
        {
            BlogAuthor = new HashSet<BlogAuthor>();
            BlogPost = new HashSet<BlogPost>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Category { get; set; }

        public virtual ICollection<BlogAuthor> BlogAuthor { get; set; }
        public virtual ICollection<BlogPost> BlogPost { get; set; }
    }
}
