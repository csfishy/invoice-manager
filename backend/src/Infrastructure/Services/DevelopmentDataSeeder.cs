using InvoiceManager.Domain.Bills;
using InvoiceManager.Domain.Licensing;
using InvoiceManager.Domain.Users;
using InvoiceManager.Infrastructure.Configuration;
using InvoiceManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvoiceManager.Infrastructure.Services;

public sealed class DevelopmentDataSeeder(
    InvoiceManagerDbContext dbContext,
    IPasswordHasher<AppUser> passwordHasher,
    IOptions<SeedOptions> options,
    ILogger<DevelopmentDataSeeder> logger)
{
    private readonly SeedOptions _options = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var admin = CreateUser(_options.DefaultAdminUsername, "System Administrator", _options.DefaultAdminPassword, UserRole.Admin);
        var operatorUser = CreateUser("operator", "Billing Operator", "operator123!", UserRole.Operator);
        var viewer = CreateUser("viewer", "Read Only Viewer", "viewer123!", UserRole.Viewer);

        dbContext.Users.AddRange(admin, operatorUser, viewer);

        var categories = await dbContext.BillCategories
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            categories =
            [
                new BillCategory
                {
                    Name = "Water Utility",
                    Type = BillType.Water,
                    Description = "Standard municipal or industrial water billing.",
                    SortOrder = 10
                },
                new BillCategory
                {
                    Name = "Electricity Utility",
                    Type = BillType.Electricity,
                    Description = "Electricity bills for offices, plants, and properties.",
                    SortOrder = 20
                },
                new BillCategory
                {
                    Name = "Gas Utility",
                    Type = BillType.Gas,
                    Description = "Natural gas or fuel gas recurring charges.",
                    SortOrder = 30
                },
                new BillCategory
                {
                    Name = "Tax Assessment",
                    Type = BillType.Tax,
                    Description = "Local tax, land tax, or related statutory charges.",
                    SortOrder = 40
                }
            ];

            dbContext.BillCategories.AddRange(categories);
        }

        var waterCategory = categories.First(x => x.Type == BillType.Water);
        var electricityCategory = categories.First(x => x.Type == BillType.Electricity);
        var gasCategory = categories.First(x => x.Type == BillType.Gas);
        var taxCategory = categories.First(x => x.Type == BillType.Tax);

        var sampleBills = new[]
        {
            new Bill
            {
                ReferenceNumber = "WTR-2026-001",
                Type = BillType.Water,
                BillCategoryId = waterCategory.Id,
                PaymentStatus = PaymentStatus.Pending,
                CustomerName = "North Plant",
                PropertyName = "North Plant Building A",
                ProviderName = "City Water Bureau",
                AccountNumber = "W-1001",
                Amount = 18200m,
                Currency = "TWD",
                PeriodStart = new DateOnly(2026, 3, 1),
                PeriodEnd = new DateOnly(2026, 3, 31),
                IssueDate = new DateOnly(2026, 4, 1),
                DueDate = new DateOnly(2026, 4, 25),
                Keywords = "industrial water monthly",
                CreatedByUserId = admin.Id,
                UpdatedByUserId = admin.Id
            },
            new Bill
            {
                ReferenceNumber = "ELE-2026-002",
                Type = BillType.Electricity,
                BillCategoryId = electricityCategory.Id,
                PaymentStatus = PaymentStatus.Overdue,
                CustomerName = "South Office",
                PropertyName = "South Office Tower",
                ProviderName = "National Power Co.",
                AccountNumber = "E-2201",
                Amount = 45200m,
                Currency = "TWD",
                PeriodStart = new DateOnly(2026, 3, 1),
                PeriodEnd = new DateOnly(2026, 3, 31),
                IssueDate = new DateOnly(2026, 4, 2),
                DueDate = new DateOnly(2026, 4, 15),
                Keywords = "office electricity urgent",
                CreatedByUserId = admin.Id,
                UpdatedByUserId = admin.Id
            },
            new Bill
            {
                ReferenceNumber = "GAS-2026-003",
                Type = BillType.Gas,
                BillCategoryId = gasCategory.Id,
                PaymentStatus = PaymentStatus.Paid,
                CustomerName = "Warehouse East",
                PropertyName = "Warehouse East Block 3",
                ProviderName = "Metro Gas",
                AccountNumber = "G-3305",
                Amount = 9700m,
                Currency = "TWD",
                PeriodStart = new DateOnly(2026, 3, 1),
                PeriodEnd = new DateOnly(2026, 3, 31),
                IssueDate = new DateOnly(2026, 4, 3),
                DueDate = new DateOnly(2026, 4, 18),
                PaidDate = new DateOnly(2026, 4, 10),
                Keywords = "gas paid warehouse",
                CreatedByUserId = admin.Id,
                UpdatedByUserId = admin.Id
            },
            new Bill
            {
                ReferenceNumber = "TAX-2026-004",
                Type = BillType.Tax,
                BillCategoryId = taxCategory.Id,
                PaymentStatus = PaymentStatus.Pending,
                CustomerName = "Corporate HQ",
                PropertyName = "Corporate HQ Campus",
                ProviderName = "Local Tax Office",
                AccountNumber = "T-8877",
                Amount = 124000m,
                Currency = "TWD",
                PeriodStart = new DateOnly(2026, 1, 1),
                PeriodEnd = new DateOnly(2026, 3, 31),
                IssueDate = new DateOnly(2026, 4, 5),
                DueDate = new DateOnly(2026, 5, 1),
                Keywords = "quarterly tax head office",
                CreatedByUserId = admin.Id,
                UpdatedByUserId = admin.Id
            }
        };

        dbContext.Bills.AddRange(sampleBills);

        dbContext.PaymentRecords.Add(new PaymentRecord
        {
            BillId = sampleBills[2].Id,
            AmountPaid = sampleBills[2].Amount,
            PaidOn = sampleBills[2].PaidDate ?? new DateOnly(2026, 4, 10),
            PaymentMethod = "BankTransfer",
            ReferenceNumber = "PAY-GAS-2026-003",
            Note = "Settled in full by finance team.",
            RecordedByUserId = admin.Id
        });

        dbContext.ReminderRules.AddRange(
            new ReminderRule
            {
                Name = "Water Due Reminder",
                BillCategoryId = waterCategory.Id,
                BillType = BillType.Water,
                DaysBeforeDue = 7,
                Recipient = "finance-water@example.local",
                Channel = "InApp"
            },
            new ReminderRule
            {
                Name = "Tax Escalation Reminder",
                BillCategoryId = taxCategory.Id,
                BillType = BillType.Tax,
                DaysBeforeDue = 14,
                Recipient = "finance-tax@example.local",
                Channel = "InApp"
            });

        dbContext.LicenseBindings.Add(new LicenseBinding
        {
            LicenseId = "DEV-LICENSE-001",
            CustomerName = "Development Environment",
            MachineFingerprintHash = "DEVELOPMENT-FINGERPRINT-HASH",
            BindingStatus = "NotActivated",
            BoundAtUtc = DateTime.UtcNow,
            FeaturesJson = "[\"Dashboard\",\"Bills\",\"Attachments\"]"
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded development users, categories, bills, reminders, payment records, and license bindings.");
    }

    private AppUser CreateUser(string username, string displayName, string password, UserRole role)
    {
        var user = new AppUser
        {
            Username = username,
            DisplayName = displayName,
            Role = role
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);
        return user;
    }
}
