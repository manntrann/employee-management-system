export type ApiClient = <T>(path: string, options?: RequestInit) => Promise<T>
