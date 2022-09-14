using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ProviderMigration.BlogManager.EFCore.PostgreSql
{
    public partial class BlogPostMetrics
    {
        public int Id { get; set; }
        public long ViewCount { get; set; }
        public decimal AverageViewCountPerDay { get; set; }
        public Guid PostId { get; set; }

        public virtual BlogPost Post { get; set; }
    }
}
