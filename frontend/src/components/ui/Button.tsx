import type { ButtonHTMLAttributes, PropsWithChildren } from "react";

type ButtonProps = PropsWithChildren<
  ButtonHTMLAttributes<HTMLButtonElement> & {
    variant?: "primary" | "secondary" | "ghost" | "danger";
  }
>;

export function Button({
  children,
  className = "",
  variant = "primary",
  ...props
}: ButtonProps) {
  return (
    <button
      {...props}
      className={`btn btn-${variant} ${className}`.trim()}
      type={props.type ?? "button"}
    >
      {children}
    </button>
  );
}
