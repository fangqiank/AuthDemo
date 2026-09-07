namespace AuthDemoAgain.Models
{
    public class Document
    {
        public string Title { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
    }

    public static class MockDocumentStore
    {
        public static List<Document> Documents = new()
        {
            new Document { Title = "admin-notes", Owner = "admin" },
            new Document { Title = "weekly-report", Owner = "user" }
        };
    }
}
