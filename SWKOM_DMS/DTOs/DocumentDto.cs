namespace SWKOM_DMS.DTOs
{
    public class DocumentDto
    {
        required public string FileName { get; set; }
        required public string ContentType { get; set; }
        required public byte[] FileContent { get; set; }
        required public string FileType { get; set; } 
    }
}
