namespace SWKOM_DMS.DTOs
{
    public class DocumentDto
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] FileContent { get; set; }
        public string FileType { get; set; }
    }
}
