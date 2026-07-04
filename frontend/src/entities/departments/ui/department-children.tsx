"use client";

import { useState } from "react";
import { DepartmentWithChildren } from "../types";
import { Button } from "@/src/shared/components/ui/button";
import { Badge } from "@/src/shared/components/ui/badge";
import {
  ChevronRightIcon,
  ChevronDownIcon,
  FolderIcon,
  LayersIcon,
  MinusIcon,
  RefreshCwIcon,
} from "lucide-react";

type DepartmentChildrenProps = {
  department: DepartmentWithChildren;
};

export function DepartmentChildren({ department }: DepartmentChildrenProps) {
  const [isOpen, setIsOpen] = useState(false);
  const hasChildren = department.children.length > 0;
  const hasMore = department.hasMoreChildren;

  if (!hasChildren) {
    return (
      <div className="mt-1">
        <div className="flex items-center gap-2 rounded-md bg-muted/30 px-3 py-2">
          <MinusIcon className="size-3.5 shrink-0 text-muted-foreground/40" />
          <span className="text-xs text-muted-foreground/60">
            Нет дочерних подразделений
          </span>
        </div>
      </div>
    );
  }

  return (
    <div className="mt-1">
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="flex w-full items-center gap-2 rounded-md px-3 py-2 text-left text-xs text-muted-foreground transition-colors hover:bg-muted/50"
      >
        {isOpen ? (
          <ChevronDownIcon className="size-3.5 shrink-0" />
        ) : (
          <ChevronRightIcon className="size-3.5 shrink-0" />
        )}
        <LayersIcon className="size-3.5 shrink-0" />
        <span>Дочерние подразделения ({department.children.length})</span>
      </button>

      {isOpen && (
        <div className="mt-2 space-y-1.5 pl-5">
          {department.children.map((child) => (
            <div
              key={child.id}
              className="flex items-center gap-2 rounded-md bg-muted/30 px-3 py-2"
            >
              <FolderIcon className="size-3.5 shrink-0 text-muted-foreground/60" />
              <div className="flex flex-1 items-center gap-2 min-w-0">
                <span className="text-xs font-medium text-foreground/80 truncate">
                  {child.name}
                </span>
                <Badge
                  variant={child.isActive ? "default" : "secondary"}
                  className="shrink-0 text-[10px] leading-none px-1.5 py-0 h-4"
                >
                  {child.isActive ? "Активно" : "Неактивно"}
                </Badge>
              </div>
            </div>
          ))}

          {hasMore && (
            <Button
              variant="ghost"
              size="sm"
              className="w-full gap-1.5 text-xs text-muted-foreground h-7 mt-1"
              onClick={() => {
                // Заглушка для будущей подгрузки
              }}
            >
              <RefreshCwIcon className="size-3" />
              Загрузить ещё
            </Button>
          )}
        </div>
      )}
    </div>
  );
}
