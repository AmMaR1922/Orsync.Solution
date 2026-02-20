using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Entities
{

    public class Analysis : BaseEntity
    {
        public string UserId { get; private set; } = string.Empty;
        public string TherapeuticArea { get; private set; } = string.Empty;
        public string Product { get; private set; } = string.Empty;
        public string Indication { get; private set; } = string.Empty;
        public string Geography { get; private set; } = string.Empty;
        public string ResearchDepth { get; private set; } = string.Empty;

        public string Status { get; private set; } = "Processing";

        // Store complete JSON response
        public string ResponseJson { get; private set; } = "{}";

        // File IDs
        public string FileIdsJson { get; private set; } = "[]";

        private Analysis() { }

        public Analysis(
            string userId,
            string therapeuticArea,
            string product,
            string indication,
            string geography,
            string researchDepth)
        {
            UserId = userId;
            TherapeuticArea = therapeuticArea;
            Product = product;
            Indication = indication;
            Geography = geography;
            ResearchDepth = researchDepth;
            Status = "Processing";
            CreatedAt = DateTime.UtcNow;
        }

        public void SetResponse(string responseJson)
        {
            ResponseJson = responseJson;
            Status = "Completed";
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetFileIds(List<Guid> fileIds)
        {
            FileIdsJson = System.Text.Json.JsonSerializer.Serialize(fileIds);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
