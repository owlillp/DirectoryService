import { positionsQueryOptions } from "@/src/entities/positions/api";
import { Position } from "@/src/entities/positions/types";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";

type UsePositionsResult = {
  positions: Position[];
  nextCursor?: { id: string; value?: string };
  isPending: boolean;
  error: string | null;
  refetch: () => void;
  isFetchingNextPage: boolean;
  cursorRef: RefCallback<HTMLDivElement>;
};

export function usePositionsList(limit = 10): UsePositionsResult {
  const {
    data,
    isPending,
    error,
    refetch,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery({
    ...positionsQueryOptions.getPositionsInfiniteOptions({
      limit: limit,
    }),
  });

  const cursorRef: RefCallback<HTMLDivElement> = useCallback(
    (el) => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage)
            fetchNextPage();
        },
        { threshold: 0.5 },
      );

      if (el) {
        observer.observe(el);
        return () => observer.disconnect();
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage],
  );

  return {
    positions: data?.records ?? [],
    nextCursor: data?.nextCursor,
    isPending,
    error: error?.message ?? null,
    refetch,
    isFetchingNextPage,
    cursorRef,
  };
}
