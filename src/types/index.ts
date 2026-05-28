export interface Employee {
  id: string;
  employeeId: string;
  name: string;
  email: string;
  phone: string;
  nationalId: string;
  dateOfBirth: string;
  gender: "ذكر" | "أنثى";
  nationality: string;
  department: string;
  position: string;
  joinDate: string;
  contractType: "دوام كامل" | "دوام جزئي" | "عقد مؤقت";
  salary: number;
  status: "نشط" | "إجازة" | "منتهي العقد";
  avatar?: string;
  leaveBalance: number;
  attendanceRate: number;
}

export interface Department {
  id: string;
  name: string;
  employeeCount: number;
  manager: string;
}

export interface NavItem {
  label: string;
  href: string;
  icon: string;
}
