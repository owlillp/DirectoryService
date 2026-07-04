"use client";

import { DepartmentWithChildren } from "../types";
import { CalendarDaysIcon } from "lucide-react";

type DepartmentInfoProps = {
  department: DepartmentWithChildren;
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

export function DepartmentInfo({ department }: DepartmentInfoProps) {
  return (
    <>
      <div className="flex items-center gap-2 rounded-md bg-muted/50 px-3 py-2">
        <span className="text-xs font-medium text-foreground/60 shrink-0">
          Код
        </span>
        <span className="font-mono text-xs font-semibold text-foreground/80 tracking-wider">
          {department.identifier}
        </span>
      </div>
      <div className="flex items-center gap-2 rounded-md bg-muted/50 px-3 py-2">
        <span className="text-xs font-medium text-foreground/60 shrink-0">
          Путь
        </span>
        <span className="font-mono text-xs text-foreground/70 truncate">
          {department.path}
        </span>
      </div>
      <div className="flex items-center gap-2 rounded-md bg-muted/50 px-3 py-2">
        <CalendarDaysIcon className="size-3.5 shrink-0 text-foreground/60" />
        <span className="text-xs">
          Создано: {formatDate(department.createdAt)}
        </span>
      </div>
    </>
  );
}
