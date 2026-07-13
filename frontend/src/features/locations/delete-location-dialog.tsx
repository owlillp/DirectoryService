"use client";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/src/shared/components/ui/dialog";
import { Button } from "@/src/shared/components/ui/button";
import { useDeleteLocation } from "./model/use-delete-location";
import { Location } from "@/src/entities/locations/types";
import { Trash2Icon, AlertTriangleIcon, BuildingIcon, MapPinIcon } from "lucide-react";

type Props = {
  location: Location;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function DeleteLocationDialog({ location, open, onOpenChange }: Props) {
  const { deleteLocation, isPending } = useDeleteLocation();

  const handleDelete = () => {
    deleteLocation(location.id, {
      onSuccess: () => {
        onOpenChange(false);
      },
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-destructive">
            <AlertTriangleIcon className="size-5 shrink-0" />
            Удаление локации
          </DialogTitle>
          <DialogDescription className="pt-2">
            Вы уверены, что хотите удалить локацию? Это действие нельзя отменить.
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-2 rounded-lg border bg-muted/30 p-3">
          <div className="flex items-center gap-2 font-medium text-foreground">
            <MapPinIcon className="size-4 shrink-0 text-muted-foreground" />
            <span>{location.name}</span>
          </div>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <BuildingIcon className="size-3.5 shrink-0" />
            <span>
              {location.address.country}, {location.address.city},{" "}
              {location.address.street}, {location.address.buildingNumber}
            </span>
          </div>
          <p className="text-xs text-muted-foreground/70">
            После удаления локация станет неактивной и будет скрыта из списка.
          </p>
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isPending}
          >
            Отмена
          </Button>
          <Button
            variant="destructive"
            onClick={handleDelete}
            disabled={isPending}
          >
            <Trash2Icon />
            {isPending ? "Удаление..." : "Удалить"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
