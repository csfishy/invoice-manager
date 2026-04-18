const checklist = [
  "Bill CRUD for water, electricity, gas, and tax invoices",
  "Role-based admin workflows",
  "Offline license inspection and import",
  "Audit log visibility",
  "Windows-friendly self-hosted deployment",
];

export default function HomePage() {
  return (
    <main className="page-shell">
      <section className="hero">
        <p className="eyebrow">Invoice Manager</p>
        <h1>Admin-ready scaffold for a self-hosted billing system.</h1>
        <p className="intro">
          This starter frontend gives the project a working Next.js entrypoint
          while the real screens for bills, licensing, audit logs, and customer
          operations are implemented.
        </p>
      </section>

      <section className="grid">
        <article className="panel">
          <h2>Next build is wired</h2>
          <p>
            The container can build and serve a minimal application immediately,
            so the repository is ready for incremental feature work.
          </p>
        </article>

        <article className="panel">
          <h2>Suggested next pages</h2>
          <ul>
            {checklist.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
      </section>
    </main>
  );
}
