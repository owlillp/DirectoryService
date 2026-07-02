"use client";

import Link from "next/link";
import { routes } from "@/src/shared/routes";
import { useLocations } from "@/src/shared/hooks/use-locations";
import { Button } from "@/src/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/src/shared/components/ui/card";
import { Badge } from "@/src/shared/components/ui/badge";
import { Spinner } from "@/src/shared/components/ui/spinner";
import {
  MapPinIcon,
  AlertCircleIcon,
  BuildingIcon,
  GlobeIcon,
  RefreshCwIcon,
} from "lucide-react";

export default function LocationsPage() {
  const { locations, totalCount, isLoading, error, refetch } = useLocations();

  return (
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
            <p className="text-sm text-muted-foreground">Загрузка локаций...</p>
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
        {!isLoading && !error && locations.length === 0 && (
          <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-border py-32 gap-4">
            <div className="flex size-12 items-center justify-center rounded-full bg-muted">
              <MapPinIcon className="size-6 text-muted-foreground" />
            </div>
            <div className="text-center">
              <p className="text-sm font-medium text-foreground">Нет локаций</p>
              <p className="text-sm text-muted-foreground mt-1">
                Список локаций пуст
              </p>
            </div>
          </div>
        )}

        {/* Список */}
        {!isLoading && !error && locations.length > 0 && (
          <>
            <p className="text-sm text-muted-foreground">
              Всего локаций: {totalCount}
            </p>

            <div className="flex flex-col gap-3">
              {locations.map((location) => (
                <Card key={location.id} size="sm">
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <MapPinIcon className="size-4 text-muted-foreground shrink-0" />
                      <span>{location.name}</span>
                      <Badge
                        variant={location.isActive ? "default" : "secondary"}
                        className="ml-auto"
                      >
                        {location.isActive ? "Активна" : "Неактивна"}
                      </Badge>
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="flex flex-col gap-1.5 text-muted-foreground">
                    <div className="flex items-center gap-2">
                      <BuildingIcon className="size-3.5 shrink-0" />
                      <span>
                        {location.address.country}, {location.address.city},{" "}
                        {location.address.street},{" "}
                        {location.address.buildingNumber}
                        {location.address.apartment &&
                          `, ${location.address.apartment}`}
                      </span>
                    </div>
                    <div className="flex items-center gap-2">
                      <GlobeIcon className="size-3.5 shrink-0" />
                      <span>Часовой пояс: {location.timeZone}</span>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </>
        )}
      </main>
    </div>
  );
}
