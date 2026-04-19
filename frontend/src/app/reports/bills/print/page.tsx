import { Suspense } from "react";
import { PrintableBillReport } from "./PrintableBillReport";

export const dynamic = "force-dynamic";

export default function PrintableBillReportPage() {
  return (
    <Suspense fallback={<main style={{ padding: 32 }}>Loading report...</main>}>
      <PrintableBillReport />
    </Suspense>
  );
}
