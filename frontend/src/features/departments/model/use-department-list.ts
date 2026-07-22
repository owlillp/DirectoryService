import { departmentsQueryOptions } from "@/src/entities/departments/api";
import { SearchDepartment } from "@/src/entities/departments/types";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback, useEffect, useRef } from "react";
import { useDebounce } from "use-debounce";
import { useDepartmentSearch } from "./department-list-store";

type UseDepartmentsSearchResult = {
  departments: SearchDepartment[];
  totalCount: number;
  isPending: boolean;
  error: string | null;
  refetch: () => void;
  isFetchingNextPage: boolean;
  cursorRef: RefCallback<HTMLDivElement>;
};

type UseDepartmentsListParams = {
  search?: string;
  isActive?: boolean;
  parentId?: string;
  locationIds?: string[];
  excludeIds?: string[];
  sortBy?: string;
  sortDirection?: "asc" | "desc";
  pageSize?: number;
  stateId?: string;
};

export function useDepartmentsList(
  params: UseDepartmentsListParams,
): UseDepartmentsSearchResult {
  const search = useDepartmentSearch(params.stateId);
  const [debouncedSearch] = useDebou;

  const {
    data,
    isPending,
    error,
    refetch,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery({
    ...departmentsQueryOptions.searchInfiniteOptions({
      search: params.search,
      isActive: params.isActive,
      parentId: params.parentId,
      locationIds: params.locationIds,
      exludeIds: params.excludeIds,
      sortBy: params.sortBy,
      sortDirection: params.sortDirection,
      pageSize: params.pageSize ?? 10,
    }),
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
    departments: data?.records ?? [],
    totalCount: data?.totalCount ?? 0,
    isPending,
    error: error?.message ?? null,
    refetch,
    isFetchingNextPage,
    cursorRef,
  };
}
