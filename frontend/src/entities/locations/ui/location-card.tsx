"use client";

import { Location } from "../types";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/src/shared/components/ui/card";
import { Badge } from "@/src/shared/components/ui/badge";
import {
  MapPinIcon,
  BuildingIcon,
  GlobeIcon,
  CalendarDaysIcon,
} from "lucide-react";

type LocationCardProps = {
  location: Location;
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

export function LocationCard({ location }: LocationCardProps) {
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
      </CardContent>
    </Card>
  );
}
