"use client";

import { SearchDepartment } from "@/src/entities/departments/types";
import {
  DepartmentListId,
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
import { DepartmentSelectCard } from "./department-select-card";
import { DepartmentSelected } from "./department-selected";
import { Input } from "@/src/shared/components/ui/input";
import { Button } from "@/src/shared/components/ui/button";
import {
  Loader2,
  ChevronsUpDown,
  ArrowUpNarrowWide,
  ArrowDownWideNarrow,
} from "lucide-react";
import { cn } from "@/src/shared/lib/utils";
import { useEffect, useState } from "react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/src/shared/components/ui/select";
import { ScrollArea } from "@/src/shared/components/ui/scroll-area";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/src/shared/components/ui/popover";

type ActiveFilter = "all" | "active" | "inactive";

const ACTIVE_FILTER_OPTIONS: { value: ActiveFilter; label: string }[] = [
  { value: "all", label: "Все" },
  { value: "active", label: "Активные" },
  { value: "inactive", label: "Неактивные" },
];

const SORT_OPTIONS = [
  { value: "name", label: "Имя", sortBy: "name" },
  { value: "createdAt", label: "Дата создания", sortBy: "created_at" },
] as const;

const SORT_DIRECTION_OPTIONS = [
  { value: "asc", label: "По возрастанию", sortDirection: "asc" },
  { value: "desc", label: "По убыванию", sortDirection: "desc" },
] as const;

interface DepartmentSelectProps extends React.ComponentProps<"div"> {
  selectedDepartments: SearchDepartment[];
  onCheckedChange: (departments: SearchDepartment[]) => void;
  stateId: DepartmentListId;
  multiselect?: boolean;
  excludeIds?: string[];
  placeholder?: string;
  disabled?: boolean;
}

export const DepartmentSelect = ({
  selectedDepartments,
  onCheckedChange,
  stateId,
  multiselect = true,
  excludeIds,
  placeholder,
  disabled = false,
  className,
  ...props
}: DepartmentSelectProps) => {
  const [open, setOpen] = useState(false);
  const search = useDepartmentSearch(stateId);
  const isActive = useDepartmentActive(stateId);
  const sortBy = useDepartmentSortBy(stateId);
  const sortDirection = useDepartmentSortDirection(stateId);

  // Синхронизируем excludeIds с хранилищем
  useEffect(() => {
    setDepartmentExcludeIds(excludeIds, stateId);
  }, [excludeIds, stateId]);

  const {
    departments,
    totalCount,
    isPending,
    error,
    refetch,
    isFetchingNextPage,
    cursorRef,
  } = useDepartmentsList(stateId);

  const handleCheckedChange = (
    selected: boolean,
    department: SearchDepartment,
  ) => {
    if (multiselect) {
      if (selected) {
        onCheckedChange([...selectedDepartments, department]);
      } else {
        onCheckedChange(
          selectedDepartments.filter((dep) => dep.id != department.id),
        );
      }
    } else {
      if (selected) {
        onCheckedChange([department]);
        setOpen(false); // Закрываем popover при выборе в single-режиме
      } else {
        onCheckedChange([]);
      }
    }
  };

  const handleRemoveDepartment = (id: string) => {
    onCheckedChange(selectedDepartments.filter((dep) => dep.id != id));
  };

  const isSelected = (id: string) => {
    return selectedDepartments.some((dep) => dep.id == id);
  };

  const triggerLabel =
    selectedDepartments.length > 0
      ? multiselect
        ? `Выбрано: ${selectedDepartments.length}`
        : selectedDepartments[0].name
      : (placeholder ?? "Выберите подразделения...");

  return (
    <div className={cn("flex flex-col gap-4", className)} {...props}>
      {/* Выбранные департаменты */}
      {selectedDepartments.length > 0 && (
        <DepartmentSelected
          selectedDepartments={selectedDepartments}
          onRemove={handleRemoveDepartment}
          disabled={disabled}
        />
      )}

      {/* Popover с выбором */}
      <Popover
        open={open && !disabled}
        onOpenChange={disabled ? () => {} : setOpen}
      >
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            role="combobox"
            aria-expanded={open}
            disabled={disabled}
            className={cn(
              "w-full justify-between bg-card hover:bg-muted/50 transition-colors",
              open && "ring-2 ring-primary/30 border-primary/50",
              disabled && "opacity-50 cursor-not-allowed hover:bg-card",
            )}
          >
            <span className="truncate text-sm">{triggerLabel}</span>
            <ChevronsUpDown className="size-4 shrink-0 text-muted-foreground" />
          </Button>
        </PopoverTrigger>

        <PopoverContent
          className="w-[var(--radix-popover-trigger-width)] p-3"
          align="start"
        >
          <div className="flex flex-col gap-3">
            {/* Поиск */}
            <Input
              placeholder="Поиск департаментов..."
              value={search ?? ""}
              onChange={(e) => setDepartmentSearch(e.target.value, stateId)}
              autoFocus
            />

            {/* Фильтры */}
            <div className="flex gap-1.5 flex-wrap">
              <Select
                value={
                  isActive === undefined
                    ? "all"
                    : isActive
                      ? "active"
                      : "inactive"
                }
                onValueChange={(value) =>
                  setDepartmentActive(
                    value === "all" ? undefined : value === "active",
                    stateId,
                  )
                }
              >
                <SelectTrigger className="flex-1 min-w-24">
                  <SelectValue placeholder="Статус" />
                </SelectTrigger>
                <SelectContent>
                  {ACTIVE_FILTER_OPTIONS.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <Select
                value={sortBy ?? "name"}
                onValueChange={(value) => setDepartmentSortBy(value, stateId)}
              >
                <SelectTrigger className="flex-1 min-w-24">
                  <SelectValue placeholder="Сортировка" />
                </SelectTrigger>
                <SelectContent>
                  {SORT_OPTIONS.map((option) => (
                    <SelectItem key={option.value} value={option.sortBy}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <Select
                value={sortDirection ?? "asc"}
                onValueChange={(value) =>
                  setDepartmentSortDirection(value, stateId)
                }
              >
                <SelectTrigger className="flex-1 min-w-28">
                  {(sortDirection ?? "asc") === "asc" ? (
                    <ArrowUpNarrowWide className="size-3.5 shrink-0 text-muted-foreground" />
                  ) : (
                    <ArrowDownWideNarrow className="size-3.5 shrink-0 text-muted-foreground" />
                  )}
                  <SelectValue placeholder="Направление" />
                </SelectTrigger>
                <SelectContent>
                  {SORT_DIRECTION_OPTIONS.map((option) => (
                    <SelectItem key={option.value} value={option.sortDirection}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Счётчик найденных департаментов */}
            {!isPending && !error && (
              <div className="flex items-center justify-between rounded-lg bg-muted/40 px-3 py-2">
                <span className="text-xs font-medium text-muted-foreground">
                  Найдено подразделений
                </span>
                <span className="inline-flex items-center justify-center rounded-md bg-primary/10 px-2 py-0.5 text-xs font-semibold text-primary ring-1 ring-primary/20">
                  {totalCount}
                </span>
              </div>
            )}

            {/* Список департаментов */}
            <ScrollArea className="h-72 rounded-lg bg-muted/20">
              {isPending ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="size-6 animate-spin text-muted-foreground" />
                </div>
              ) : error ? (
                <div className="flex flex-col items-center gap-2 py-8 text-destructive">
                  <span className="text-sm">{error}</span>
                  <button
                    onClick={() => refetch()}
                    className="text-sm underline hover:no-underline"
                  >
                    Попробовать снова
                  </button>
                </div>
              ) : departments.length === 0 ? (
                <div className="flex items-center justify-center py-8 text-sm text-muted-foreground">
                  Ничего не найдено
                </div>
              ) : (
                <div role="listbox" className="py-1">
                  {departments.map((department) => (
                    <DepartmentSelectCard
                      key={department.id}
                      department={department}
                      isSelected={isSelected(department.id)}
                      multiselect={multiselect}
                      onSelect={(dep) =>
                        handleCheckedChange(!isSelected(dep.id), dep)
                      }
                    />
                  ))}

                  {/* Элемент для intersection observer (подгрузка следующих страниц) */}
                  <div
                    ref={cursorRef}
                    className="flex items-center justify-center py-2"
                  >
                    {isFetchingNextPage && (
                      <Loader2 className="size-4 animate-spin text-muted-foreground" />
                    )}
                  </div>
                </div>
              )}
            </ScrollArea>
          </div>
        </PopoverContent>
      </Popover>
    </div>
  );
};
