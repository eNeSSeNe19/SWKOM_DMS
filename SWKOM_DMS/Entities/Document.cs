using System;
using System.ComponentModel.DataAnnotations;

namespace SWKOM_DMS.Entities
{
    public class Document
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "FileName is required")]
        [StringLength(255, ErrorMessage = "FileName cannot exceed 255 characters")]
        public string FileName { get; set; }

        [Required(ErrorMessage = "FileType is required")]
        public string FileType { get; set; }

        [Required(ErrorMessage = "FileSize is required")]
        [Range(0, int.MaxValue, ErrorMessage = "FileSize must be a positive number")]
        public long FileSize { get; set; }

        [Required(ErrorMessage = "UploadDate is required")]
        public DateTime UploadDate { get; set; }

        // Add these missing properties
        [Required(ErrorMessage = "ContentType is required")]
        public string ContentType { get; set; }

        [Required(ErrorMessage = "FileContent is required")]
        public byte[] FileContent { get; set; }
    }
}
