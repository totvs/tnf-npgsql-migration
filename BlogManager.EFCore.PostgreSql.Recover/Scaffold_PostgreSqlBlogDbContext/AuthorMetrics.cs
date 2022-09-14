using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ProviderMigration.BlogManager.EFCore.PostgreSql
{
    public partial class AuthorMetrics
    {
        public int Id { get; set; }
        public decimal AverageWordsPerPost { get; set; }
        public decimal AveragePostsPerMonth { get; set; }
        public float StarRating { get; set; }
        public long AuthorId { get; set; }

        public virtual Author Author { get; set; }
    }
}
