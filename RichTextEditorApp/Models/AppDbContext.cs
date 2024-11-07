using Microsoft.EntityFrameworkCore;

namespace RichTextEditorApp.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<TextContent> TextContents { get; set; }
    }
}
