import Link from "next/link";
import { routes } from "@/src/shared/routes";

const sections = [
  {
    title: "Локации",
    description: "Управление списком локаций",
    href: routes.locations,
  },
  {
    title: "Подразделения",
    description: "Управление списком подразделений",
    href: routes.departments,
  },
  {
    title: "Позиции",
    description: "Управление списком позиций",
    href: routes.positions,
  },
];

export default function Home() {
  return (
    <div className="flex flex-col flex-1 items-center justify-center p-8">
      <main className="flex flex-col w-full max-w-2xl gap-10">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Directory Service
          </h1>
          <p className="text-sm text-muted-foreground mt-2">
            Выберите раздел для управления
          </p>
        </div>

        <div className="grid gap-4">
          {sections.map((section) => (
            <Link
              key={section.href}
              href={section.href}
              className="group flex items-center gap-4 rounded-xl border border-border bg-card p-5 transition-all hover:border-primary/30 hover:shadow-md"
            >
              <div className="flex flex-1 flex-col gap-1">
                <span className="text-base font-medium text-foreground group-hover:text-primary transition-colors">
                  {section.title}
                </span>
                <span className="text-sm text-muted-foreground">
                  {section.description}
                </span>
              </div>
              <svg
                className="h-5 w-5 text-muted-foreground transition-all group-hover:translate-x-0.5 group-hover:text-primary"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={2}
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M9 5l7 7-7 7"
                />
              </svg>
            </Link>
          ))}
        </div>
      </main>
    </div>
  );
}
