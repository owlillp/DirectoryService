"use client";

import { SearchDepartment } from "@/src/entities/departments/types";
import {
  DepartmentListId,
  resetDepartmentState,
  setDepartmentActive,
  setDepartmentExcludeIds,
  setDepartmentSearch,
  setDepartmentSortBy,
  setDepartmentSortDirection,
  useDepartmentActive,
  useDepartmentSearch,
  useDepartmentSortBy,
  useDepartmentSortDirection,
} from "../department-list-store";
import { useDepartmentsList } from "../use-department-list";
import { useCallback, useEffect, useId, useState } from "react";
import { Button } from "@/src/shared/components/ui/button";
import { Input } from "@/src/shared/components/ui/input";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/src/shared/components/ui/popover";
import { Spinner } from "@/src/shared/components/ui/spinner";
import { cn } from "@/src/shared/lib/utils";
import { ChevronsUpDown } from "lucide-react";
import { DepartmentSelected } from "./department-selected";
import { DepartmentSelectCard } from "./department-select-card";

type ActiveFilter = "all" | "active" | "inactive";

const ACTIVE_FILTER_OPTIONS: { value: ActiveFilter; label: string }[] = [
  { value: "all", label: "Все" },
  { value: "active", label: "Активные" },
  { value: "inactive", label: "Неактивные" },
];

const SORT_OPTIONS = [
  { value: "name_asc", label: "Имя (А→Я)", sortBy: "name", sortDirection: "asc" },
  { value: "name_desc", label: "Имя (Я→А)", sortBy: "name", sortDirection: "desc" },
  { value: "createdAt_desc", label: "Сначала новые", sortBy: "createdAt", sortDirection: "desc" },
  { value: "createdAt_asc", label: "Сначала старые", sortBy: "createdAt", sortDirection: "asc" },
] as const;

export type DepartmentSelectProps = {
  selectedDepartments: SearchDepartment[];
  onChange: (departments: SearchDepartment[]) => void;
  stateId: DepartmentListId;
  multiselect?: boolean;
  excludeIds?: string[];
  placeholder?: string;
};

export const DepartmentSelect = ({
  selectedDepartments,
  onChange,
  stateId,
  multiselect = true,
  excludeIds,
  placeholder = "Выберите подразделения...",
}: DepartmentSelectProps) => {
  const [open, setOpen] = useState(false);
  const search = useDepartmentSearch(stateId);
  const isActive = useDepartmentActive(stateId);
  const sortBy = useDepartmentSortBy(stateId);
  const sortDirection = useDepartmentSortDirection(stateId);
  const inputId = useId();

  const activeFilter: ActiveFilter =
    isActive === undefined ? "all" : isActive ? "active" : "inactive";

  const currentSortValue =
    sortBy && sortDirection ? `${sortBy}_${sortDirection}` : "name_asc";

  useEffect(() => {
    setDepartmentExcludeIds(excludeIds, stateId);
  }, [excludeIds, stateId]);

  useEffect(() => {
    return () => {
      resetDepartmentState(stateId);
    };
  }, [stateId]);

  const {
    departments,
    totalCount,
    isPending,
    error,
    refetch,
    isFetchingNextPage,
    cursorRef,
  } = useDepartmentsList(stateId);

  const selectedIds = new Set(selectedDepartments.map((d) => d.id));

  const handleSelect = useCallback(
    (department: SearchDepartment) => {
      if (multiselect) {
        if (selectedIds.has(department.id)) {
          onChange(selectedDepartments.filter((d) => d.id !== department.id));
        } else {
          onChange([...selectedDepartments, department]);
        }
      } else {
        if (selectedIds.has(department.id)) {
          onChange([]);
        } else {
          onChange([department]);
        }
      }
    },
    [multiselect, onChange, selectedDepartments, selectedIds],
  );

  const handleRemoveBadge = useCallback(
    (departmentId: string) => {
      onChange(selectedDepartments.filter((d) => d.id !== departmentId));
    },
    [onChange, selectedDepartments],
  );

  const handleActiveFilterChange = useCallback(
    (filter: ActiveFilter) => {
      switch (filter) {
        case "all":
          setDepartmentActive(undefined, stateId);
          break;
        case "active":
          setDepartmentActive(true, stateId);
          break;
        case "inactive":
          setDepartmentActive(false, stateId);
          break;
      }
    },
    [stateId],
  );

  const handleSortChange = useCallback(
    (value: string) => {
      const option = SORT_OPTIONS.find((o) => o.value === value);
      if (option) {
        setDepartmentSortBy(option.sortBy, stateId);
        setDepartmentSortDirection(option.sortDirection, stateId);
      }
    },
    [stateId],
  );

  const filteredDepartments = excludeIds?.length
    ? departments.filter((d) => !excludeIds.includes(d.id))
    : departments;

  return (
    <div className="space-y-2">
      {/* Badges for selected departments (both modes) */}
      {selectedDepartments.length > 0 && (
        <div className="flex flex-wrap gap-1.5">
          {selectedDepartments.map((dept) => (
            <DepartmentSelected
              key={dept.id}
              department={dept}
              onRemove={handleRemoveBadge}
            />
          ))}
        </div>
      )}

      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            role="combobox"
            aria-expanded={open}
            className={cn(
              "w-full justify-between",
              !selectedDepartments.length && "text-muted-foreground",
            )}
          >
            {selectedDepartments.length > 0
              ? multiselect
                ? `Выбрано: ${selectedDepartments.length}`
                : selectedDepartments[0].name
              : placeholder}
            <ChevronsUpDown className="ml-2 size-4 shrink-0 opacity-50" />
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-[--radix-popover-trigger-width] p-0">
          <div className="flex flex-col">
            {/* Search */}
            <div className="border-b p-2">
              <Input
                id={inputId}
                placeholder="Поиск подразделений..."
                value={search ?? ""}
                onChange={(e) => setDepartmentSearch(e.target.value, stateId)}
                className="h-8"
              />
            </div>

            {/* Filters row */}
            <div className="flex items-center gap-2 border-b px-3 py-1.5">
              <div className="flex items-center gap-1 shrink-0">
                {ACTIVE_FILTER_OPTIONS.map((opt) => (
                  <button
                    key={opt.value}
                    type="button"
                    className={cn(
                      "rounded-md px-2 py-0.5 text-xs font-medium transition-colors",
                      activeFilter === opt.value
                        ? "bg-primary text-primary-foreground"
                        : "text-muted-foreground hover:bg-muted",
                    )}
                    onClick={() => handleActiveFilterChange(opt.value)}
                  >
                    {opt.label}
                  </button>
                ))}
              </div>

              <div className="ml-auto">
                <select
                  value={currentSortValue}
                  onChange={(e) => handleSortChange(e.target.value)}
                  className="h-7 rounded-md border border-input bg-transparent px-2 text-xs text-muted-foreground outline-none focus-visible:border-ring focus-visible:ring-1 focus-visible:ring-ring/50"
                >
                  {SORT_OPTIONS.map((opt) => (
                    <option key={opt.value} value={opt.value}>
                      {opt.label}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {/* List */}
            <div className="max-h-64 overflow-y-auto">
              {isPending && (
                <div className="flex items-center justify-center py-8">
                  <Spinner className="size-5" />
                </div>
              )}

              {error && (
                <div className="flex flex-col items-center gap-2 py-6 px-4 text-center">
                  <p className="text-sm text-destructive">{error}</p>
                  <Button variant="outline" size="sm" onClick={() => refetch()}>
                    Повторить
                  </Button>
                </div>
              )}

              {!isPending && !error && filteredDepartments.length === 0 && (
                <div className="flex flex-col items-center gap-1 py-6 px-4 text-center">
                  <p className="text-sm text-muted-foreground">
                    {search
                      ? "Ничего не найдено"
                      : "Нет доступных подразделений"}
                  </p>
                </div>
              )}

              {!isPending &&
                !error &&
                filteredDepartments.map((department) => (
                  <DepartmentSelectCard
                    key={department.id}
                    department={department}
                    isSelected={selectedIds.has(department.id)}
                    multiselect={multiselect}
                    onSelect={handleSelect}
                  />
                ))}

              <div ref={cursorRef} className="h-px" />

              {isFetchingNextPage && (
                <div className="flex items-center justify-center py-2">
                  <Spinner className="size-4" />
                </div>
              )}

              {!isPending && !error && filteredDepartments.length > 0 && (
                <div className="border-t px-3 py-1.5 text-xs text-muted-foreground">
                  {totalCount
                    ? `Всего: ${totalCount}`
                    : `${filteredDepartments.length} подразделений`}
                </div>
              )}
            </div>
          </div>
        </PopoverContent>
      </Popover>
    </div>
  );
};
