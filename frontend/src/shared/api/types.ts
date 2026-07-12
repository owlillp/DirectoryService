export type PaginationRequest = {
  page: number;
  pageSize: number;
};

export type PagedResult<T> = {
  records: T[];
  totalCount: number;
};

export type CursorPaginationRequest = {
  cursor?: Cursor;
  limit: number;
};

export type CursorPagedResult<T> = {
  records: T[];
  nextCursor?: Cursor;
  hasNextPage: boolean;
};

export type Cursor = {
  id: string;
  value?: string;
};
