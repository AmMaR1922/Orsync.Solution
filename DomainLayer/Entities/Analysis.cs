//using DomainLayer.Common;
//using DomainLayer.Enums;
//using System.Text.Json;

//namespace DomainLayer.Entities;

//public class Analysis : BaseEntity
//{
//    public string UserId { get; private set; } = string.Empty;
//    public string TherapeuticArea { get; private set; } = string.Empty;
//    public string Product { get; private set; } = string.Empty;
//    public string Indication { get; private set; } = string.Empty;

//    // ✅ Multi Select Enums (Stored as JSON)
//    public string TargetGeographyJson { get; private set; } = "[]";
//    public string ResearchDepthJson { get; private set; } = "[]";

//    public string Status { get; private set; } = "Processing";

//    // Store complete ML response
//    public string ResponseJson { get; private set; } = "{}";

//    // File IDs
//    public string FileIdsJson { get; private set; } = "[]";

//    private Analysis() { }

//    // ✅ Constructor
//    public Analysis(
//        string userId,
//        string therapeuticArea,
//        string product,
//        string indication,
//        List<TargetGeography> geographies,
//        List<ResearchDepth> researchDepths)
//    {
//        UserId = userId;
//        TherapeuticArea = therapeuticArea;
//        Product = product;
//        Indication = indication;

//        // Serialize enums as JSON
//        TargetGeographyJson = JsonSerializer.Serialize(geographies);
//        ResearchDepthJson = JsonSerializer.Serialize(researchDepths);

//        Status = "Processing";
//        CreatedAt = DateTime.UtcNow;
//    }

//    // =====================================
//    // ✅ Update Response
//    // =====================================
//    public void SetResponse(string responseJson)
//    {
//        ResponseJson = responseJson;
//        Status = "Completed";
//        UpdatedAt = DateTime.UtcNow;
//    }

//    // =====================================
//    // ✅ Store File IDs
//    // =====================================
//    public void SetFileIds(List<Guid> fileIds)
//    {
//        FileIdsJson = JsonSerializer.Serialize(fileIds);
//        UpdatedAt = DateTime.UtcNow;
//    }

//    // =====================================
//    // ✅ Helpers (Optional – أفضل Practice)
//    // =====================================

//    public List<TargetGeography> GetGeographies()
//    {
//        return JsonSerializer.Deserialize<List<TargetGeography>>(TargetGeographyJson)
//               ?? new List<TargetGeography>();
//    }

//    public List<ResearchDepth> GetResearchDepths()
//    {
//        return JsonSerializer.Deserialize<List<ResearchDepth>>(ResearchDepthJson)
//               ?? new List<ResearchDepth>();
//    }
//}using DomainLayer.Common;
using DomainLayer.Common;
using DomainLayer.Enums;
using System.Text.Json;

namespace DomainLayer.Entities;

public class Analysis : BaseEntity
{
    // =========================
    // Basic Info
    // =========================

    public string UserId { get; private set; } = string.Empty;
    public string TherapeuticArea { get; private set; } = string.Empty;
    public string Product { get; private set; } = string.Empty;
    public string Indication { get; private set; } = string.Empty;

    // =========================
    // Multi Select Stored as JSON
    // =========================

    public string TargetGeographyJson { get; private set; } = "[]";
    public string ResearchDepthJson { get; private set; } = "[]";

    // =========================
    // Status + Results
    // =========================

    public string Status { get; private set; } = "Processing";

    public string ResponseJson { get; private set; } = "{}";

    public string FileIdsJson { get; private set; } = "[]";

    // ============================================================
    // Constructor (✔ FIXED – Accepts Lists Not Strings)
    // ============================================================

    private Analysis() { }

    public Analysis(
        string userId,
        string therapeuticArea,
        string product,
        string indication,
        List<TargetGeography> geographies,
        List<ResearchDepth> researchDepths)
    {
        UserId = userId;
        TherapeuticArea = therapeuticArea;
        Product = product;
        Indication = indication;

        // 🔥 Store enums as JSON
        TargetGeographyJson = JsonSerializer.Serialize(geographies);
        ResearchDepthJson = JsonSerializer.Serialize(researchDepths);

        Status = "Processing";
        CreatedAt = DateTime.UtcNow;
    }

    // ============================================================
    // Methods
    // ============================================================

    public void SetResponse(string responseJson)
    {
        ResponseJson = responseJson;
        Status = "Completed";
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFileIds(List<Guid> fileIds)
    {
        FileIdsJson = JsonSerializer.Serialize(fileIds);
        UpdatedAt = DateTime.UtcNow;
    }
}
