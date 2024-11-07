namespace RichTextEditorApp.Models
{
    public class TextContent
    {
        public int Id { get; set; }
        public string Content { get; set; }  // Stores HTML or plain text content
        public string? Image {  get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
