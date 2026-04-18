type BadgeTone = "success" | "warning" | "danger" | "neutral";

const toneByLabel: Record<string, BadgeTone> = {
  Valid: "success",
  Paid: "success",
  Pending: "warning",
  Overdue: "danger",
  InvalidSignature: "danger",
  InvalidLicense: "danger",
  Expired: "danger",
  FingerprintMismatch: "danger",
  MissingLicense: "neutral",
  WrongProduct: "danger",
  Error: "danger",
  Active: "success",
  Inactive: "warning",
  Default: "neutral",
  ConfigurationError: "danger",
  Water: "neutral",
  Electricity: "warning",
  Gas: "success",
  Tax: "danger",
};

export function StatusBadge({ label }: { label: string }) {
  const tone = toneByLabel[label] ?? "neutral";
  return <span className={`badge badge-${tone}`}>{label}</span>;
}
