import {
  locationsApi,
  locationsQueryOptions,
  UpdateLocationRequest,
} from "@/src/entities/locations/api";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (request: UpdateLocationRequest) =>
      locationsApi.updateLocation(request),
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      }),
    onSuccess: () => {
      toast.success("Локация успешно обновлена");
    },
  });

  return {
    updateLocation: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  };
}
