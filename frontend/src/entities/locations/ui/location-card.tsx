"use client";

import { Location } from "../types";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/src/shared/components/ui/card";
import { Badge } from "@/src/shared/components/ui/badge";
import { Button } from "@/src/shared/components/ui/button";
import {
  MapPinIcon,
  BuildingIcon,
  GlobeIcon,
  CalendarDaysIcon,
  Trash2Icon,
  PencilIcon,
} from "lucide-react";

type LocationCardProps = {
  location: Location;
  onDelete: () => void;
  onEdit: () => void;
};

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString("ru-RU", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function LocationCard({
  location,
  onDelete,
  onEdit,
}: LocationCardProps) {
  return (
    <Card size="sm">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <MapPinIcon className="size-4 text-muted-foreground shrink-0" />
          <span>{location.name}</span>
          <Badge
            variant={location.isActive ? "default" : "secondary"}
            className="ml-auto"
          >
            {location.isActive ? "Активна" : "Неактивна"}
          </Badge>
        </CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-2 text-muted-foreground">
        <div className="flex items-center gap-2 rounded-md bg-muted/50 px-3 py-2">
          <BuildingIcon className="size-3.5 shrink-0 text-foreground/60" />
          <span>
            {location.address.country}, {location.address.city},{" "}
            {location.address.street}, {location.address.buildingNumber}
            {location.address.apartment && `, ${location.address.apartment}`}
          </span>
        </div>
        <div className="flex items-center gap-2 rounded-md bg-muted/50 px-3 py-2">
          <GlobeIcon className="size-3.5 shrink-0 text-foreground/60" />
          <span>Часовой пояс: {location.timeZone}</span>
        </div>
        <div className="flex items-center gap-2 rounded-md bg-muted/50 px-3 py-2">
          <CalendarDaysIcon className="size-3.5 shrink-0 text-foreground/60" />
          <span>Создана: {formatDate(location.createdAt)}</span>
        </div>

        <div className="flex justify-end gap-2 pt-2 border-t border-border/50 mt-1">
          <Button
            variant="ghost"
            size="sm"
            onClick={onEdit}
            className="text-muted-foreground/50 hover:text-foreground hover:bg-muted [&_svg]:hover:text-foreground"
          >
            <PencilIcon />
            Изменить
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={onDelete}
            className="text-muted-foreground/50 hover:text-destructive hover:bg-destructive/10 [&_svg]:hover:text-destructive"
          >
            <Trash2Icon />
            Удалить
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
