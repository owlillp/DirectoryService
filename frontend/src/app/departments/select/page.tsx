"use client";

import { useState } from "react";
import { SearchDepartment } from "@/src/entities/departments/types";
import { DepartmentSelect } from "@/src/features/departments/model/select/department-select";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/src/shared/components/ui/card";
import { Separator } from "@/src/shared/components/ui/separator";

export default function DepartmentSelectDemoPage() {
  const [singleValue, setSingleValue] = useState<SearchDepartment[]>([]);
  const [multiValue, setMultiValue] = useState<SearchDepartment[]>([]);

  return (
    <div className="mx-auto max-w-3xl space-y-8 p-8">
      <div>
        <h1 className="text-2xl font-bold">DepartmentSelect Playground</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Демонстрация компонента выбора подразделений — два независимых
          экземпляра
        </p>
      </div>

      <Separator />

      <Card>
        <CardHeader>
          <CardTitle>Single-режим</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <DepartmentSelect
            stateId="demo-single"
            selectedDepartments={singleValue}
            onCheckedChange={setSingleValue}
            multiselect={false}
            placeholder="Выберите одно подразделение..."
          />
          <div className="rounded-lg border bg-muted/30 p-3">
            <p className="text-xs font-medium text-muted-foreground mb-1">
              Состояние:
            </p>
            <pre className="text-xs whitespace-pre-wrap">
              {JSON.stringify(singleValue, null, 2) || "пусто"}
            </pre>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Multiselect-режим</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <DepartmentSelect
            stateId="demo-multi"
            selectedDepartments={multiValue}
            onCheckedChange={setMultiValue}
            multiselect={true}
            placeholder="Выберите несколько подразделений..."
          />
          <div className="rounded-lg border bg-muted/30 p-3">
            <p className="text-xs font-medium text-muted-foreground mb-1">
              Состояние:
            </p>
            <pre className="text-xs whitespace-pre-wrap">
              {JSON.stringify(multiValue, null, 2) || "пусто"}
            </pre>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
