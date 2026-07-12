import { positionsQueryOptions } from "@/src/entities/positions/api";
import { Position } from "@/src/entities/positions/types";
import { useInfiniteQuery } from "@tanstack/react-query";

type UsePositionsResult = {
  positions: Position[];
  nextCursor?: { id: string; value?: string };
  hasNextPage: boolean;
  isPending: boolean;
  error: string | null;
  refetch: () => void;
};

export function usePositionsList(limit = 10): UsePositionsResult {
  const { data, isPending, error, refetch } = useInfiniteQuery({
    ...positionsQueryOptions.getPositionsInfiniteOptions({
      limit: limit,
    }),
  });

  return {
    positions: data?.records ?? [],
    nextCursor: data?.nextCursor,
    hasNextPage: data?.hasNextPage ?? false,
    isPending,
    error: error?.message ?? null,
    refetch,
  };
}
