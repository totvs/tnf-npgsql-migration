using System;

namespace BlogManager.Domain
{
    public class Author
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime Birthdate { get; set; }
        public short Ranking { get; set; }
    }
}
