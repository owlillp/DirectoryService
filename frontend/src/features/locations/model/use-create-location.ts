import {
  locationsApi,
  locationsQueryOptions,
} from "@/src/entities/locations/api";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreateLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: locationsApi.createLocation,
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      }),
    onError: (e) => {
      console.log(e);
      toast.error("Ошибка при создании локации");
    },
    onSuccess: () => {
      toast.success("Локация успешно создана");
    },
  });

  return {
    createLocation: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  };
}
