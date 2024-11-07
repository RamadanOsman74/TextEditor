using Ganss.Xss;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RichTextEditorApp.DTO;
using RichTextEditorApp.Models;
using System;

namespace RichTextEditorApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextContentController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        TextAndImageDTO textAndImageDTO = new TextAndImageDTO();
        TextContent textContent = new TextContent();

        public TextContentController(IWebHostEnvironment environment, AppDbContext context)
        {
            _environment = environment;
            _context = context;
        }
        [HttpGet]
        public IActionResult GetAllTextContent()
        {
            var content = _context.TextContents.ToList();
            return content == null ? NotFound() : Ok(content);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTextContent(int id)
        {
            var content = await _context.TextContents.FindAsync(id);
            return content == null ? NotFound() : Ok(content);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTextContent([FromForm]TextAndImageDTO textAndImageDTO)
        {
            // Create and configure the sanitizer
            var sanitizer = new HtmlSanitizer();

            // You can remove inline event handlers like 'onclick', 'onload', etc.
            sanitizer.AllowedAttributes.Remove("href");  // disallow links
            sanitizer.AllowedAttributes.Remove("script");  // Allow links if needed

            // Sanitize content to remove any dangerous tags like <script>
            textAndImageDTO.Content = sanitizer.Sanitize(textAndImageDTO.Content);

            if (textAndImageDTO.Image != null)
            {
                var allowedExtensions = new[] { ".jpg", ".png" };
                var fileExtension = Path.GetExtension(textAndImageDTO.Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Only .jpg and .png are allowed.");
                }

                string uploadFolder = Path.Combine(_environment.WebRootPath, "Images");

                // Check if directory exists and create it if not
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Create a unique filename with timestamp to avoid overwriting
                string fileName = $"{Path.GetFileNameWithoutExtension(textAndImageDTO.Image.FileName)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(textAndImageDTO.Image.FileName)}";
                string filePath = Path.Combine(uploadFolder, fileName);

                // Save the file on the server
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await textAndImageDTO.Image.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error while saving file: {ex.Message}");
                }

                // Save the relative path to the image in the database
                textContent.Image = Path.Combine("Images", fileName);
            }

            // Save the TextContent to the database
            try
            {
                textContent.Content = textAndImageDTO.Content;
                _context.TextContents.Add(textContent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error while saving text content: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetTextContent), new { id = textContent.Id }, textContent);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTextContent(int id)
        {
            var content = await _context.TextContents.FindAsync(id);
            if (content == null) return NotFound();
            _context.TextContents.Remove(content);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
