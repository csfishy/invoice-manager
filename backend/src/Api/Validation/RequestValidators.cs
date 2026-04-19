using InvoiceManager.Application.Auth;
using InvoiceManager.Application.Bills;

namespace InvoiceManager.Api.Validation;

internal static class RequestValidators
{
    public static Dictionary<string, string[]> Validate(LoginRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            errors["username"] = ["Username is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = ["Password is required."];
        }

        return errors;
    }

    public static Dictionary<string, string[]> Validate(CreateBillRequestDto request) =>
        ValidateBillCore(
            request.BillCategoryId,
            request.ReferenceNumber,
            request.CustomerName,
            request.PropertyName,
            request.ProviderName,
            request.AccountNumber,
            request.Amount,
            request.Currency,
            request.PeriodStart,
            request.PeriodEnd,
            request.IssueDate,
            request.DueDate);

    public static Dictionary<string, string[]> Validate(UpdateBillRequestDto request) =>
        ValidateBillCore(
            request.BillCategoryId,
            request.ReferenceNumber,
            request.CustomerName,
            request.PropertyName,
            request.ProviderName,
            request.AccountNumber,
            request.Amount,
            request.Currency,
            request.PeriodStart,
            request.PeriodEnd,
            request.IssueDate,
            request.DueDate);

    public static Dictionary<string, string[]> Validate(CreateBillCategoryRequestDto request) =>
        ValidateCategoryCore(request.Name, request.Description, request.SortOrder);

    public static Dictionary<string, string[]> Validate(UpdateBillCategoryRequestDto request) =>
        ValidateCategoryCore(request.Name, request.Description, request.SortOrder);

    public static Dictionary<string, string[]> Validate(CreateReminderRuleRequestDto request) =>
        ValidateReminderRuleCore(request.Name, request.DaysBeforeDue, request.Recipient, request.Channel);

    public static Dictionary<string, string[]> Validate(UpdateReminderRuleRequestDto request) =>
        ValidateReminderRuleCore(request.Name, request.DaysBeforeDue, request.Recipient, request.Channel);

    private static Dictionary<string, string[]> ValidateBillCore(
        Guid billCategoryId,
        string referenceNumber,
        string customerName,
        string propertyName,
        string providerName,
        string accountNumber,
        decimal amount,
        string currency,
        DateOnly periodStart,
        DateOnly periodEnd,
        DateOnly issueDate,
        DateOnly dueDate)
    {
        var errors = new Dictionary<string, string[]>();

        if (billCategoryId == Guid.Empty)
        {
            errors["billCategoryId"] = ["Bill category is required."];
        }

        if (string.IsNullOrWhiteSpace(referenceNumber))
        {
            errors["referenceNumber"] = ["Reference number is required."];
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            errors["customerName"] = ["Customer name is required."];
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            errors["propertyName"] = ["Address or property name is required."];
        }

        if (string.IsNullOrWhiteSpace(providerName))
        {
            errors["providerName"] = ["Provider name is required."];
        }

        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            errors["accountNumber"] = ["Account number is required."];
        }

        if (amount <= 0)
        {
            errors["amount"] = ["Amount must be greater than zero."];
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            errors["currency"] = ["Currency is required."];
        }

        if (periodEnd < periodStart)
        {
            errors["period"] = ["Period end must be on or after period start."];
        }

        if (dueDate < issueDate)
        {
            errors["dueDate"] = ["Due date must be on or after issue date."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateCategoryCore(
        string name,
        string description,
        int sortOrder)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = ["Category name is required."];
        }

        if (name.Trim().Length > 120)
        {
            errors["name"] = ["Category name must be 120 characters or fewer."];
        }

        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 1000)
        {
            errors["description"] = ["Description must be 1000 characters or fewer."];
        }

        if (sortOrder < 0)
        {
            errors["sortOrder"] = ["Sort order must be zero or greater."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateReminderRuleCore(
        string name,
        int daysBeforeDue,
        string recipient,
        string channel)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = ["Reminder rule name is required."];
        }

        if (daysBeforeDue < 0 || daysBeforeDue > 365)
        {
            errors["daysBeforeDue"] = ["Days before due must be between 0 and 365."];
        }

        if (string.IsNullOrWhiteSpace(recipient))
        {
            errors["recipient"] = ["Recipient is required."];
        }

        if (string.IsNullOrWhiteSpace(channel))
        {
            errors["channel"] = ["Channel is required."];
        }

        return errors;
    }
}
