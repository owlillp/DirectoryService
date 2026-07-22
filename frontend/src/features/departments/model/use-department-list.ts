import { departmentsQueryOptions } from "@/src/entities/departments/api";
import { SearchDepartment } from "@/src/entities/departments/types";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback, useEffect, useRef } from "react";
import { useDebounce } from "use-debounce";
import {
  useDepartmentActive,
  useDepartmentExcludeIds,
  useDepartmentLocationIds,
  useDepartmentPageSize,
  useDepartmentParentId,
  useDepartmentSearch,
  useDepartmentSortBy,
  useDepartmentSortDirection,
} from "./department-list-store";

type UseDepartmentsSearchResult = {
  departments: SearchDepartment[];
  totalCount: number;
  isPending: boolean;
  error: string | null;
  refetch: () => void;
  isFetchingNextPage: boolean;
  cursorRef: RefCallback<HTMLDivElement>;
};

export function useDepartmentsList(
  stateId?: string,
): UseDepartmentsSearchResult {
  const search = useDepartmentSearch(stateId);
  const [debouncedSearch] = useDebounce(search, 300);
  const isActive = useDepartmentActive(stateId);
  const parentId = useDepartmentParentId(stateId);
  const locationIds = useDepartmentLocationIds(stateId);
  const excludeIds = useDepartmentExcludeIds(stateId);
  const sortBy = useDepartmentSortBy(stateId);
  const sortDirection = useDepartmentSortDirection(stateId);
  const pageSize = useDepartmentPageSize(stateId);

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
      search: debouncedSearch,
      isActive,
      parentId,
      locationIds,
      excludeIds,
      sortBy,
      sortDirection,
      pageSize,
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
