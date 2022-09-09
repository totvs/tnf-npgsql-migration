namespace BlogManager.Domain
{
    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public BlogCategory Category { get; set; }
    }

    public enum BlogCategory
    {
        Tecnology,
        Cooking,
        Travel
    }
}
