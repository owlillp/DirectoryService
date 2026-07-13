import { locationsQueryOptions } from "@/src/entities/locations/api";
import { Location } from "@/src/entities/locations/types";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback, useEffect, useRef } from "react";

type UseLocationsResult = {
  locations: Location[];
  totalCount: number;
  isPending: boolean;
  error: string | null;
  refetch: () => void;
  isFetchingNextPage: boolean;
  cursorRef: RefCallback<HTMLDivElement>;
};

export function useLocationsList(pageSize = 10): UseLocationsResult {
  const {
    data,
    isPending,
    error,
    refetch,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery({
    ...locationsQueryOptions.getLocationsInfiniteOptions(pageSize),
  });

  const targetRef = useRef<HTMLDivElement | null>(null);

  const cursorRef: RefCallback<HTMLDivElement> = useCallback((el) => {
    targetRef.current = el;
  }, []);

  useEffect(() => {
    const el = targetRef.current;
    if (!el) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage)
          fetchNextPage();
      },
      { threshold: 0.5 },
    );

    observer.observe(el);

    return () => observer.disconnect();
  }, [fetchNextPage, hasNextPage, isFetchingNextPage]);

  return {
    locations: data?.records ?? [],
    totalCount: data?.totalCount ?? 0,
    isPending: isPending,
    error: error?.message ?? null,
    refetch,
    isFetchingNextPage,
    cursorRef,
  };
}
