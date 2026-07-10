"use client";

import { Button } from "@/src/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/src/shared/components/ui/card";
import { AlertCircleIcon, RefreshCwIcon } from "lucide-react";
import { ErrorBoundary } from "react-error-boundary";
import type { FallbackProps } from "react-error-boundary";

export { ErrorBoundary };

export function DefaultFallback({ error, resetErrorBoundary }: FallbackProps) {
  return (
    <div className="flex flex-col items-center justify-center flex-1 p-8">
      <Card className="max-w-md w-full">
        <CardHeader>
          <div className="flex items-center gap-3">
            <div className="flex size-10 items-center justify-center rounded-full bg-destructive/10">
              <AlertCircleIcon className="size-5 text-destructive" />
            </div>
            <CardTitle className="text-lg">Что-то пошло не так</CardTitle>
          </div>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <p className="text-sm text-muted-foreground">
            {error instanceof Error
              ? error.message
              : "Произошла непредвиденная ошибка"}
          </p>
          <Button
            variant="outline"
            onClick={resetErrorBoundary}
            className="w-fit"
          >
            <RefreshCwIcon />
            Попробовать снова
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
