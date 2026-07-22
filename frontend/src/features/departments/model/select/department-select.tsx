import { SearchDepartment } from "@/src/entities/departments/types";
import { DepartmentListId } from "../department-list-store";

type DepartmentSelectProps = {
  selectedDepartments: SearchDepartment[];
  onChange: (selectedDepartments: SearchDepartment[]) => void;
  stateId: DepartmentListId;
  multiselect?: boolean;
  excludeIds?: string[];
};
