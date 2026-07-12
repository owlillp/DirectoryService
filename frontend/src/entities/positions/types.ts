export type Position = {
  id: string;
  name: string;
  description?: string;
  departmentIds: string[];
  isActive: boolean;
  createdAt: string;
};
