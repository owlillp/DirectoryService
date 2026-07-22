"use client";

import { SearchDepartment } from "@/src/entities/departments/types";
import { Badge } from "@/src/shared/components/ui/badge";
import { XIcon } from "lucide-react";

type DepartmentSelectedProps = {
  department: SearchDepartment;
  onRemove: (departmentId: string) => void;
};

export const DepartmentSelected = ({
  department,
  onRemove,
}: DepartmentSelectedProps) => {
  return (
    <Badge variant="secondary" className="gap-1 pr-1">
      {department.name}
      <button
        type="button"
        onClick={() => onRemove(department.id)}
        className="ml-0.5 rounded-full p-0.5 hover:bg-muted-foreground/20 transition-colors"
      >
        <XIcon className="size-3" />
      </button>
    </Badge>
  );
};
