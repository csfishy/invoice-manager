export type UserRole = "Viewer" | "Operator" | "Admin";
export type BillType = "Water" | "Electricity" | "Gas" | "Tax";
export type PaymentStatus = "Pending" | "Paid" | "Overdue";

export type AuthResponse = {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  username: string;
  displayName: string;
  role: UserRole;
};

export type CurrentUser = {
  userId: string;
  username: string;
  displayName: string;
  role: UserRole;
};

export type BillListItem = {
  id: string;
  referenceNumber: string;
  type: BillType;
  categoryName: string;
  paymentStatus: PaymentStatus;
  customerName: string;
  propertyName: string;
  providerName: string;
  accountNumber: string;
  amount: number;
  currency: string;
  periodStart: string;
  periodEnd: string;
  issueDate: string;
  dueDate: string;
  paidDate: string | null;
  attachmentCount: number;
  updatedAtUtc: string;
};

export type BillAttachment = {
  id: string;
  originalFileName: string;
  fileExtension: string;
  contentType: string;
  fileSize: number;
  isPreviewable: boolean;
  uploadedAtUtc: string;
};

export type BillDetail = {
  id: string;
  referenceNumber: string;
  type: BillType;
  billCategoryId: string;
  categoryName: string;
  paymentStatus: PaymentStatus;
  customerName: string;
  propertyName: string;
  providerName: string;
  accountNumber: string;
  amount: number;
  currency: string;
  periodStart: string;
  periodEnd: string;
  issueDate: string;
  dueDate: string;
  paidDate: string | null;
  notes: string;
  keywords: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  attachments: BillAttachment[];
};

export type BillCategory = {
  id: string;
  name: string;
  type: BillType;
  description: string;
  sortOrder: number;
  isActive: boolean;
  isSystemDefault: boolean;
  createdAtUtc: string;
  billCount: number;
};

export type PagedResult<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
};

export type DashboardSummary = {
  totalBills: number;
  pendingBills: number;
  overdueBills: number;
  paidBills: number;
  totalAmount: number;
  pendingAmount: number;
  overdueAmount: number;
  byType: { type: BillType; count: number; amount: number }[];
  upcomingDueBills: {
    id: string;
    referenceNumber: string;
    customerName: string;
    type: BillType;
    paymentStatus: PaymentStatus;
    dueDate: string;
    amount: number;
  }[];
};

export type AuditLog = {
  id: string;
  occurredAtUtc: string;
  username: string;
  action: string;
  entityType: string;
  entityId: string;
  summary: string;
  metadataJson: string;
};

export type LicenseStatus = {
  status: string;
  isValid: boolean;
  fingerprintHash: string;
  licenseId: string | null;
  customerName: string | null;
  issuedAtUtc: string | null;
  expiresAtUtc: string | null;
  features: string[];
  checkedAtUtc: string;
  message: string;
  requiresActivation: boolean;
};

export type LicenseFingerprint = {
  fingerprintHash: string;
  generatedAtUtc: string;
};

export type LicenseRequestCode = {
  requestCode: string;
  fingerprintHash: string;
  productName: string;
  machineName: string;
  generatedAtUtc: string;
  format: string;
  message: string;
};

export type BillFormValues = {
  type: BillType;
  billCategoryId: string;
  paymentStatus: PaymentStatus;
  referenceNumber: string;
  customerName: string;
  propertyName: string;
  providerName: string;
  accountNumber: string;
  amount: string;
  currency: string;
  periodStart: string;
  periodEnd: string;
  issueDate: string;
  dueDate: string;
  paidDate: string;
  notes: string;
  keywords: string;
};

export type CategoryFormValues = {
  name: string;
  type: BillType;
  description: string;
  sortOrder: string;
  isActive: boolean;
  isSystemDefault: boolean;
};
