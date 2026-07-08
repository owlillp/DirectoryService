import { locationsQueryOptions } from "@/src/entities/locations/api";
import { Location } from "@/src/entities/locations/types";
import { useQuery } from "@tanstack/react-query";

type UseLocationsResult = {
  locations: Location[];
  totalCount: number;
  isPending: boolean;
  error: string | null;
  refetch: () => void;
};

export function useLocationsList(page = 1, pageSize = 10): UseLocationsResult {
  const { data, isPending, error, refetch } = useQuery(
    locationsQueryOptions.getLocationsOptions({ page, pageSize }),
  );

  return {
    locations: data?.records ?? [],
    totalCount: data?.totalCount ?? 0,
    isPending: isPending,
    error: error?.message ?? null,
    refetch,
  };
}
