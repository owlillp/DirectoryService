import { FieldValues, Path, UseFormSetError } from "react-hook-form";
import { ErrorMessage, isEnvelopeError } from "@/src/shared/api/errors";
import { toast } from "sonner";

export const locationErrorFieldMap: Record<string, string> = {
  name: "name",
  timeZone: "timeZone",
  country: "address.country",
  city: "address.city",
  street: "address.street",
  buildingNumber: "address.buildingNumber",
  apartment: "address.apartment",
  postalCode: "address.postalCode",
};

export function applyLocationErrors<TFieldValues extends FieldValues>(
  setError: UseFormSetError<TFieldValues>,
  errors: ErrorMessage[],
): void {
  let hasUnmappedError = false;

  for (const error of errors) {
    const fieldPath = error.invalidField
      ? locationErrorFieldMap[error.invalidField]
      : undefined;

    if (fieldPath) {
      setError(fieldPath as Path<TFieldValues>, {
        type: "manual",
        message: error.message,
      });
    } else {
      hasUnmappedError = true;
    }
  }

  if (hasUnmappedError) {
    const unmappedMessages = errors
      .filter((e) => !e.invalidField || !locationErrorFieldMap[e.invalidField])
      .map((e) => e.message);

    if (unmappedMessages.length > 0) {
      toast.error(unmappedMessages.join("\n"));
    }
  }
}

export function handleLocationSubmitError<TFieldValues extends FieldValues>(
  error: unknown,
  setError?: UseFormSetError<TFieldValues>,
): void {
  if (isEnvelopeError(error) && setError) {
    applyLocationErrors(setError, error.errors);
  } else if (isEnvelopeError(error)) {
    toast.error(error.message);
  } else {
    toast.error("Ошибка при создании локации");
  }
}
