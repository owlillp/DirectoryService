import { locationsApi } from "@/src/entities/locations/api";
import { Location } from "@/src/entities/locations/types";
import { useQuery } from "@tanstack/react-query";

type UseLocationsResult = {
  locations: Location[];
  totalCount: number;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
};

export function useLocations(page = 1, pageSize = 10): UseLocationsResult {
  const { data, isLoading, error, refetch } = useQuery({
    queryFn: () =>
      locationsApi.getLocations({ pagination: { page, pageSize } }),
    queryKey: ["locations", { page, pageSize }],
  });

  return {
    locations: data?.records ?? [],
    totalCount: data?.totalCount ?? 0,
    isLoading: isLoading,
    error: error?.message ?? null,
    refetch,
  };
}
