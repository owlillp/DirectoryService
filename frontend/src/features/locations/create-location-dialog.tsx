import { Button } from "@/src/shared/components/ui/button";
import {
  Dialog,
  DialogTrigger,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/src/shared/components/ui/dialog";
import { Input } from "@/src/shared/components/ui/input";
import { PlusIcon } from "lucide-react";
import { useCreateLocation } from "./model/use-create-location";
import { handleLocationSubmitError } from "./model/location-error-map";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

const knownTimeZones =
  typeof window !== "undefined" ? Intl.supportedValuesOf("timeZone") : [];

const createLocationSchema = z.object({
  name: z
    .string()
    .min(1, "Название локации обязательно")
    .min(3, "Название должно содержать минимум 3 символа")
    .max(120, "Название не должно превышать 120 символов"),
  timeZone: z
    .string()
    .min(1, "Часовой пояс обязателен")
    .refine(
      (val) => knownTimeZones.includes(val),
      "Указан некорректный часовой пояс",
    ),
  address: z.object({
    country: z.string().min(1, "Страна обязательна"),
    city: z.string().min(1, "Город обязателен"),
    street: z.string().min(1, "Улица обязательна"),
    buildingNumber: z
      .number({ error: "Номер дома обязателен" })
      .positive("Номер дома должен быть положительным числом"),
    apartment: z.string().optional().or(z.literal("")),
    postalCode: z
      .number({ error: "Почтовый индекс обязателен" })
      .positive("Почтовый индекс должен быть положительным числом"),
  }),
});

type CreateLocationData = z.infer<typeof createLocationSchema>;

type Props = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function CreateLocationDialog({ open, onOpenChange }: Props) {
  const initialData: CreateLocationData = {
    name: "",
    timeZone: "",
    address: {
      country: "",
      city: "",
      street: "",
      buildingNumber: undefined as unknown as number,
      apartment: "",
      postalCode: undefined as unknown as number,
    },
  };

  const { createLocation, isPending } = useCreateLocation();

  const {
    register,
    handleSubmit,
    formState: { errors },
    setError,
    reset,
  } = useForm<CreateLocationData>({
    defaultValues: initialData,
    resolver: zodResolver(createLocationSchema),
  });

  const onSubmit = (data: CreateLocationData) => {
    createLocation(data, {
      onSuccess: () => {
        reset(initialData);
        onOpenChange(false);
      },
      onError: (error) => handleLocationSubmitError(error, setError),
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogTrigger asChild>
        <Button size="sm">
          <PlusIcon />
          Создать
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Создание локации</DialogTitle>
          <DialogDescription>
            Заполните данные для новой локации
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="flex flex-col gap-4 py-2">
            {/* Название локации */}
            <div className="flex flex-col gap-2">
              <label
                htmlFor="name"
                className="text-sm font-medium text-foreground"
              >
                Название локации
              </label>
              <Input
                id="name"
                placeholder="Например: Главный офис"
                aria-invalid={!!errors.name}
                {...register("name")}
              />
              {errors.name?.message && (
                <p className="text-sm text-destructive">
                  {errors.name.message}
                </p>
              )}
            </div>

            {/* Часовой пояс */}
            <div className="flex flex-col gap-2">
              <label
                htmlFor="timezone"
                className="text-sm font-medium text-foreground"
              >
                Часовой пояс
              </label>
              <Input
                id="timezone"
                placeholder="Например: Europe/Moscow"
                aria-invalid={!!errors.timeZone}
                {...register("timeZone")}
              />
              {errors.timeZone?.message && (
                <p className="text-sm text-destructive">
                  {errors.timeZone.message}
                </p>
              )}
            </div>

            {/* Адрес */}
            <div className="flex flex-col gap-3 rounded-lg border border-border bg-muted/30 p-4">
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
                Адрес
              </p>

              <div className="flex flex-col gap-2">
                <label
                  htmlFor="country"
                  className="text-sm font-medium text-foreground"
                >
                  Страна
                </label>
                <Input
                  id="country"
                  placeholder="Россия"
                  aria-invalid={!!errors.address?.country}
                  {...register("address.country")}
                />
                {errors.address?.country?.message && (
                  <p className="text-sm text-destructive">
                    {errors.address.country.message}
                  </p>
                )}
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div className="flex flex-col gap-2">
                  <label
                    htmlFor="city"
                    className="text-sm font-medium text-foreground"
                  >
                    Город
                  </label>
                  <Input
                    id="city"
                    placeholder="Москва"
                    aria-invalid={!!errors.address?.city}
                    {...register("address.city")}
                  />
                  {errors.address?.city?.message && (
                    <p className="text-sm text-destructive">
                      {errors.address.city.message}
                    </p>
                  )}
                </div>

                <div className="flex flex-col gap-2">
                  <label
                    htmlFor="street"
                    className="text-sm font-medium text-foreground"
                  >
                    Улица
                  </label>
                  <Input
                    id="street"
                    placeholder="Тверская"
                    aria-invalid={!!errors.address?.street}
                    {...register("address.street")}
                  />
                  {errors.address?.street?.message && (
                    <p className="text-sm text-destructive">
                      {errors.address.street.message}
                    </p>
                  )}
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div className="flex flex-col gap-2">
                  <label
                    htmlFor="buildingNumber"
                    className="text-sm font-medium text-foreground"
                  >
                    Номер дома
                  </label>
                  <Input
                    id="buildingNumber"
                    type="number"
                    placeholder="1"
                    aria-invalid={!!errors.address?.buildingNumber}
                    {...register("address.buildingNumber", {
                      valueAsNumber: true,
                    })}
                  />
                  {errors.address?.buildingNumber?.message && (
                    <p className="text-sm text-destructive">
                      {errors.address.buildingNumber.message}
                    </p>
                  )}
                </div>

                <div className="flex flex-col gap-2">
                  <label
                    htmlFor="apartment"
                    className="text-sm font-medium text-foreground"
                  >
                    Квартира / Офис
                  </label>
                  <Input
                    id="apartment"
                    placeholder="Офис 101"
                    {...register("address.apartment")}
                  />
                </div>
              </div>

              <div className="flex flex-col gap-2">
                <label
                  htmlFor="postalCode"
                  className="text-sm font-medium text-foreground"
                >
                  Почтовый индекс
                </label>
                <Input
                  id="postalCode"
                  type="number"
                  placeholder="101000"
                  aria-invalid={!!errors.address?.postalCode}
                  {...register("address.postalCode", {
                    valueAsNumber: true,
                  })}
                />
                {errors.address?.postalCode?.message && (
                  <p className="text-sm text-destructive">
                    {errors.address.postalCode.message}
                  </p>
                )}
              </div>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Отмена
            </Button>
            <Button type="submit" disabled={isPending}>
              Создать
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
