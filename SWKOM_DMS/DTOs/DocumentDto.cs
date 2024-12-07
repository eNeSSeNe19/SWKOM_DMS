using System.ComponentModel.DataAnnotations;

namespace SWKOM_DMS.DTOs
{
    public class DocumentDto
    {
        [Required] // FileName is mandatory
        public string FileName { get; set; }

        [Required] // ContentType is mandatory
        public string ContentType { get; set; }

        [Required] // FileContent is mandatory
        public string FileContent { get; set; }

        [Required] // FileType is mandatory
        public string FileType { get; set; }

        public string ?FilePath { get; set; }
    }
}
