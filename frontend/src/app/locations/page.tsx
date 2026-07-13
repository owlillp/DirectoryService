"use client";

import Link from "next/link";
import { routes } from "@/src/shared/routes";
import { useLocationsList } from "@/src/features/locations/model/use-locations-list";
import { LocationCard } from "@/src/entities/locations/ui/location-card";
import { Button } from "@/src/shared/components/ui/button";
import { Card, CardContent } from "@/src/shared/components/ui/card";
import { Spinner } from "@/src/shared/components/ui/spinner";
import { MapPinIcon, AlertCircleIcon, RefreshCwIcon } from "lucide-react";
import { useState } from "react";
import { CreateLocationDialog } from "@/src/features/locations/create-location-dialog";

export default function LocationsPage() {
  const { locations, totalCount, isPending, error, refetch } =
    useLocationsList();
  const [open, setOpen] = useState(false);

  return (
    <>
      <title>Локации — Directory Service</title>
      <div className="flex flex-col flex-1 items-center p-8">
        <main className="flex flex-col w-full max-w-3xl gap-6">
          {/* Шапка */}
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Локации</h1>
              <p className="text-sm text-muted-foreground mt-1">
                Управление списком локаций
              </p>
            </div>
            <div className="flex items-center gap-3">
              <Link
                href={routes.home}
                className="text-sm text-muted-foreground transition-colors hover:text-foreground"
              >
                ← На главную
              </Link>
              <CreateLocationDialog open={open} onOpenChange={setOpen} />
            </div>
          </div>

          {/* Загрузка */}
          {isPending && (
            <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-border py-32 gap-4">
              <Spinner className="size-8 text-muted-foreground" />
              <p className="text-sm text-muted-foreground">
                Загрузка локаций...
              </p>
            </div>
          )}

          {/* Ошибка */}
          {!isPending && error && (
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
          {!isPending && !error && locations.length === 0 && (
            <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-border py-32 gap-4">
              <div className="flex size-12 items-center justify-center rounded-full bg-muted">
                <MapPinIcon className="size-6 text-muted-foreground" />
              </div>
              <div className="text-center">
                <p className="text-sm font-medium text-foreground">
                  Нет локаций
                </p>
                <p className="text-sm text-muted-foreground mt-1">
                  Список локаций пуст
                </p>
              </div>
            </div>
          )}

          {/* Список */}
          {!isPending && !error && locations.length > 0 && (
            <>
              <p className="text-sm text-muted-foreground">
                Всего локаций: {totalCount}
              </p>

              <div className="flex flex-col gap-3">
                {locations.map((location) => (
                  <LocationCard key={location.id} location={location} />
                ))}
              </div>
            </>
          )}
        </main>
      </div>
    </>
  );
}
