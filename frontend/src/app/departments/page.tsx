"use client";

import Link from "next/link";
import { routes } from "@/src/shared/routes";
import { useRootDepartments } from "@/src/shared/hooks/use-root-departments";
import { DepartmentCard } from "@/src/entities/departments/ui/department-card";
import { Button } from "@/src/shared/components/ui/button";
import { Card, CardContent } from "@/src/shared/components/ui/card";
import { Spinner } from "@/src/shared/components/ui/spinner";
import { AlertCircleIcon, RefreshCwIcon, FolderTreeIcon } from "lucide-react";

export default function DepartmentsPage() {
  const { departments, totalCount, isLoading, error, refetch } =
    useRootDepartments();

  return (
    <div className="flex flex-col flex-1 items-center p-8">
      <main className="flex flex-col w-full max-w-3xl gap-6">
        {/* Шапка */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Подразделения</h1>
            <p className="text-sm text-muted-foreground mt-1">
              Управление списком подразделений
            </p>
          </div>
          <Link
            href={routes.home}
            className="text-sm text-muted-foreground transition-colors hover:text-foreground"
          >
            ← На главную
          </Link>
        </div>

        {/* Загрузка */}
        {isLoading && (
          <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-border py-32 gap-4">
            <Spinner className="size-8 text-muted-foreground" />
            <p className="text-sm text-muted-foreground">
              Загрузка подразделений...
            </p>
          </div>
        )}

        {/* Ошибка */}
        {!isLoading && error && (
          <Card>
            <CardContent className="flex flex-col items-center justify-center py-16 gap-4">
              <div className="flex size-12 items-center justify-center rounded-full bg-destructive/10">
                <AlertCircleIcon className="size-6 text-destructive" />
              </div>
              <div className="text-center">
                <p className="font-medium text-foreground">Ошибка загрузки</p>
                <p className="text-sm text-muted-foreground mt-1 max-w-md">
                  {error}
                </p>
              </div>
              <Button variant="outline" onClick={refetch}>
                <RefreshCwIcon />
                Повторить
              </Button>
            </CardContent>
          </Card>
        )}

        {/* Пусто */}
        {!isLoading && !error && departments.length === 0 && (
          <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-border py-32 gap-4">
            <div className="flex size-12 items-center justify-center rounded-full bg-muted">
              <FolderTreeIcon className="size-6 text-muted-foreground" />
            </div>
            <div className="text-center">
              <p className="text-sm font-medium text-foreground">
                Нет подразделений
              </p>
              <p className="text-sm text-muted-foreground mt-1">
                Список корневых подразделений пуст
              </p>
            </div>
          </div>
        )}

        {/* Список */}
        {!isLoading && !error && departments.length > 0 && (
          <>
            <p className="text-sm text-muted-foreground">
              Всего подразделений: {totalCount}
            </p>

            <div className="flex flex-col gap-3">
              {departments.map((department) => (
                <DepartmentCard key={department.id} department={department} />
              ))}
            </div>
          </>
        )}
      </main>
    </div>
  );
}
