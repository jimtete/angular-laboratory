export interface ApiResponse<TData> {
  statusCode: number;
  message: string;
  data: TData | null;
}

