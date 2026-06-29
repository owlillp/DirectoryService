"use client";

import { usePathname } from "next/navigation";
import Link from "next/link";
import { Home, PanelLeft } from "lucide-react";
import { routes, navItems } from "@/src/shared/routes";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarHeader,
  SidebarFooter,
  SidebarTrigger,
  useSidebar,
} from "@/src/shared/components/ui/sidebar";
import {
  Avatar,
  AvatarFallback,
  AvatarImage,
} from "@/src/shared/components/ui/avatar";

function NavLink({
  href,
  children,
  ...props
}: {
  href: string;
  children: React.ReactNode;
}) {
  const { isMobile, setOpenMobile } = useSidebar();

  return (
    <Link
      href={href}
      onClick={() => {
        if (isMobile) setOpenMobile(false);
      }}
      {...props}
    >
      {children}
    </Link>
  );
}

export default function AppSidebar() {
  const pathname = usePathname();
  const { state, isMobile } = useSidebar();

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader className="p-3">
        <SidebarMenu>
          <SidebarMenuItem>
            <div className="flex items-center gap-3 group-data-[collapsible=icon]:justify-center">
              <Avatar className="size-9 shrink-0 ring-1 ring-sidebar-border">
                <AvatarImage src="https://github.com/shadcn.png" alt="User" />
                <AvatarFallback>U</AvatarFallback>
              </Avatar>
              <div className="flex flex-col min-w-0 group-data-[collapsible=icon]:hidden">
                <span className="text-sm font-medium truncate text-sidebar-foreground">
                  Пользователь
                </span>
                <span className="text-xs truncate text-sidebar-foreground/50">
                  user@example.com
                </span>
              </div>
            </div>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarContent>
        {/* Свернуть / Развернуть */}
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              <SidebarMenuItem>
                <SidebarMenuButton
                  asChild
                  tooltip={{
                    children: "Развернуть",
                    hidden: state !== "collapsed" || isMobile,
                  }}
                >
                  <SidebarTrigger className="w-full justify-start gap-2 group-data-[collapsible=icon]:justify-center">
                    <PanelLeft className="h-4 w-4 shrink-0" />
                    <span className="group-data-[collapsible=icon]:hidden">
                      Свернуть
                    </span>
                  </SidebarTrigger>
                </SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        {/* Навигация */}
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu className="gap-0.5">
              <div className="mx-2 mb-2 h-0.5 rounded-full bg-sidebar-accent/70" />

              {/* Главная */}
              <SidebarMenuItem>
                <SidebarMenuButton
                  asChild
                  isActive={pathname === routes.home}
                  tooltip="Главная"
                >
                  <NavLink href={routes.home}>
                    <Home className="h-4 w-4 shrink-0" />
                    <span>Главная</span>
                  </NavLink>
                </SidebarMenuButton>
              </SidebarMenuItem>

              {navItems.map((item) => {
                const isActive = pathname === item.href;

                return (
                  <SidebarMenuItem key={item.href}>
                    <SidebarMenuButton
                      asChild
                      isActive={isActive}
                      tooltip={item.label}
                    >
                      <NavLink href={item.href}>
                        <item.icon className="h-4 w-4 shrink-0" />
                        <span>{item.label}</span>
                      </NavLink>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter className="p-3"></SidebarFooter>
    </Sidebar>
  );
}
