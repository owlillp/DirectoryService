"use client";

import { SearchDepartment } from "@/src/entities/departments/types";
import { Badge } from "@/src/shared/components/ui/badge";
import { Checkbox } from "@/src/shared/components/ui/checkbox";
import { cn } from "@/src/shared/lib/utils";
import { CheckIcon } from "lucide-react";

type DepartmentSelectCardProps = {
  department: SearchDepartment;
  isSelected: boolean;
  multiselect: boolean;
  onSelect: (department: SearchDepartment) => void;
};

export const DepartmentSelectCard = ({
  department,
  isSelected,
  multiselect,
  onSelect,
}: DepartmentSelectCardProps) => {
  return (
    <div
      role="button"
      tabIndex={0}
      className={cn(
        "flex w-full items-center gap-2 px-3 py-2 text-sm text-left hover:bg-muted transition-colors cursor-pointer",
        isSelected && "bg-muted/50",
      )}
      onClick={() => onSelect(department)}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          onSelect(department);
        }
      }}
    >
      {multiselect ? (
        <Checkbox checked={isSelected} className="size-4 pointer-events-none" />
      ) : (
        <div
          className={cn(
            "size-4 shrink-0 rounded-full border transition-colors flex items-center justify-center",
            isSelected
              ? "border-primary bg-primary text-primary-foreground"
              : "border-input",
          )}
        >
          {isSelected && <CheckIcon className="size-3" />}
        </div>
      )}
      <div className="flex flex-col min-w-0 flex-1">
        <span className="truncate font-medium">{department.name}</span>
        <span className="truncate text-xs text-muted-foreground">
          {department.path || department.identifier}
        </span>
      </div>
      {!department.isActive && (
        <Badge variant="outline" className="shrink-0 text-[10px] px-1 py-0">
          archived
        </Badge>
      )}
    </div>
  );
};
