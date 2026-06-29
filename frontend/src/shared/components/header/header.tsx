"use client";

import Link from "next/link";
import { routes } from "@/src/shared/routes";
import { SidebarTrigger } from "@/src/shared/components/ui/sidebar";

export default function Header() {
  return (
    <header className="sticky top-0 z-40 flex h-14 items-center gap-4 border-b border-border bg-background/95 backdrop-blur-sm px-4 lg:px-6">
      <SidebarTrigger className="lg:hidden" />

      <Link
        href={routes.home}
        className="flex items-center gap-2 transition-opacity hover:opacity-80"
      >
        <div className="flex h-7 w-7 items-center justify-center rounded-md bg-primary text-xs font-bold text-primary-foreground">
          DS
        </div>
        <span className="text-sm font-semibold">Directory Service</span>
      </Link>
    </header>
  );
}
