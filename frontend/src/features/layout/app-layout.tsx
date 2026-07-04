"use client";

import { queryClient } from "@/src/shared/api/query-client";
import { SidebarProvider } from "@/src/shared/components/ui/sidebar";
import { TooltipProvider } from "@/src/shared/components/ui/tooltip";
import { QueryClientProvider } from "@tanstack/react-query";
import Header from "../header/header";
import AppSidebar from "../sidebar/app-sidebar";

export default function Layout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <SidebarProvider defaultOpen={true}>
          <div className="flex min-h-svh w-full">
            <AppSidebar />
            <div className="flex flex-1 flex-col">
              <Header />
              <main className="flex flex-1 flex-col">{children}</main>
            </div>
          </div>
        </SidebarProvider>
      </TooltipProvider>
    </QueryClientProvider>
  );
}
