namespace RichTextEditorApp.DTO
{
    public class TextAndImageDTO
    {
        public string Content { get; set; }
        public IFormFile? Image { get; set; }
    }
}
