const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080";

type RequestOptions = {
  method?: string;
  token?: string | null;
  body?: unknown;
  headers?: HeadersInit;
};

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly payload?: unknown,
  ) {
    super(message);
  }
}

export async function apiRequest<T>(
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const headers = new Headers(options.headers);

  if (options.token) {
    headers.set("Authorization", `Bearer ${options.token}`);
  }

  let body: BodyInit | undefined;
  if (options.body instanceof FormData) {
    body = options.body;
  } else if (options.body !== undefined) {
    headers.set("Content-Type", "application/json");
    body = JSON.stringify(options.body);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? "GET",
    headers,
    body,
    cache: "no-store",
  });

  if (!response.ok) {
    let payload: unknown = undefined;
    try {
      payload = await response.json();
    } catch {
      payload = await response.text();
    }

    const message =
      typeof payload === "object" &&
      payload !== null &&
      "title" in payload &&
      typeof payload.title === "string"
        ? payload.title
        : `Request failed with status ${response.status}`;

    throw new ApiError(message, response.status, payload);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export async function downloadFile(
  path: string,
  token: string,
  fileName: string,
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new ApiError("Unable to download file.", response.status);
  }

  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = fileName;
  document.body.append(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}
