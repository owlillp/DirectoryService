export type DepartmentWithChildren = {
  id: string;
  name: string;
  identifier: string;
  path: string;
  parentId?: string;
  depth: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  hasMoreChildren: boolean;
  children: DepartmentWithChildren[];
};

export type SearchDepartment = {
  id: string;
  name: string;
  identifier: string;
  path: string;
  parentId?: string;
  depth: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  deletedAt?: string;
};
