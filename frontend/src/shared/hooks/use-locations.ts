import { useState, useEffect, useRef } from "react";
import axios from "axios";
import { locationsApi } from "@/src/entities/locations/api";
import { Location } from "@/src/entities/locations/types";

type UseLocationsResult = {
  locations: Location[];
  totalCount: number;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
};

export function useLocations(pageSize = 10): UseLocationsResult {
  const [locations, setLocations] = useState<Location[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);

  const loadLocations = (signal: AbortSignal) => {
    return locationsApi
      .getLocations({ pagination: { page: 1, pageSize }, signal })
      .then((result) => {
        setLocations(result?.records ?? []);
        setTotalCount(result?.totalCount ?? 0);
      })
      .catch((err) => {
        if (axios.isCancel(err)) return;
        setIsLoading(false);
        setError(err instanceof Error ? err.message : "Неизвестная ошибка");
        setLocations([]);
      });
  };

  const fetch = () => {
    abortControllerRef.current?.abort();
    const abortController = new AbortController();
    abortControllerRef.current = abortController;

    loadLocations(abortController.signal).finally(() => {
      setIsLoading(false);
    });
  };

  useEffect(() => {
    fetch();
    return () => abortControllerRef.current?.abort();
  }, []);

  const refetch = () => {
    setIsLoading(true);
    setError(null);
    fetch();
  };

  return { locations, totalCount, isLoading, error, refetch };
}
