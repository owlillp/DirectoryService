import type { Metadata } from "next";
import { DM_Sans, JetBrains_Mono } from "next/font/google";
import "./globals.css";
import { SidebarProvider } from "@/src/shared/components/ui/sidebar";
import { TooltipProvider } from "@/src/shared/components/ui/tooltip";
import AppSidebar from "@/src/shared/components/sidebar/app.sidebar";
import Header from "@/src/shared/components/header/header";

const dmSans = DM_Sans({
  variable: "--font-sans",
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
});

const jetbrainsMono = JetBrains_Mono({
  variable: "--font-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Directory Service",
  description: "Управление справочником",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="dark">
      <body
        className={`${dmSans.variable} ${jetbrainsMono.variable} h-full antialiased`}
      >
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
      </body>
    </html>
  );
}
