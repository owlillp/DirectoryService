import { SearchDepartment } from "@/src/entities/departments/types";
import { Badge } from "@/src/shared/components/ui/badge";
import { Checkbox } from "@/src/shared/components/ui/checkbox";
import { cn } from "@/src/shared/lib/utils";
import { Building2, CheckIcon, Archive } from "lucide-react";

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
      role="option"
      aria-selected={isSelected}
      className={cn(
        "group flex w-full cursor-pointer items-center gap-3 px-3 py-3",
        "transition-all duration-150 ease-in-out",
        "border-b border-border/40 last:border-b-0",
        "hover:bg-accent/50 hover:pl-4",
        isSelected && "bg-primary/10 border-l-2 border-l-primary",
        !isSelected && "border-l-2 border-l-transparent",
      )}
      onClick={() => onSelect(department)}
    >
      {/* Radio/Checkbox */}
      {multiselect ? (
        <div className="relative shrink-0">
          <Checkbox
            checked={isSelected}
            className={cn(
              "size-4 pointer-events-none transition-all duration-150",
              isSelected && "scale-110",
            )}
          />
        </div>
      ) : (
        <div
          className={cn(
            "size-4 shrink-0 rounded-full border-2 flex items-center justify-center transition-all duration-150",
            isSelected
              ? "border-primary bg-primary text-primary-foreground scale-110 shadow-sm shadow-primary/30"
              : "border-muted-foreground/30 group-hover:border-muted-foreground/50",
          )}
        >
          {isSelected && <CheckIcon className="size-3" strokeWidth={3} />}
        </div>
      )}

      {/* Department info */}
      <div className="flex flex-col min-w-0 flex-1 text-left">
        <span className="truncate text-sm font-medium">{department.name}</span>
        <span className="truncate text-xs text-muted-foreground/70 flex items-center gap-1">
          <Building2 className="size-3 shrink-0 inline" />
          {department.path || department.identifier}
        </span>
      </div>

      {/* Archived badge */}
      {!department.isActive && (
        <Badge
          variant="outline"
          className="shrink-0 text-[10px] px-1.5 py-0.5 gap-1 border-amber-200 bg-amber-50 text-amber-700 dark:border-amber-800 dark:bg-amber-950/50 dark:text-amber-400"
        >
          <Archive className="size-3" />
          archived
        </Badge>
      )}

      {/* Selected indicator */}
      {isSelected && !multiselect && (
        <div className="shrink-0 ml-1">
          <div className="size-2 rounded-full bg-primary" />
        </div>
      )}
    </div>
  );
};
