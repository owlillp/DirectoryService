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
import { useCrateLocation } from "./model/use-create-location";
import { useState } from "react";
import { CreateLocationsRequest } from "@/src/entities/locations/api";

type Props = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function CreateLocationDialog({ open, onOpenChange }: Props) {
  const initialData: CreateLocationsRequest = {
    name: "",
    timeZone: "",
    address: {
      country: "",
      city: "",
      street: "",
      buildingNumber: 0,
      apartment: null,
      postalCode: 0,
    },
  };

  const [createFormData, setCreateFormData] = useState(initialData);
  const { createLocation, isPending } = useCrateLocation();

  const handleSubmit = (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();

    createLocation(createFormData, {
      onSuccess: () => {
        setCreateFormData(initialData);
        onOpenChange(false);
      },
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

        <form onSubmit={handleSubmit}>
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
                value={createFormData.name}
                onChange={(e) =>
                  setCreateFormData((prev) => ({
                    ...prev,
                    name: e.target.value,
                  }))
                }
              />
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
                value={createFormData.timeZone}
                onChange={(e) =>
                  setCreateFormData((prev) => ({
                    ...prev,
                    timeZone: e.target.value,
                  }))
                }
              />
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
                  value={createFormData.address.country}
                  onChange={(e) =>
                    setCreateFormData((prev) => ({
                      ...prev,
                      address: { ...prev.address, country: e.target.value },
                    }))
                  }
                />
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
                    value={createFormData.address.city}
                    onChange={(e) =>
                      setCreateFormData((prev) => ({
                        ...prev,
                        address: { ...prev.address, city: e.target.value },
                      }))
                    }
                  />
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
                    value={createFormData.address.street}
                    onChange={(e) =>
                      setCreateFormData((prev) => ({
                        ...prev,
                        address: { ...prev.address, street: e.target.value },
                      }))
                    }
                  />
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
                    value={createFormData.address.buildingNumber}
                    onChange={(e) =>
                      setCreateFormData((prev) => ({
                        ...prev,
                        address: {
                          ...prev.address,
                          buildingNumber: Number(e.target.value),
                        },
                      }))
                    }
                  />
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
                    value={createFormData.address.apartment ?? ""}
                    onChange={(e) =>
                      setCreateFormData((prev) => ({
                        ...prev,
                        address: {
                          ...prev.address,
                          apartment: e.target.value,
                        },
                      }))
                    }
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
                  value={createFormData.address.postalCode}
                  onChange={(e) =>
                    setCreateFormData((prev) => ({
                      ...prev,
                      address: {
                        ...prev.address,
                        postalCode: Number(e.target.value),
                      },
                    }))
                  }
                />
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
