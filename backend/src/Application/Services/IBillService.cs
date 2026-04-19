using InvoiceManager.Application.Bills;
using InvoiceManager.Application.Common;

namespace InvoiceManager.Application.Services;

public interface IBillService
{
    Task<IReadOnlyCollection<BillCategoryDto>> GetCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<BillCategoryDto?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<BillCategoryDto> CreateCategoryAsync(Guid actingUserId, string actingUsername, CreateBillCategoryRequestDto request, CancellationToken cancellationToken = default);
    Task<BillCategoryDto?> UpdateCategoryAsync(Guid categoryId, Guid actingUserId, string actingUsername, UpdateBillCategoryRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(Guid categoryId, Guid actingUserId, string actingUsername, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ReminderRuleDto>> GetReminderRulesAsync(CancellationToken cancellationToken = default);
    Task<ReminderRuleDto?> GetReminderRuleAsync(Guid reminderRuleId, CancellationToken cancellationToken = default);
    Task<ReminderRuleDto> CreateReminderRuleAsync(Guid actingUserId, string actingUsername, CreateReminderRuleRequestDto request, CancellationToken cancellationToken = default);
    Task<ReminderRuleDto?> UpdateReminderRuleAsync(Guid reminderRuleId, Guid actingUserId, string actingUsername, UpdateReminderRuleRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteReminderRuleAsync(Guid reminderRuleId, Guid actingUserId, string actingUsername, CancellationToken cancellationToken = default);
    Task<PagedResult<BillListItemDto>> GetBillsAsync(BillQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BillListItemDto>> ExportBillsAsync(BillQueryDto query, CancellationToken cancellationToken = default);
    Task<BillReportSummaryDto> GetBillReportSummaryAsync(BillQueryDto query, CancellationToken cancellationToken = default);
    Task<BillDetailDto?> GetBillAsync(Guid billId, CancellationToken cancellationToken = default);
    Task<BillDetailDto> CreateBillAsync(Guid actingUserId, string actingUsername, CreateBillRequestDto request, CancellationToken cancellationToken = default);
    Task<BillDetailDto?> UpdateBillAsync(Guid billId, Guid actingUserId, string actingUsername, UpdateBillRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteBillAsync(Guid billId, Guid actingUserId, string actingUsername, CancellationToken cancellationToken = default);
    Task<BillAttachmentDto?> AddAttachmentAsync(Guid billId, Guid actingUserId, string actingUsername, string originalFileName, string contentType, Stream content, CancellationToken cancellationToken = default);
    Task<Stream?> OpenAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default);
    Task<BillAttachmentDto?> GetAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAttachmentAsync(Guid attachmentId, Guid actingUserId, string actingUsername, CancellationToken cancellationToken = default);
}
