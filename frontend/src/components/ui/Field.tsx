import type { InputHTMLAttributes, ReactNode, TextareaHTMLAttributes } from "react";

type FieldProps = {
  label: string;
  error?: string;
  children: ReactNode;
};

export function Field({ label, error, children }: FieldProps) {
  return (
    <label className="field">
      <span className="field-label">{label}</span>
      {children}
      {error ? <span className="field-error">{error}</span> : null}
    </label>
  );
}

export function TextInput(props: InputHTMLAttributes<HTMLInputElement>) {
  return <input {...props} className={`input ${props.className ?? ""}`.trim()} />;
}

export function TextArea(props: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return (
    <textarea
      {...props}
      className={`textarea ${props.className ?? ""}`.trim()}
    />
  );
}
