import { Home, MapPin, Building2, Briefcase } from "lucide-react";
import type { LucideIcon } from "lucide-react";

export const routes = {
  home: "/",
  locations: "/locations",
  positions: "/positions",
  departments: "/departments",
} as const;

export type RouteKey = keyof typeof routes;

export type NavItem = {
  label: string;
  href: string;
  icon: LucideIcon;
};

export const navItems: NavItem[] = [
  { label: "Главная", href: routes.home, icon: Home },
  { label: "Локации", href: routes.locations, icon: MapPin },
  { label: "Подразделения", href: routes.departments, icon: Building2 },
  { label: "Позиции", href: routes.positions, icon: Briefcase },
];
