"use client";

import { DepartmentWithChildren } from "../types";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/src/shared/components/ui/card";
import { Badge } from "@/src/shared/components/ui/badge";
import { BuildingIcon } from "lucide-react";
import { DepartmentInfo } from "./department-info";
import { DepartmentChildren } from "./department-children";

type DepartmentCardProps = {
  department: DepartmentWithChildren;
};

export function DepartmentCard({ department }: DepartmentCardProps) {
  return (
    <Card size="sm">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <BuildingIcon className="size-4 text-muted-foreground shrink-0" />
          <span>{department.name}</span>
          <Badge
            variant={department.isActive ? "default" : "secondary"}
            className="ml-auto"
          >
            {department.isActive ? "Активно" : "Неактивно"}
          </Badge>
        </CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-2 text-muted-foreground">
        <DepartmentInfo department={department} />
        <DepartmentChildren department={department} />
      </CardContent>
    </Card>
  );
}
