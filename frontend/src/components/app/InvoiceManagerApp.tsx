"use client";

import { useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/Button";
import { Field, TextArea, TextInput } from "@/components/ui/Field";
import { Panel } from "@/components/ui/Panel";
import { StatusBadge } from "@/components/ui/StatusBadge";
import { ApiError, apiRequest, downloadFile } from "@/lib/api";
import type {
  AuditLog,
  AuthResponse,
  BillCategory,
  BillDetail,
  BillFormValues,
  BillListItem,
  BillType,
  CategoryFormValues,
  CurrentUser,
  DashboardSummary,
  LicenseFingerprint,
  LicenseRequestCode,
  LicenseStatus,
  PagedResult,
  PaymentStatus,
  UserRole,
} from "@/lib/types";

type View = "dashboard" | "bills" | "categories" | "audit" | "license";
type BillFilters = {
  billType: string;
  paymentStatus: string;
  issueDateFrom: string;
  issueDateTo: string;
  dueDateFrom: string;
  dueDateTo: string;
  periodFrom: string;
  periodTo: string;
  customer: string;
  keyword: string;
  hasAttachment: string;
  page: number;
};

const billTypes: BillType[] = ["Water", "Electricity", "Gas", "Tax"];
const paymentStatuses: PaymentStatus[] = ["Pending", "Paid", "Overdue"];
const attachmentAccept = ".pdf,.png,.jpg,.jpeg,.gif,.bmp,.webp,.tif,.tiff";
const maxAttachmentBytes = 10 * 1024 * 1024;

const emptyBillForm: BillFormValues = {
  type: "Water",
  billCategoryId: "",
  paymentStatus: "Pending",
  referenceNumber: "",
  customerName: "",
  propertyName: "",
  providerName: "",
  accountNumber: "",
  amount: "",
  currency: "TWD",
  periodStart: "",
  periodEnd: "",
  issueDate: "",
  dueDate: "",
  paidDate: "",
  notes: "",
  keywords: "",
};

const emptyCategoryForm: CategoryFormValues = {
  name: "",
  type: "Water",
  description: "",
  sortOrder: "10",
  isActive: true,
  isSystemDefault: false,
};

const emptyFilters: BillFilters = {
  billType: "",
  paymentStatus: "",
  issueDateFrom: "",
  issueDateTo: "",
  dueDateFrom: "",
  dueDateTo: "",
  periodFrom: "",
  periodTo: "",
  customer: "",
  keyword: "",
  hasAttachment: "",
  page: 1,
};

export function InvoiceManagerApp() {
  const [token, setToken] = useState<string | null>(null);
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);
  const [activeView, setActiveView] = useState<View>("dashboard");
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState<string | null>(null);
  const [loginError, setLoginError] = useState<string | null>(null);
  const [pageError, setPageError] = useState<string | null>(null);

  const [loginForm, setLoginForm] = useState({
    username: "admin",
    password: "change_me_now",
  });

  const [dashboard, setDashboard] = useState<DashboardSummary | null>(null);
  const [bills, setBills] = useState<PagedResult<BillListItem> | null>(null);
  const [selectedBill, setSelectedBill] = useState<BillDetail | null>(null);
  const [categories, setCategories] = useState<BillCategory[]>([]);
  const [auditLogs, setAuditLogs] = useState<PagedResult<AuditLog> | null>(null);
  const [licenseStatus, setLicenseStatus] = useState<LicenseStatus | null>(null);
  const [fingerprint, setFingerprint] = useState<LicenseFingerprint | null>(null);
  const [requestCode, setRequestCode] = useState<LicenseRequestCode | null>(null);

  const [filters, setFilters] = useState<BillFilters>(emptyFilters);
  const [billForm, setBillForm] = useState<BillFormValues>(emptyBillForm);
  const [billErrors, setBillErrors] = useState<Record<string, string>>({});
  const [editingBillId, setEditingBillId] = useState<string | null>(null);

  const [categoryForm, setCategoryForm] = useState<CategoryFormValues>(emptyCategoryForm);
  const [categoryErrors, setCategoryErrors] = useState<Record<string, string>>({});
  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null);

  const canEditBills = currentUser?.role === "Admin" || currentUser?.role === "Operator";
  const isAdmin = currentUser?.role === "Admin";

  const activeCategories = useMemo(
    () => categories.filter((category) => category.isActive),
    [categories],
  );

  const billTypeCategories = useMemo(
    () => activeCategories.filter((category) => category.type === billForm.type),
    [activeCategories, billForm.type],
  );

  useEffect(() => {
    const storedToken = window.localStorage.getItem("invoice-manager-token");
    if (!storedToken) {
      setLoading(false);
      return;
    }

    setToken(storedToken);
  }, []);

  useEffect(() => {
    if (!token) {
      setLoading(false);
      setCurrentUser(null);
      return;
    }

    void bootstrap(token);
  }, [token]);

  useEffect(() => {
    if (!billTypeCategories.some((category) => category.id === billForm.billCategoryId)) {
      setBillForm((previous) => ({
        ...previous,
        billCategoryId: billTypeCategories[0]?.id ?? "",
      }));
    }
  }, [billTypeCategories, billForm.billCategoryId]);

  useEffect(() => {
    if (!token || !currentUser) {
      return;
    }

    void refreshActiveView(token, currentUser.role);
  }, [activeView]);

  async function bootstrap(activeToken: string) {
    try {
      setLoading(true);
      setPageError(null);

      const me = await apiRequest<CurrentUser>("/api/me", { token: activeToken });
      setCurrentUser(me);
      setLoginError(null);

      const license = me.role === "Admin" ? await loadLicense(activeToken) : null;
      if (license && !license.isValid) {
        setActiveView("license");
        setPageError(license.message);
        return;
      }

      const loadedCategories = await loadCategories(activeToken, me.role === "Admin");
      await Promise.all([
        loadDashboard(activeToken),
        loadBills(activeToken, 1, emptyFilters),
        me.role === "Admin" ? loadAuditLogs(activeToken) : Promise.resolve(),
      ]);

      setBillForm((previous) => ({
        ...previous,
        billCategoryId:
          loadedCategories.find((category) => category.type === previous.type && category.isActive)?.id ?? "",
      }));
    } catch (error) {
      if (error instanceof ApiError && error.status === 403) {
        setPageError(error.message);
        return;
      }

      window.localStorage.removeItem("invoice-manager-token");
      setToken(null);
      setCurrentUser(null);
      setLoginError("Session expired. Please sign in again.");
    } finally {
      setLoading(false);
    }
  }

  async function refreshActiveView(activeToken: string, role: UserRole) {
    try {
      setPageError(null);

      if (activeView === "license" && role === "Admin") {
        await loadLicense(activeToken);
        return;
      }

      if (activeView === "dashboard") {
        await loadDashboard(activeToken);
      }

      if (activeView === "bills") {
        await Promise.all([
          loadBills(activeToken, filters.page, filters),
          loadCategories(activeToken, false),
        ]);
      }

      if (activeView === "categories" && role === "Admin") {
        await loadCategories(activeToken, true);
      }

      if (activeView === "audit" && role === "Admin") {
        await loadAuditLogs(activeToken);
      }
    } catch (error) {
      if (error instanceof ApiError && error.status === 403 && role === "Admin") {
        setActiveView("license");
      }

      setPageError(error instanceof Error ? error.message : "Unable to load the requested page.");
    }
  }

  async function handleLogin() {
    setLoginError(null);

    try {
      const response = await apiRequest<AuthResponse>("/api/auth/login", {
        method: "POST",
        body: loginForm,
      });

      window.localStorage.setItem("invoice-manager-token", response.accessToken);
      setToken(response.accessToken);
    } catch (error) {
      setLoginError(error instanceof ApiError ? error.message : "Unable to sign in.");
    }
  }

  function handleLogout() {
    window.localStorage.removeItem("invoice-manager-token");
    setToken(null);
    setCurrentUser(null);
    setDashboard(null);
    setBills(null);
    setCategories([]);
    setAuditLogs(null);
    setLicenseStatus(null);
    setFingerprint(null);
    setRequestCode(null);
    setSelectedBill(null);
    setEditingBillId(null);
    setEditingCategoryId(null);
    setFilters(emptyFilters);
    setBillForm(emptyBillForm);
    setCategoryForm(emptyCategoryForm);
  }

  async function loadDashboard(activeToken: string) {
    const response = await apiRequest<DashboardSummary>("/api/dashboard/summary", {
      token: activeToken,
    });

    setDashboard(response);
  }

  async function loadBills(activeToken: string, page: number, activeFilters: BillFilters) {
    const query = new URLSearchParams();
    if (activeFilters.billType) query.set("billType", activeFilters.billType);
    if (activeFilters.paymentStatus) query.set("paymentStatus", activeFilters.paymentStatus);
    if (activeFilters.issueDateFrom) query.set("issueDateFrom", activeFilters.issueDateFrom);
    if (activeFilters.issueDateTo) query.set("issueDateTo", activeFilters.issueDateTo);
    if (activeFilters.dueDateFrom) query.set("dueDateFrom", activeFilters.dueDateFrom);
    if (activeFilters.dueDateTo) query.set("dueDateTo", activeFilters.dueDateTo);
    if (activeFilters.periodFrom) query.set("periodFrom", activeFilters.periodFrom);
    if (activeFilters.periodTo) query.set("periodTo", activeFilters.periodTo);
    if (activeFilters.customer) query.set("customer", activeFilters.customer);
    if (activeFilters.keyword) query.set("keyword", activeFilters.keyword);
    if (activeFilters.hasAttachment) query.set("hasAttachment", activeFilters.hasAttachment);
    query.set("page", String(page));
    query.set("pageSize", "12");

    const response = await apiRequest<PagedResult<BillListItem>>(`/api/bills?${query.toString()}`, {
      token: activeToken,
    });

    setBills(response);
    setFilters((previous) => ({ ...previous, page: response.page }));
  }

  async function loadBillDetail(billId: string) {
    if (!token) {
      return;
    }

    const response = await apiRequest<BillDetail>(`/api/bills/${billId}`, { token });
    setSelectedBill(response);
  }

  async function loadCategories(activeToken: string, includeInactive: boolean) {
    const response = await apiRequest<BillCategory[]>(
      `/api/categories?includeInactive=${includeInactive ? "true" : "false"}`,
      { token: activeToken },
    );

    setCategories(response);
    return response;
  }

  async function loadAuditLogs(activeToken: string) {
    const response = await apiRequest<PagedResult<AuditLog>>("/api/audit-logs?page=1&pageSize=20", {
      token: activeToken,
    });

    setAuditLogs(response);
  }

  async function loadLicense(activeToken: string) {
    const [status, machineFingerprint, nextRequestCode] = await Promise.all([
      apiRequest<LicenseStatus>("/api/license/status", { token: activeToken }),
      apiRequest<LicenseFingerprint>("/api/license/fingerprint", { token: activeToken }),
      apiRequest<LicenseRequestCode>("/api/license/request-code", { token: activeToken }),
    ]);

    setLicenseStatus(status);
    setFingerprint(machineFingerprint);
    setRequestCode(nextRequestCode);
    return status;
  }

  async function handleBillSearch() {
    if (!token) {
      return;
    }

    await loadBills(token, 1, { ...filters, page: 1 });
  }

  function beginCreateBill() {
    setEditingBillId(null);
    setBillErrors({});
    setSelectedBill(null);
    setBillForm({
      ...emptyBillForm,
      billCategoryId: activeCategories.find((category) => category.type === "Water")?.id ?? "",
    });
  }

  function beginEditBill(bill: BillDetail) {
    setEditingBillId(bill.id);
    setBillErrors({});
    setBillForm({
      type: bill.type,
      billCategoryId: bill.billCategoryId,
      paymentStatus: bill.paymentStatus,
      referenceNumber: bill.referenceNumber,
      customerName: bill.customerName,
      propertyName: bill.propertyName,
      providerName: bill.providerName,
      accountNumber: bill.accountNumber,
      amount: String(bill.amount),
      currency: bill.currency,
      periodStart: bill.periodStart,
      periodEnd: bill.periodEnd,
      issueDate: bill.issueDate,
      dueDate: bill.dueDate,
      paidDate: bill.paidDate ?? "",
      notes: bill.notes,
      keywords: bill.keywords,
    });
  }

  async function saveBill() {
    if (!token) {
      return;
    }

    setBillErrors({});

    try {
      const payload = {
        ...billForm,
        amount: Number(billForm.amount),
        paidDate: billForm.paidDate || null,
      };

      if (editingBillId) {
        const updated = await apiRequest<BillDetail>(`/api/bills/${editingBillId}`, {
          method: "PUT",
          token,
          body: payload,
        });

        setSelectedBill(updated);
        setMessage(`Bill ${updated.referenceNumber} updated.`);
      } else {
        const created = await apiRequest<BillDetail>("/api/bills", {
          method: "POST",
          token,
          body: payload,
        });

        setSelectedBill(created);
        setMessage(`Bill ${created.referenceNumber} created.`);
      }

      await Promise.all([
        loadBills(token, 1, emptyFilters),
        loadDashboard(token),
        loadCategories(token, isAdmin === true),
      ]);

      setFilters(emptyFilters);
      setEditingBillId(null);
      beginCreateBill();
    } catch (error) {
      if (error instanceof ApiError && isValidationPayload(error.payload)) {
        setBillErrors(flattenErrors(error.payload.errors));
        return;
      }

      setMessage(error instanceof Error ? error.message : "Unable to save bill.");
    }
  }

  async function deleteBill() {
    if (!token || !selectedBill) {
      return;
    }

    if (!window.confirm(`Delete bill ${selectedBill.referenceNumber}?`)) {
      return;
    }

    await apiRequest(`/api/bills/${selectedBill.id}`, {
      method: "DELETE",
      token,
    });

    setMessage(`Bill ${selectedBill.referenceNumber} deleted.`);
    setSelectedBill(null);
    setEditingBillId(null);
    beginCreateBill();
    await Promise.all([loadBills(token, 1, emptyFilters), loadDashboard(token)]);
  }

  async function uploadAttachment(file: File) {
    if (!token || !selectedBill) {
      return;
    }

    const normalizedName = file.name.toLowerCase();
    const isSupported = attachmentAccept.split(",").some((extension) => normalizedName.endsWith(extension));
    if (!isSupported) {
      setMessage("Only PDF and common image formats are supported.");
      return;
    }

    if (file.size > maxAttachmentBytes) {
      setMessage("File size exceeds the 10 MB limit.");
      return;
    }

    const formData = new FormData();
    formData.append("file", file);

    await apiRequest(`/api/bills/${selectedBill.id}/attachments`, {
      method: "POST",
      token,
      body: formData,
    });

    setMessage(`Uploaded ${file.name}.`);
    await Promise.all([loadBillDetail(selectedBill.id), loadBills(token, filters.page, filters)]);
  }

  async function deleteAttachment(attachmentId: string) {
    if (!token) {
      return;
    }

    await apiRequest(`/api/attachments/${attachmentId}`, {
      method: "DELETE",
      token,
    });

    setMessage("Attachment removed.");

    if (selectedBill) {
      await loadBillDetail(selectedBill.id);
    }
  }

  function beginCreateCategory() {
    setEditingCategoryId(null);
    setCategoryErrors({});
    setCategoryForm(emptyCategoryForm);
  }

  function beginEditCategory(category: BillCategory) {
    setEditingCategoryId(category.id);
    setCategoryErrors({});
    setCategoryForm({
      name: category.name,
      type: category.type,
      description: category.description,
      sortOrder: String(category.sortOrder),
      isActive: category.isActive,
      isSystemDefault: category.isSystemDefault,
    });
  }

  async function saveCategory() {
    if (!token) {
      return;
    }

    setCategoryErrors({});

    try {
      const payload = {
        ...categoryForm,
        sortOrder: Number(categoryForm.sortOrder),
      };

      if (editingCategoryId) {
        await apiRequest<BillCategory>(`/api/categories/${editingCategoryId}`, {
          method: "PUT",
          token,
          body: payload,
        });

        setMessage(`Category ${categoryForm.name} updated.`);
      } else {
        await apiRequest<BillCategory>("/api/categories", {
          method: "POST",
          token,
          body: payload,
        });

        setMessage(`Category ${categoryForm.name} created.`);
      }

      beginCreateCategory();
      await loadCategories(token, true);
    } catch (error) {
      if (error instanceof ApiError && isValidationPayload(error.payload)) {
        setCategoryErrors(flattenErrors(error.payload.errors));
        return;
      }

      setMessage(error instanceof Error ? error.message : "Unable to save category.");
    }
  }

  async function deleteCategory(category: BillCategory) {
    if (!token) {
      return;
    }

    if (!window.confirm(`Delete category ${category.name}?`)) {
      return;
    }

    try {
      await apiRequest(`/api/categories/${category.id}`, {
        method: "DELETE",
        token,
      });

      setMessage(`Category ${category.name} deleted.`);
      beginCreateCategory();
      await loadCategories(token, true);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Unable to delete category.");
    }
  }

  async function importLicense(file: File) {
    if (!token) {
      return;
    }

    const formData = new FormData();
    formData.append("file", file);

    await apiRequest("/api/license/import", {
      method: "POST",
      token,
      body: formData,
    });

    setMessage(`Imported license file ${file.name}.`);
    await loadLicense(token);
  }

  if (loading) {
    return (
      <main className="page-shell">
        <p>Loading application...</p>
      </main>
    );
  }

  if (!token || !currentUser) {
    return (
      <main className="page-shell auth-shell">
        <section className="hero">
          <p className="eyebrow">Invoice Manager</p>
          <h1>Self-hosted bill operations.</h1>
          <p className="intro">
            Sign in to manage water, electricity, gas, and tax bills with role-based access,
            attachments, audit logs, and offline license visibility.
          </p>
        </section>

        <Panel className="auth-panel" title="Sign In">
          <div className="field-grid">
            <Field label="Username">
              <TextInput
                value={loginForm.username}
                onChange={(event) =>
                  setLoginForm((previous) => ({ ...previous, username: event.target.value }))
                }
                autoComplete="username"
              />
            </Field>

            <Field label="Password">
              <TextInput
                type="password"
                value={loginForm.password}
                onChange={(event) =>
                  setLoginForm((previous) => ({ ...previous, password: event.target.value }))
                }
                autoComplete="current-password"
              />
            </Field>
          </div>

          {loginError ? <p className="inline-error">{loginError}</p> : null}

          <div className="toolbar">
            <Button onClick={() => void handleLogin()}>Sign In</Button>
          </div>
        </Panel>
      </main>
    );
  }

  return (
    <main className="page-shell">
      <header className="topbar">
        <div>
          <p className="eyebrow">Invoice Manager</p>
          <h1>Operations Console</h1>
        </div>

        <div className="topbar-actions">
          <div className="user-chip">
            <strong>{currentUser.displayName}</strong>
            <span>
              {currentUser.username} | {currentUser.role}
            </span>
          </div>
          <Button variant="ghost" onClick={handleLogout}>
            Sign Out
          </Button>
        </div>
      </header>

      <nav className="tabs">
        <Button variant={activeView === "dashboard" ? "primary" : "secondary"} onClick={() => setActiveView("dashboard")}>
          Dashboard
        </Button>
        <Button variant={activeView === "bills" ? "primary" : "secondary"} onClick={() => setActiveView("bills")}>
          Bills
        </Button>
        {isAdmin ? (
          <Button variant={activeView === "categories" ? "primary" : "secondary"} onClick={() => setActiveView("categories")}>
            Categories
          </Button>
        ) : null}
        {isAdmin ? (
          <Button variant={activeView === "audit" ? "primary" : "secondary"} onClick={() => setActiveView("audit")}>
            Audit Logs
          </Button>
        ) : null}
        {isAdmin ? (
          <Button variant={activeView === "license" ? "primary" : "secondary"} onClick={() => setActiveView("license")}>
            Licensing
          </Button>
        ) : null}
      </nav>

      {message ? <p className="toast">{message}</p> : null}
      {pageError ? <p className="inline-error">{pageError}</p> : null}

      {activeView === "dashboard" && dashboard ? (
        <section className="stack">
          <div className="metrics-grid">
            <MetricCard label="Total Bills" value={dashboard.totalBills} />
            <MetricCard label="Pending Bills" value={dashboard.pendingBills} />
            <MetricCard label="Overdue Bills" value={dashboard.overdueBills} tone="danger" />
            <MetricCard label="Paid Bills" value={dashboard.paidBills} tone="success" />
          </div>

          <div className="metrics-grid">
            <MetricCard label="Total Amount" value={formatCurrency(dashboard.totalAmount)} />
            <MetricCard label="Pending Amount" value={formatCurrency(dashboard.pendingAmount)} />
            <MetricCard label="Overdue Amount" value={formatCurrency(dashboard.overdueAmount)} tone="danger" />
          </div>

          <div className="two-column">
            <Panel title="Bills by Type">
              <div className="summary-list">
                {dashboard.byType.map((item) => (
                  <div key={item.type} className="summary-row">
                    <div>
                      <strong>{item.type}</strong>
                      <span>{formatCurrency(item.amount)}</span>
                    </div>
                    <StatusBadge label={`${item.count} bills`} />
                  </div>
                ))}
              </div>
            </Panel>

            <Panel title="Upcoming Due Bills">
              <div className="summary-list">
                {dashboard.upcomingDueBills.map((item) => (
                  <div key={item.id} className="summary-row">
                    <div>
                      <strong>{item.referenceNumber}</strong>
                      <span>{item.customerName}</span>
                    </div>
                    <div>
                      <StatusBadge label={item.paymentStatus} />
                      <span>{item.dueDate}</span>
                    </div>
                  </div>
                ))}
              </div>
            </Panel>
          </div>
        </section>
      ) : null}

      {activeView === "bills" ? (
        <section className="stack">
          <Panel
            title="Bill Search"
            actions={
              <div className="toolbar">
                <Button variant="secondary" onClick={() => void handleBillSearch()}>
                  Search
                </Button>
                <Button
                  variant="ghost"
                  onClick={() => {
                    setFilters(emptyFilters);
                    void loadBills(token, 1, emptyFilters);
                  }}
                >
                  Reset
                </Button>
                {canEditBills ? <Button onClick={beginCreateBill}>New Bill</Button> : null}
              </div>
            }
          >
            <div className="field-grid">
              <Field label="Bill Type">
                <select
                  className="input"
                  value={filters.billType}
                  onChange={(event) => setFilters((previous) => ({ ...previous, billType: event.target.value }))}
                >
                  <option value="">All</option>
                  {billTypes.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
              </Field>

              <Field label="Payment Status">
                <select
                  className="input"
                  value={filters.paymentStatus}
                  onChange={(event) =>
                    setFilters((previous) => ({ ...previous, paymentStatus: event.target.value }))
                  }
                >
                  <option value="">All</option>
                  {paymentStatuses.map((status) => (
                    <option key={status} value={status}>
                      {status}
                    </option>
                  ))}
                </select>
              </Field>

              <Field label="Customer">
                <TextInput
                  value={filters.customer}
                  onChange={(event) => setFilters((previous) => ({ ...previous, customer: event.target.value }))}
                />
              </Field>

              <Field label="Keyword">
                <TextInput
                  value={filters.keyword}
                  onChange={(event) => setFilters((previous) => ({ ...previous, keyword: event.target.value }))}
                />
              </Field>

              <Field label="Issue Date From">
                <TextInput
                  type="date"
                  value={filters.issueDateFrom}
                  onChange={(event) =>
                    setFilters((previous) => ({ ...previous, issueDateFrom: event.target.value }))
                  }
                />
              </Field>

              <Field label="Issue Date To">
                <TextInput
                  type="date"
                  value={filters.issueDateTo}
                  onChange={(event) =>
                    setFilters((previous) => ({ ...previous, issueDateTo: event.target.value }))
                  }
                />
              </Field>

              <Field label="Due Date From">
                <TextInput
                  type="date"
                  value={filters.dueDateFrom}
                  onChange={(event) =>
                    setFilters((previous) => ({ ...previous, dueDateFrom: event.target.value }))
                  }
                />
              </Field>

              <Field label="Due Date To">
                <TextInput
                  type="date"
                  value={filters.dueDateTo}
                  onChange={(event) => setFilters((previous) => ({ ...previous, dueDateTo: event.target.value }))}
                />
              </Field>

              <Field label="Period From">
                <TextInput
                  type="date"
                  value={filters.periodFrom}
                  onChange={(event) => setFilters((previous) => ({ ...previous, periodFrom: event.target.value }))}
                />
              </Field>

              <Field label="Period To">
                <TextInput
                  type="date"
                  value={filters.periodTo}
                  onChange={(event) => setFilters((previous) => ({ ...previous, periodTo: event.target.value }))}
                />
              </Field>

              <Field label="Has Attachment">
                <select
                  className="input"
                  value={filters.hasAttachment}
                  onChange={(event) =>
                    setFilters((previous) => ({ ...previous, hasAttachment: event.target.value }))
                  }
                >
                  <option value="">All</option>
                  <option value="true">With Attachments</option>
                  <option value="false">Without Attachments</option>
                </select>
              </Field>
            </div>
          </Panel>

          <div className="two-column">
            <Panel title="Bill Register">
              <div className="table-wrap">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Reference</th>
                      <th>Type</th>
                      <th>Category</th>
                      <th>Status</th>
                      <th>Customer</th>
                      <th>Due Date</th>
                      <th>Amount</th>
                    </tr>
                  </thead>
                  <tbody>
                    {bills?.items.map((bill) => (
                      <tr
                        key={bill.id}
                        className={selectedBill?.id === bill.id ? "is-selected" : ""}
                        onClick={() => void loadBillDetail(bill.id)}
                      >
                        <td>{bill.referenceNumber}</td>
                        <td>{bill.type}</td>
                        <td>{bill.categoryName}</td>
                        <td>
                          <StatusBadge label={bill.paymentStatus} />
                        </td>
                        <td>{bill.customerName}</td>
                        <td>{bill.dueDate}</td>
                        <td>{formatCurrency(bill.amount, bill.currency)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="toolbar split">
                <span>
                  {bills?.totalCount ?? 0} records | page {bills?.page ?? 1}
                </span>
                <div className="toolbar">
                  <Button
                    variant="secondary"
                    disabled={(bills?.page ?? 1) <= 1}
                    onClick={() => void loadBills(token, (bills?.page ?? 1) - 1, filters)}
                  >
                    Previous
                  </Button>
                  <Button
                    variant="secondary"
                    disabled={!bills || bills.page * bills.pageSize >= bills.totalCount}
                    onClick={() => void loadBills(token, (bills?.page ?? 1) + 1, filters)}
                  >
                    Next
                  </Button>
                </div>
              </div>
            </Panel>

            <Panel
              title={editingBillId ? "Edit Bill" : "Create Bill"}
              actions={
                canEditBills ? (
                  <div className="toolbar">
                    <Button variant="secondary" onClick={beginCreateBill}>
                      Clear
                    </Button>
                    <Button onClick={() => void saveBill()}>
                      {editingBillId ? "Update Bill" : "Create Bill"}
                    </Button>
                  </div>
                ) : null
              }
            >
              <div className="field-grid">
                <Field label="Bill Type" error={billErrors.type}>
                  <select
                    className="input"
                    value={billForm.type}
                    onChange={(event) =>
                      setBillForm((previous) => ({
                        ...previous,
                        type: event.target.value as BillType,
                      }))
                    }
                    disabled={!canEditBills}
                  >
                    {billTypes.map((type) => (
                      <option key={type} value={type}>
                        {type}
                      </option>
                    ))}
                  </select>
                </Field>

                <Field label="Category" error={billErrors.billCategoryId}>
                  <select
                    className="input"
                    value={billForm.billCategoryId}
                    onChange={(event) =>
                      setBillForm((previous) => ({
                        ...previous,
                        billCategoryId: event.target.value,
                      }))
                    }
                    disabled={!canEditBills}
                  >
                    <option value="">Select category</option>
                    {billTypeCategories.map((category) => (
                      <option key={category.id} value={category.id}>
                        {category.name}
                      </option>
                    ))}
                  </select>
                </Field>

                <Field label="Payment Status" error={billErrors.paymentStatus}>
                  <select
                    className="input"
                    value={billForm.paymentStatus}
                    onChange={(event) =>
                      setBillForm((previous) => ({
                        ...previous,
                        paymentStatus: event.target.value as PaymentStatus,
                      }))
                    }
                    disabled={!canEditBills}
                  >
                    {paymentStatuses.map((status) => (
                      <option key={status} value={status}>
                        {status}
                      </option>
                    ))}
                  </select>
                </Field>

                <Field label="Reference Number" error={billErrors.referenceNumber}>
                  <TextInput
                    value={billForm.referenceNumber}
                    onChange={(event) =>
                      setBillForm((previous) => ({ ...previous, referenceNumber: event.target.value }))
                    }
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Customer Name" error={billErrors.customerName}>
                  <TextInput
                    value={billForm.customerName}
                    onChange={(event) =>
                      setBillForm((previous) => ({ ...previous, customerName: event.target.value }))
                    }
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Address or Property" error={billErrors.propertyName}>
                  <TextInput
                    value={billForm.propertyName}
                    onChange={(event) =>
                      setBillForm((previous) => ({ ...previous, propertyName: event.target.value }))
                    }
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Provider Name" error={billErrors.providerName}>
                  <TextInput
                    value={billForm.providerName}
                    onChange={(event) =>
                      setBillForm((previous) => ({ ...previous, providerName: event.target.value }))
                    }
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Account Number" error={billErrors.accountNumber}>
                  <TextInput
                    value={billForm.accountNumber}
                    onChange={(event) =>
                      setBillForm((previous) => ({ ...previous, accountNumber: event.target.value }))
                    }
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Amount" error={billErrors.amount}>
                  <TextInput
                    type="number"
                    min="0"
                    step="0.01"
                    value={billForm.amount}
                    onChange={(event) => setBillForm((previous) => ({ ...previous, amount: event.target.value }))}
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Currency" error={billErrors.currency}>
                  <TextInput
                    value={billForm.currency}
                    onChange={(event) => setBillForm((previous) => ({ ...previous, currency: event.target.value }))}
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Period Start" error={billErrors.period}>
                  <TextInput
                    type="date"
                    value={billForm.periodStart}
                    onChange={(event) =>
                      setBillForm((previous) => ({ ...previous, periodStart: event.target.value }))
                    }
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Period End" error={billErrors.period}>
                  <TextInput
                    type="date"
                    value={billForm.periodEnd}
                    onChange={(event) =>
                      setBillForm((previous) => ({ ...previous, periodEnd: event.target.value }))
                    }
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Issue Date">
                  <TextInput
                    type="date"
                    value={billForm.issueDate}
                    onChange={(event) =>
                      setBillForm((previous) => ({ ...previous, issueDate: event.target.value }))
                    }
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Due Date" error={billErrors.dueDate}>
                  <TextInput
                    type="date"
                    value={billForm.dueDate}
                    onChange={(event) => setBillForm((previous) => ({ ...previous, dueDate: event.target.value }))}
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Paid Date">
                  <TextInput
                    type="date"
                    value={billForm.paidDate}
                    onChange={(event) => setBillForm((previous) => ({ ...previous, paidDate: event.target.value }))}
                    disabled={!canEditBills}
                  />
                </Field>

                <Field label="Keywords">
                  <TextInput
                    value={billForm.keywords}
                    onChange={(event) => setBillForm((previous) => ({ ...previous, keywords: event.target.value }))}
                    disabled={!canEditBills}
                  />
                </Field>
              </div>

              <Field label="Notes">
                <TextArea
                  rows={4}
                  value={billForm.notes}
                  onChange={(event) => setBillForm((previous) => ({ ...previous, notes: event.target.value }))}
                  disabled={!canEditBills}
                />
              </Field>
            </Panel>
          </div>

          {selectedBill ? (
            <Panel
              title={`Bill Detail | ${selectedBill.referenceNumber}`}
              actions={
                <div className="toolbar">
                  {canEditBills ? (
                    <Button variant="secondary" onClick={() => beginEditBill(selectedBill)}>
                      Load Into Editor
                    </Button>
                  ) : null}
                  {isAdmin ? (
                    <Button variant="danger" onClick={() => void deleteBill()}>
                      Delete Bill
                    </Button>
                  ) : null}
                </div>
              }
            >
              <div className="detail-grid">
                <DetailItem label="Customer" value={selectedBill.customerName} />
                <DetailItem label="Property" value={selectedBill.propertyName} />
                <DetailItem label="Category" value={selectedBill.categoryName} />
                <DetailItem label="Provider" value={selectedBill.providerName} />
                <DetailItem label="Account" value={selectedBill.accountNumber} />
                <DetailItem label="Amount" value={formatCurrency(selectedBill.amount, selectedBill.currency)} />
                <DetailItem label="Issue Date" value={selectedBill.issueDate} />
                <DetailItem label="Due Date" value={selectedBill.dueDate} />
                <DetailItem label="Type" value={selectedBill.type} />
                <DetailItem label="Payment Status" value={selectedBill.paymentStatus} badge />
              </div>

              <Field label="Notes">
                <TextArea rows={3} readOnly value={selectedBill.notes || "-"} />
              </Field>

              <div className="attachment-block">
                <div className="panel-header">
                  <h3>Attachments</h3>
                  {canEditBills ? (
                    <label className="upload-button">
                      <input
                        type="file"
                        hidden
                        accept={attachmentAccept}
                        onChange={(event) => {
                          const file = event.target.files?.[0];
                          if (file) {
                            void uploadAttachment(file);
                          }
                          event.target.value = "";
                        }}
                      />
                      Upload File
                    </label>
                  ) : null}
                </div>

                <div className="summary-list">
                  {selectedBill.attachments.length === 0 ? (
                    <p className="muted">No attachments uploaded.</p>
                  ) : (
                    selectedBill.attachments.map((attachment) => (
                      <div key={attachment.id} className="summary-row">
                        <div>
                          <strong>{attachment.originalFileName}</strong>
                          <span>
                            {attachment.fileExtension} | {attachment.contentType} | {Math.max(1, Math.round(attachment.fileSize / 1024))} KB
                          </span>
                          <span>
                            {attachment.isPreviewable ? "Preview available" : "Download only"} | uploaded{" "}
                            {new Date(attachment.uploadedAtUtc).toLocaleString()}
                          </span>
                        </div>
                        <div className="toolbar">
                          <Button
                            variant="secondary"
                            onClick={() =>
                              void downloadFile(
                                `/api/attachments/${attachment.id}`,
                                token,
                                attachment.originalFileName,
                              )
                            }
                          >
                            Download
                          </Button>
                          {canEditBills ? (
                            <Button variant="ghost" onClick={() => void deleteAttachment(attachment.id)}>
                              Remove
                            </Button>
                          ) : null}
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </Panel>
          ) : null}
        </section>
      ) : null}

      {activeView === "categories" && isAdmin ? (
        <section className="two-column">
          <Panel
            title="Category Management"
            actions={
              <div className="toolbar">
                <Button variant="secondary" onClick={beginCreateCategory}>
                  Clear
                </Button>
                <Button onClick={() => void saveCategory()}>
                  {editingCategoryId ? "Update Category" : "Create Category"}
                </Button>
              </div>
            }
          >
            <div className="field-grid">
              <Field label="Name" error={categoryErrors.name}>
                <TextInput
                  value={categoryForm.name}
                  onChange={(event) => setCategoryForm((previous) => ({ ...previous, name: event.target.value }))}
                />
              </Field>

              <Field label="Bill Type">
                <select
                  className="input"
                  value={categoryForm.type}
                  onChange={(event) =>
                    setCategoryForm((previous) => ({
                      ...previous,
                      type: event.target.value as BillType,
                    }))
                  }
                >
                  {billTypes.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
              </Field>

              <Field label="Sort Order" error={categoryErrors.sortOrder}>
                <TextInput
                  type="number"
                  min="0"
                  value={categoryForm.sortOrder}
                  onChange={(event) =>
                    setCategoryForm((previous) => ({ ...previous, sortOrder: event.target.value }))
                  }
                />
              </Field>
            </div>

            <Field label="Description" error={categoryErrors.description}>
              <TextArea
                rows={4}
                value={categoryForm.description}
                onChange={(event) =>
                  setCategoryForm((previous) => ({ ...previous, description: event.target.value }))
                }
              />
            </Field>

            <div className="toolbar wrap">
              <label className="upload-button">
                <input
                  type="checkbox"
                  checked={categoryForm.isActive}
                  onChange={(event) =>
                    setCategoryForm((previous) => ({ ...previous, isActive: event.target.checked }))
                  }
                />
                <span style={{ marginLeft: 8 }}>Active</span>
              </label>

              <label className="upload-button">
                <input
                  type="checkbox"
                  checked={categoryForm.isSystemDefault}
                  onChange={(event) =>
                    setCategoryForm((previous) => ({
                      ...previous,
                      isSystemDefault: event.target.checked,
                    }))
                  }
                />
                <span style={{ marginLeft: 8 }}>System Default</span>
              </label>
            </div>
          </Panel>

          <Panel title="Configured Categories">
            <div className="summary-list">
              {categories.map((category) => (
                <article key={category.id} className="summary-row">
                  <div>
                    <strong>{category.name}</strong>
                    <span>
                      {category.type} | sort {category.sortOrder} | {category.billCount} linked bills
                    </span>
                  </div>
                  <div className="toolbar">
                    <StatusBadge label={category.isActive ? "Active" : "Inactive"} />
                    {category.isSystemDefault ? <StatusBadge label="Default" /> : null}
                    <Button variant="secondary" onClick={() => beginEditCategory(category)}>
                      Edit
                    </Button>
                    <Button variant="ghost" onClick={() => void deleteCategory(category)}>
                      Delete
                    </Button>
                  </div>
                </article>
              ))}
            </div>
          </Panel>
        </section>
      ) : null}

      {activeView === "audit" && isAdmin && auditLogs ? (
        <Panel title="Audit Logs">
          <div className="summary-list">
            {auditLogs.items.map((log) => (
              <article key={log.id} className="audit-entry">
                <div className="summary-row">
                  <div>
                    <strong>{log.summary}</strong>
                    <span>
                      {log.username} | {log.action} | {log.entityType}
                    </span>
                  </div>
                  <span>{new Date(log.occurredAtUtc).toLocaleString()}</span>
                </div>
                <pre className="json-box">{log.metadataJson}</pre>
              </article>
            ))}
          </div>
        </Panel>
      ) : null}

      {activeView === "license" && isAdmin && licenseStatus ? (
        <section className="stack">
          <Panel title="License Status">
            {!licenseStatus.isValid ? (
              <p className="inline-error">{licenseStatus.message}</p>
            ) : null}

            <div className="detail-grid">
              <DetailItem label="Status" value={licenseStatus.status} badge />
              <DetailItem label="License ID" value={licenseStatus.licenseId ?? "Not imported"} />
              <DetailItem label="Customer" value={licenseStatus.customerName ?? "Not imported"} />
              <DetailItem label="Issued At" value={licenseStatus.issuedAtUtc ?? "-"} />
              <DetailItem label="Expires At" value={licenseStatus.expiresAtUtc ?? "-"} />
              <DetailItem label="Checked At" value={licenseStatus.checkedAtUtc} />
            </div>

            {requestCode ? (
              <>
                <Field label="Machine Request Code">
                  <TextArea rows={6} readOnly value={requestCode.requestCode} />
                </Field>

                <div className="detail-grid">
                  <DetailItem label="Product" value={requestCode.productName} />
                  <DetailItem label="Machine Name" value={requestCode.machineName} />
                  <DetailItem label="Generated At" value={requestCode.generatedAtUtc} />
                  <DetailItem label="Format" value={requestCode.format} />
                </div>
              </>
            ) : null}

            <Field label="Machine Fingerprint Hash">
              <TextArea rows={3} readOnly value={fingerprint?.fingerprintHash ?? ""} />
            </Field>

            <Field label="Validation Message">
              <TextArea rows={3} readOnly value={licenseStatus.message} />
            </Field>

            <Field label="Activation Guidance">
              <TextArea
                rows={4}
                readOnly
                value={
                  requestCode?.message ??
                  "Generate a request code, send it to the vendor, and import the signed license file returned to you."
                }
              />
            </Field>

            <div className="toolbar">
              <label className="upload-button">
                <input
                  type="file"
                  hidden
                  onChange={(event) => {
                    const file = event.target.files?.[0];
                    if (file) {
                      void importLicense(file);
                    }
                    event.target.value = "";
                  }}
                />
                Import License
              </label>
              {requestCode ? (
                <Button
                  variant="secondary"
                  onClick={() => void navigator.clipboard.writeText(requestCode.requestCode)}
                >
                  Copy Request Code
                </Button>
              ) : null}
              <Button variant="secondary" onClick={() => void loadLicense(token)}>
                Refresh Status
              </Button>
            </div>

            <div className="toolbar wrap">
              {licenseStatus.features.map((feature) => (
                <StatusBadge key={feature} label={feature} />
              ))}
            </div>
          </Panel>
        </section>
      ) : null}
    </main>
  );
}

function MetricCard({
  label,
  value,
  tone = "neutral",
}: {
  label: string;
  value: string | number;
  tone?: "neutral" | "success" | "danger";
}) {
  return (
    <article className={`metric-card metric-${tone}`}>
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  );
}

function DetailItem({
  label,
  value,
  badge = false,
}: {
  label: string;
  value: string;
  badge?: boolean;
}) {
  return (
    <div className="detail-item">
      <span>{label}</span>
      {badge ? <StatusBadge label={value} /> : <strong>{value}</strong>}
    </div>
  );
}

function formatCurrency(amount: number, currency = "TWD") {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
    maximumFractionDigits: 0,
  }).format(amount);
}

function isValidationPayload(payload: unknown): payload is { errors: Record<string, string[]> } {
  return typeof payload === "object" && payload !== null && "errors" in payload;
}

function flattenErrors(errors: Record<string, string[]>) {
  return Object.fromEntries(Object.entries(errors).map(([key, value]) => [key, value.join(", ")]));
}
