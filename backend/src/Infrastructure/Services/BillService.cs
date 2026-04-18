using InvoiceManager.Application.Bills;
using InvoiceManager.Application.Common;
using InvoiceManager.Application.Services;
using InvoiceManager.Domain.Bills;
using InvoiceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Services;

public sealed class BillService(
    InvoiceManagerDbContext dbContext,
    IFileStorageService fileStorageService,
    IAuditLogService auditLogService) : IBillService
{
    public async Task<IReadOnlyCollection<BillCategoryDto>> GetCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.BillCategories
            .AsNoTracking()
            .Include(x => x.Bills)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.Type)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => MapCategory(x, x.Bills.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<BillCategoryDto?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await dbContext.BillCategories
            .AsNoTracking()
            .Where(x => x.Id == categoryId)
            .Select(x => MapCategory(x, x.Bills.Count))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BillCategoryDto> CreateCategoryAsync(
        Guid actingUserId,
        string actingUsername,
        CreateBillCategoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = request.Name.Trim();
        var duplicateExists = await dbContext.BillCategories.AnyAsync(
            x => x.Type == request.Type && x.Name.ToLower() == normalizedName.ToLower(),
            cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException($"A category named '{normalizedName}' already exists for type {request.Type}.");
        }

        var category = new BillCategory
        {
            Name = normalizedName,
            Type = request.Type,
            Description = request.Description.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            IsSystemDefault = request.IsSystemDefault
        };

        dbContext.BillCategories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            actingUserId,
            actingUsername,
            "bill-category.created",
            "BillCategory",
            category.Id.ToString(),
            $"Created category {category.Name}",
            new { category.Type, category.SortOrder, category.IsActive, category.IsSystemDefault },
            cancellationToken);

        return MapCategory(category, 0);
    }

    public async Task<BillCategoryDto?> UpdateCategoryAsync(
        Guid categoryId,
        Guid actingUserId,
        string actingUsername,
        UpdateBillCategoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var category = await dbContext.BillCategories
            .Include(x => x.Bills)
            .FirstOrDefaultAsync(x => x.Id == categoryId, cancellationToken);

        if (category is null)
        {
            return null;
        }

        var normalizedName = request.Name.Trim();
        var duplicateExists = await dbContext.BillCategories.AnyAsync(
            x => x.Id != categoryId && x.Type == request.Type && x.Name.ToLower() == normalizedName.ToLower(),
            cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException($"A category named '{normalizedName}' already exists for type {request.Type}.");
        }

        category.Name = normalizedName;
        category.Type = request.Type;
        category.Description = request.Description.Trim();
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        category.IsSystemDefault = request.IsSystemDefault;

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            actingUserId,
            actingUsername,
            "bill-category.updated",
            "BillCategory",
            category.Id.ToString(),
            $"Updated category {category.Name}",
            new { category.Type, category.SortOrder, category.IsActive, category.IsSystemDefault },
            cancellationToken);

        return MapCategory(category, category.Bills.Count);
    }

    public async Task<bool> DeleteCategoryAsync(Guid categoryId, Guid actingUserId, string actingUsername, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.BillCategories
            .Include(x => x.Bills)
            .FirstOrDefaultAsync(x => x.Id == categoryId, cancellationToken);

        if (category is null)
        {
            return false;
        }

        if (category.IsSystemDefault || category.Bills.Count > 0)
        {
            throw new InvalidOperationException("System default categories or categories linked to bills cannot be deleted.");
        }

        dbContext.BillCategories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            actingUserId,
            actingUsername,
            "bill-category.deleted",
            "BillCategory",
            category.Id.ToString(),
            $"Deleted category {category.Name}",
            new { category.Type, category.SortOrder },
            cancellationToken);

        return true;
    }

    public async Task<PagedResult<BillListItemDto>> GetBillsAsync(BillQueryDto query, CancellationToken cancellationToken = default)
    {
        var normalizedCustomer = query.Customer?.Trim().ToLowerInvariant();
        var normalizedKeyword = query.Keyword?.Trim().ToLowerInvariant();

        var billQuery = dbContext.Bills
            .AsNoTracking()
            .Include(x => x.BillCategory)
            .Include(x => x.Attachments)
            .AsQueryable();

        if (query.BillType.HasValue)
        {
            billQuery = billQuery.Where(x => x.Type == query.BillType.Value);
        }

        if (query.PaymentStatus.HasValue)
        {
            billQuery = billQuery.Where(x => x.PaymentStatus == query.PaymentStatus.Value);
        }

        if (query.IssueDateFrom.HasValue)
        {
            billQuery = billQuery.Where(x => x.IssueDate >= query.IssueDateFrom.Value);
        }

        if (query.IssueDateTo.HasValue)
        {
            billQuery = billQuery.Where(x => x.IssueDate <= query.IssueDateTo.Value);
        }

        if (query.DueDateFrom.HasValue)
        {
            billQuery = billQuery.Where(x => x.DueDate >= query.DueDateFrom.Value);
        }

        if (query.DueDateTo.HasValue)
        {
            billQuery = billQuery.Where(x => x.DueDate <= query.DueDateTo.Value);
        }

        if (query.PeriodFrom.HasValue)
        {
            billQuery = billQuery.Where(x => x.PeriodEnd >= query.PeriodFrom.Value);
        }

        if (query.PeriodTo.HasValue)
        {
            billQuery = billQuery.Where(x => x.PeriodStart <= query.PeriodTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedCustomer))
        {
            billQuery = billQuery.Where(x => x.CustomerName.ToLower().Contains(normalizedCustomer));
        }

        if (!string.IsNullOrWhiteSpace(normalizedKeyword))
        {
            billQuery = billQuery.Where(x =>
                x.ReferenceNumber.ToLower().Contains(normalizedKeyword) ||
                x.CustomerName.ToLower().Contains(normalizedKeyword) ||
                x.PropertyName.ToLower().Contains(normalizedKeyword) ||
                x.ProviderName.ToLower().Contains(normalizedKeyword) ||
                x.AccountNumber.ToLower().Contains(normalizedKeyword) ||
                x.Keywords.ToLower().Contains(normalizedKeyword) ||
                x.Notes.ToLower().Contains(normalizedKeyword));
        }

        if (query.HasAttachment.HasValue)
        {
            billQuery = query.HasAttachment.Value
                ? billQuery.Where(x => x.Attachments.Any())
                : billQuery.Where(x => !x.Attachments.Any());
        }

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var totalCount = await billQuery.CountAsync(cancellationToken);

        var items = await billQuery
            .OrderBy(x => x.DueDate)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BillListItemDto(
                x.Id,
                x.ReferenceNumber,
                x.Type,
                x.BillCategory != null ? x.BillCategory.Name : x.Type.ToString(),
                x.PaymentStatus,
                x.CustomerName,
                x.PropertyName,
                x.ProviderName,
                x.AccountNumber,
                x.Amount,
                x.Currency,
                x.PeriodStart,
                x.PeriodEnd,
                x.IssueDate,
                x.DueDate,
                x.PaidDate,
                x.Attachments.Count,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<BillListItemDto>(items, totalCount, page, pageSize);
    }

    public async Task<BillDetailDto?> GetBillAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        var bill = await dbContext.Bills
            .AsNoTracking()
            .Include(x => x.BillCategory)
            .Include(x => x.Attachments.OrderByDescending(a => a.UploadedAtUtc))
            .FirstOrDefaultAsync(x => x.Id == billId, cancellationToken);

        return bill is null ? null : MapDetail(bill);
    }

    public async Task<BillDetailDto> CreateBillAsync(Guid actingUserId, string actingUsername, CreateBillRequestDto request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var category = await ResolveCategoryAsync(request.BillCategoryId, request.Type, cancellationToken);
        var bill = new Bill
        {
            ReferenceNumber = request.ReferenceNumber.Trim(),
            Type = request.Type,
            BillCategoryId = request.BillCategoryId,
            PaymentStatus = request.PaymentStatus,
            CustomerName = request.CustomerName.Trim(),
            PropertyName = request.PropertyName.Trim(),
            ProviderName = request.ProviderName.Trim(),
            AccountNumber = request.AccountNumber.Trim(),
            Amount = request.Amount,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            PaidDate = request.PaidDate,
            Notes = request.Notes.Trim(),
            Keywords = request.Keywords.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedByUserId = actingUserId,
            UpdatedByUserId = actingUserId
        };

        dbContext.Bills.Add(bill);
        await dbContext.SaveChangesAsync(cancellationToken);
        bill.BillCategory = category;

        await auditLogService.LogAsync(
            actingUserId,
            actingUsername,
            "bill.created",
            "Bill",
            bill.Id.ToString(),
            $"Created bill {bill.ReferenceNumber}",
            new { bill.Type, bill.PaymentStatus, bill.CustomerName, bill.PropertyName, Category = category.Name },
            cancellationToken);

        return MapDetail(bill);
    }

    public async Task<BillDetailDto?> UpdateBillAsync(Guid billId, Guid actingUserId, string actingUsername, UpdateBillRequestDto request, CancellationToken cancellationToken = default)
    {
        var bill = await dbContext.Bills
            .Include(x => x.BillCategory)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == billId, cancellationToken);

        if (bill is null)
        {
            return null;
        }

        var validatedCategory = await ResolveCategoryAsync(request.BillCategoryId, request.Type, cancellationToken);
        bill.ReferenceNumber = request.ReferenceNumber.Trim();
        bill.Type = request.Type;
        bill.BillCategoryId = validatedCategory.Id;
        bill.BillCategory = validatedCategory;
        bill.PaymentStatus = request.PaymentStatus;
        bill.CustomerName = request.CustomerName.Trim();
        bill.PropertyName = request.PropertyName.Trim();
        bill.ProviderName = request.ProviderName.Trim();
        bill.AccountNumber = request.AccountNumber.Trim();
        bill.Amount = request.Amount;
        bill.Currency = request.Currency.Trim().ToUpperInvariant();
        bill.PeriodStart = request.PeriodStart;
        bill.PeriodEnd = request.PeriodEnd;
        bill.IssueDate = request.IssueDate;
        bill.DueDate = request.DueDate;
        bill.PaidDate = request.PaidDate;
        bill.Notes = request.Notes.Trim();
        bill.Keywords = request.Keywords.Trim();
        bill.UpdatedAtUtc = DateTime.UtcNow;
        bill.UpdatedByUserId = actingUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            actingUserId,
            actingUsername,
            "bill.updated",
            "Bill",
            bill.Id.ToString(),
            $"Updated bill {bill.ReferenceNumber}",
            new { bill.Type, bill.PaymentStatus, bill.CustomerName, bill.PropertyName, Category = validatedCategory.Name },
            cancellationToken);

        return MapDetail(bill);
    }

    public async Task<bool> DeleteBillAsync(Guid billId, Guid actingUserId, string actingUsername, CancellationToken cancellationToken = default)
    {
        var bill = await dbContext.Bills
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == billId, cancellationToken);

        if (bill is null)
        {
            return false;
        }

        foreach (var attachment in bill.Attachments)
        {
            await fileStorageService.DeleteAsync(attachment.StoredFileName, cancellationToken);
        }

        dbContext.Bills.Remove(bill);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            actingUserId,
            actingUsername,
            "bill.deleted",
            "Bill",
            bill.Id.ToString(),
            $"Deleted bill {bill.ReferenceNumber}",
            new { bill.ReferenceNumber, bill.CustomerName },
            cancellationToken);

        return true;
    }

    public async Task<BillAttachmentDto?> AddAttachmentAsync(
        Guid billId,
        Guid actingUserId,
        string actingUsername,
        string originalFileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var bill = await dbContext.Bills.FirstOrDefaultAsync(x => x.Id == billId, cancellationToken);
        if (bill is null)
        {
            return null;
        }

        var fileSize = content.CanSeek ? content.Length : 0;
        var storedFileName = await fileStorageService.SaveAsync(originalFileName, content, cancellationToken);
        var attachment = new BillAttachment
        {
            BillId = billId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            ContentType = ResolveContentType(originalFileName, contentType),
            FileSize = fileSize,
            UploadedByUserId = actingUserId
        };

        dbContext.BillFiles.Add(attachment);
        bill.UpdatedAtUtc = DateTime.UtcNow;
        bill.UpdatedByUserId = actingUserId;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            actingUserId,
            actingUsername,
            "bill.attachment.uploaded",
            "BillAttachment",
            attachment.Id.ToString(),
            $"Uploaded attachment {attachment.OriginalFileName}",
            new { billId, attachment.OriginalFileName, attachment.FileSize },
            cancellationToken);

        return MapAttachment(attachment);
    }

    public async Task<Stream?> OpenAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await dbContext.BillFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == attachmentId, cancellationToken);

        return attachment is null
            ? null
            : await fileStorageService.OpenReadAsync(attachment.StoredFileName, cancellationToken);
    }

    public async Task<BillAttachmentDto?> GetAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await dbContext.BillFiles
            .AsNoTracking()
            .Where(x => x.Id == attachmentId)
            .FirstOrDefaultAsync(cancellationToken);

        return attachment is null ? null : MapAttachment(attachment);
    }

    public async Task<bool> DeleteAttachmentAsync(Guid attachmentId, Guid actingUserId, string actingUsername, CancellationToken cancellationToken = default)
    {
        var attachment = await dbContext.BillFiles.FirstOrDefaultAsync(x => x.Id == attachmentId, cancellationToken);
        if (attachment is null)
        {
            return false;
        }

        await fileStorageService.DeleteAsync(attachment.StoredFileName, cancellationToken);
        dbContext.BillFiles.Remove(attachment);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            actingUserId,
            actingUsername,
            "bill.attachment.deleted",
            "BillAttachment",
            attachment.Id.ToString(),
            $"Deleted attachment {attachment.OriginalFileName}",
            new { attachment.BillId, attachment.OriginalFileName },
            cancellationToken);

        return true;
    }

    private static BillDetailDto MapDetail(Bill bill)
    {
        return new BillDetailDto(
            bill.Id,
            bill.ReferenceNumber,
            bill.Type,
            bill.BillCategoryId,
            bill.BillCategory?.Name ?? bill.Type.ToString(),
            bill.PaymentStatus,
            bill.CustomerName,
            bill.PropertyName,
            bill.ProviderName,
            bill.AccountNumber,
            bill.Amount,
            bill.Currency,
            bill.PeriodStart,
            bill.PeriodEnd,
            bill.IssueDate,
            bill.DueDate,
            bill.PaidDate,
            bill.Notes,
            bill.Keywords,
            bill.CreatedAtUtc,
            bill.UpdatedAtUtc,
            bill.Attachments
                .OrderByDescending(x => x.UploadedAtUtc)
                .Select(MapAttachment)
                .ToList());
    }

    private async Task<BillCategory> ResolveCategoryAsync(Guid categoryId, BillType billType, CancellationToken cancellationToken)
    {
        var category = await dbContext.BillCategories
            .FirstOrDefaultAsync(
                x => x.Id == categoryId,
                cancellationToken);

        if (category is null)
        {
            throw new InvalidOperationException("The selected bill category does not exist.");
        }

        if (!category.IsActive)
        {
            throw new InvalidOperationException("The selected bill category is inactive.");
        }

        if (category.Type != billType)
        {
            throw new InvalidOperationException("The selected bill category does not match the bill type.");
        }

        return category;
    }

    private static BillCategoryDto MapCategory(BillCategory category, int billCount)
    {
        return new BillCategoryDto(
            category.Id,
            category.Name,
            category.Type,
            category.Description,
            category.SortOrder,
            category.IsActive,
            category.IsSystemDefault,
            category.CreatedAtUtc,
            billCount);
    }

    private static BillAttachmentDto MapAttachment(BillAttachment attachment)
    {
        return new BillAttachmentDto(
            attachment.Id,
            attachment.OriginalFileName,
            Path.GetExtension(attachment.OriginalFileName).ToLowerInvariant(),
            attachment.ContentType,
            attachment.FileSize,
            IsPreviewable(attachment.ContentType),
            attachment.UploadedAtUtc);
    }

    private static bool IsPreviewable(string contentType)
        => contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase);

    private static string ResolveContentType(string originalFileName, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) &&
            !string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return contentType;
        }

        return Path.GetExtension(originalFileName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".tif" or ".tiff" => "image/tiff",
            _ => "application/octet-stream"
        };
    }
}
