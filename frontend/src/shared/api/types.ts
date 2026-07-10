export type PaginationRequest = {
  page: number;
  pageSize: number;
};

export type PagedResult<T> = {
  records: T[];
  totalCount: number;
};
