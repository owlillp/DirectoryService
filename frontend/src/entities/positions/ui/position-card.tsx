"use client";

import { Position } from "../types";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/src/shared/components/ui/card";
import { Badge } from "@/src/shared/components/ui/badge";
import {
  BriefcaseIcon,
  Building2Icon,
  CalendarDaysIcon,
  FileTextIcon,
} from "lucide-react";

type PositionCardProps = {
  position: Position;
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

export function PositionCard({ position }: PositionCardProps) {
  return (
    <Card size="sm">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <BriefcaseIcon className="size-4 text-muted-foreground shrink-0" />
          <span>{position.name}</span>
          <Badge
            variant={position.isActive ? "default" : "secondary"}
            className="ml-auto"
          >
            {position.isActive ? "Активна" : "Неактивна"}
          </Badge>
        </CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-2 text-muted-foreground">
        {position.descroption && (
          <div className="flex items-start gap-2 rounded-md bg-muted/50 px-3 py-2">
            <FileTextIcon className="size-3.5 shrink-0 text-foreground/60 mt-0.5" />
            <span>{position.descroption}</span>
          </div>
        )}
        {position.departmentIds.length > 0 && (
          <div className="flex items-center gap-2 rounded-md bg-muted/50 px-3 py-2">
            <Building2Icon className="size-3.5 shrink-0 text-foreground/60" />
            <span>
              Подразделений:{" "}
              <span className="font-medium text-foreground/80">
                {position.departmentIds.length}
              </span>
            </span>
          </div>
        )}
        <div className="flex items-center gap-2 rounded-md bg-muted/50 px-3 py-2">
          <CalendarDaysIcon className="size-3.5 shrink-0 text-foreground/60" />
          <span>Создана: {formatDate(position.createdAt)}</span>
        </div>
      </CardContent>
    </Card>
  );
}
