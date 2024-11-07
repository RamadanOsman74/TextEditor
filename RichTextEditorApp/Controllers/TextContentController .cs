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
            var sanitizer = new HtmlSanitizer();

            // You can remove inline event handlers like 'onclick', 'onload', etc.
            sanitizer.AllowedAttributes.Remove("href");  // disallow links
            sanitizer.AllowedAttributes.Remove("script");  // Allow links if needed

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

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string fileName = $"{Path.GetFileNameWithoutExtension(textAndImageDTO.Image.FileName)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(textAndImageDTO.Image.FileName)}";
                string filePath = Path.Combine(uploadFolder, fileName);

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

                textContent.Image = Path.Combine("Images", fileName);
            }

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

            return Content("Created");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTextContent(int id, [FromForm] TextAndImageDTO textAndImageDTO)
        {
            // Find the existing content in the database
            var existingContent = await _context.TextContents.FindAsync(id);
            if (existingContent == null)
            {
                return NotFound("Content not found");
            }

            // Sanitize and update the content
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Remove("href");  
            sanitizer.AllowedAttributes.Remove("script");  
            textAndImageDTO.Content = sanitizer.Sanitize(textAndImageDTO.Content);

           
            existingContent.Content = textAndImageDTO.Content;
            existingContent.UpdatedAt = DateTime.UtcNow; 

            if (textAndImageDTO.Image != null)
            {
                var allowedExtensions = new[] { ".jpg", ".png" };
                var fileExtension = Path.GetExtension(textAndImageDTO.Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Only .jpg and .png are allowed.");
                }

                string uploadFolder = Path.Combine(_environment.WebRootPath, "Images");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string fileName = $"{Path.GetFileNameWithoutExtension(textAndImageDTO.Image.FileName)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(textAndImageDTO.Image.FileName)}";
                string filePath = Path.Combine(uploadFolder, fileName);

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

                if (!string.IsNullOrEmpty(existingContent.Image))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, existingContent.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                existingContent.Image = Path.Combine("Images", fileName);
            }

            try
            {
                _context.Entry(existingContent).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error while updating content: {ex.Message}");
            }

            return Content("Updated"); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTextContent(int id)
        {
            var content = await _context.TextContents.FindAsync(id);
            if (content == null) return NotFound();
            _context.TextContents.Remove(content);
            await _context.SaveChangesAsync();
            return Content("Deleted");
        }
    }
}
