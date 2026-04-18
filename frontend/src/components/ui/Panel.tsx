import type { PropsWithChildren, ReactNode } from "react";

type PanelProps = PropsWithChildren<{
  title?: string;
  actions?: ReactNode;
  className?: string;
}>;

export function Panel({ title, actions, className = "", children }: PanelProps) {
  return (
    <section className={`panel ${className}`.trim()}>
      {title || actions ? (
        <header className="panel-header">
          <div>
            {title ? <h2>{title}</h2> : null}
          </div>
          {actions ? <div className="panel-actions">{actions}</div> : null}
        </header>
      ) : null}
      {children}
    </section>
  );
}
