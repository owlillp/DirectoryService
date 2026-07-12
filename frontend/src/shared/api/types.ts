export type PaginationRequest = {
  page: number;
  pageSize: number;
};

export type PagedResult<T> = {
  records: T[];
  totalCount: number;
};

export type InfinitePaginationRequest = {
  cursor?: Cursor | null;
  limit: number;
};

export type InfinitePagedResult<T> = {
  records: T[];
  nextCursor?: Cursor;
  hasNextPage: boolean;
};

export type Cursor = {
  id: string;
  value?: string;
};
