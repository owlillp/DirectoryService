import { apiClient } from "@/src/shared/api/axios-instance";
import { Envelope } from "@/src/shared/api/envelope";
import { Position } from "./types";
import {
  Cursor,
  InfinitePagedResult,
  InfinitePaginationRequest,
} from "@/src/shared/api/types";
import { infiniteQueryOptions } from "@tanstack/react-query";

export type GetPositionsInfiniteRequest = {
  infiniteRequest: InfinitePaginationRequest;
  search?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
};

export const positionsApi = {
  getPositionsInfinite: async (request: GetPositionsInfiniteRequest) => {
    const response = await apiClient.get<
      Envelope<InfinitePagedResult<Position>>
    >("/Positions", { params: request });
    return response.data.result;
  },
};

export const positionsQueryOptions = {
  baseKey: "positions",

  getPositionsInfiniteOptions: ({
    search,
    isActive,
    sortBy,
    sortDirection,
    limit = 10,
  }: {
    search?: string;
    isActive?: boolean;
    sortBy?: string;
    sortDirection?: string;
    limit?: number;
  }) => {
    return infiniteQueryOptions({
      queryKey: [
        positionsQueryOptions.baseKey,
        { search, isActive, sortBy, sortDirection, limit },
      ],
      queryFn: ({ pageParam }) => {
        return positionsApi.getPositionsInfinite({
          search,
          isActive,
          sortBy,
          sortDirection,
          infiniteRequest: {
            limit,
            cursor: pageParam,
          },
        });
      },
      initialPageParam: null as Cursor | null,
      getNextPageParam: (lastPage) =>
        lastPage?.hasNextPage ? lastPage.nextCursor : null,
      select: (data): InfinitePagedResult<Position> => ({
        records: data.pages.flatMap((page) => page?.records ?? []),
        nextCursor: data.pages[data.pages.length - 1]?.nextCursor,
        hasNextPage: data.pages[data.pages.length - 1]?.hasNextPage ?? false,
      }),
    });
  },
};
