"use client";

import { SearchDepartment } from "@/src/entities/departments/types";
import { Badge } from "@/src/shared/components/ui/badge";
import { Button } from "@/src/shared/components/ui/button";
import { cn } from "@/src/shared/lib/utils";
import { XIcon } from "lucide-react";

interface DepartmentSelectedProps extends React.ComponentProps<"div"> {
  selectedDepartments: SearchDepartment[];
  onRemove: (id: string) => void;
}

export const DepartmentSelected = ({
  selectedDepartments,
  onRemove,
  className,
  ...props
}: DepartmentSelectedProps) => {
  return (
    <div className={cn("flex gap-2 flex-wrap", className)} {...props}>
      {selectedDepartments.map((dep) => (
        <Badge key={dep.id} asChild>
          <Button onClick={() => onRemove(dep.id)}>
            {dep.name}
            <XIcon />
          </Button>
        </Badge>
      ))}
    </div>
  );
};
