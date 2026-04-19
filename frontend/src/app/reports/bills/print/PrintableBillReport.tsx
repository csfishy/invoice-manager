"use client";

import { useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import type { ReactNode } from "react";
import type { BillReportSummary } from "@/lib/types";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080";

export function PrintableBillReport() {
  const searchParams = useSearchParams();
  const [report, setReport] = useState<BillReportSummary | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token = window.localStorage.getItem("invoice-manager-token");
    if (!token) {
      setError("Missing session token.");
      return;
    }

    const query = searchParams.toString();
    void fetch(`${API_BASE_URL}/api/reports/bills/summary?${query}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      cache: "no-store",
    })
      .then(async (response) => {
        if (!response.ok) {
          throw new Error(`Unable to load report (${response.status}).`);
        }

        return response.json() as Promise<BillReportSummary>;
      })
      .then(setReport)
      .catch((fetchError: Error) => setError(fetchError.message));
  }, [searchParams]);

  return (
    <main style={{ padding: 32, fontFamily: "Georgia, serif", color: "#1b2530" }}>
      <header style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 24 }}>
        <div>
          <p style={{ margin: 0, textTransform: "uppercase", letterSpacing: "0.12em", color: "#9c4f2f" }}>
            Invoice Manager
          </p>
          <h1 style={{ margin: "8px 0" }}>Bill Summary Report</h1>
        </div>

        <button
          onClick={() => window.print()}
          style={{ padding: "10px 16px", borderRadius: 999, border: "1px solid #d8c5b8", background: "#fff" }}
        >
          Print
        </button>
      </header>

      {error ? <p>{error}</p> : null}

      {report ? (
        <>
          <section style={{ display: "grid", gridTemplateColumns: "repeat(3, minmax(0, 1fr))", gap: 16, marginBottom: 24 }}>
            <ReportCard label="Bill Count" value={String(report.billCount)} />
            <ReportCard label="Paid Count" value={String(report.paidCount)} />
            <ReportCard label="Unpaid Count" value={String(report.unpaidCount)} />
            <ReportCard label="Overdue Count" value={String(report.overdueCount)} />
            <ReportCard label="Total Amount" value={formatCurrency(report.totalAmount)} />
            <ReportCard label="Unpaid Amount" value={formatCurrency(report.unpaidAmount)} />
          </section>

          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr>
                <HeaderCell>Reference</HeaderCell>
                <HeaderCell>Type</HeaderCell>
                <HeaderCell>Status</HeaderCell>
                <HeaderCell>Customer</HeaderCell>
                <HeaderCell>Due Date</HeaderCell>
                <HeaderCell>Amount</HeaderCell>
              </tr>
            </thead>
            <tbody>
              {report.items.map((item) => (
                <tr key={item.id}>
                  <BodyCell>{item.referenceNumber}</BodyCell>
                  <BodyCell>{item.type}</BodyCell>
                  <BodyCell>{item.paymentStatus}</BodyCell>
                  <BodyCell>{item.customerName}</BodyCell>
                  <BodyCell>{item.dueDate}</BodyCell>
                  <BodyCell>{formatCurrency(item.amount, item.currency)}</BodyCell>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      ) : null}
    </main>
  );
}

function ReportCard({ label, value }: { label: string; value: string }) {
  return (
    <article style={{ border: "1px solid #d8c5b8", borderRadius: 16, padding: 16 }}>
      <span style={{ display: "block", color: "#5a6875" }}>{label}</span>
      <strong style={{ fontSize: 24 }}>{value}</strong>
    </article>
  );
}

function HeaderCell({ children }: { children: ReactNode }) {
  return (
    <th style={{ textAlign: "left", borderBottom: "1px solid #d8c5b8", padding: "10px 8px" }}>
      {children}
    </th>
  );
}

function BodyCell({ children }: { children: ReactNode }) {
  return (
    <td style={{ borderBottom: "1px solid #eee", padding: "10px 8px" }}>
      {children}
    </td>
  );
}

function formatCurrency(amount: number, currency = "TWD") {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
    maximumFractionDigits: 0,
  }).format(amount);
}
