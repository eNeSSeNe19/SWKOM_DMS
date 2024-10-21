using SWKOM_DMS.Entities;

namespace SWKOM_DMS
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task<Document> GetDocumentByIdAsync(int id);
        Task AddDocumentAsync(Document document);
        Task DeleteDocumentAsync(int id);
    }
}
