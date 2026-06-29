import Link from "next/link";
import { routes } from "@/src/shared/routes";

export const metadata = {
  title: "Позиции",
};

export default function PositionsPage() {
  return (
    <div className="flex flex-col flex-1 items-center justify-center p-8">
      <main className="flex flex-col w-full max-w-2xl gap-8">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Позиции</h1>
            <p className="text-sm text-muted-foreground mt-1">
              Управление списком позиций
            </p>
          </div>
          <Link
            href={routes.home}
            className="text-sm text-muted-foreground transition-colors hover:text-foreground"
          >
            ← На главную
          </Link>
        </div>
        <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-border py-32 text-muted-foreground">
          <p className="text-sm">Здесь будет список позиций</p>
        </div>
      </main>
    </div>
  );
}
