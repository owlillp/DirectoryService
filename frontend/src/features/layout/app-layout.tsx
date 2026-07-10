"use client";

import { queryClient } from "@/src/shared/api/query-client";
import { SidebarProvider } from "@/src/shared/components/ui/sidebar";
import { TooltipProvider } from "@/src/shared/components/ui/tooltip";
import {
  ErrorBoundary,
  DefaultFallback,
} from "@/src/shared/components/errorBoundary/error-boundary";
import { QueryClientProvider } from "@tanstack/react-query";
import Header from "../header/header";
import AppSidebar from "../sidebar/app-sidebar";
import { Toaster } from "sonner";

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
              <main className="flex flex-1 flex-col">
                <ErrorBoundary
                  FallbackComponent={DefaultFallback}
                  onError={(error, info) => {
                    console.error(
                      "[ErrorBoundary] Caught an error:",
                      error,
                      info,
                    );
                  }}
                >
                  {children}
                </ErrorBoundary>
              </main>
              <Toaster
                position="top-center"
                duration={3000}
                richColors={true}
              />
            </div>
          </div>
        </SidebarProvider>
      </TooltipProvider>
    </QueryClientProvider>
  );
}
