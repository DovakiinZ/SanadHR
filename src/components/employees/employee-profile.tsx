"use client";

import { Users, Calendar, Banknote, Clock } from "lucide-react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Separator } from "@/components/ui/separator";
import { StatusBadge } from "./status-badge";
import { StatCard } from "@/components/dashboard/stat-card";
import { Employee } from "@/types";

interface EmployeeProfileProps {
  employee: Employee;
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between py-3 border-b border-border last:border-0">
      <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">{label}</span>
      <span className="text-sm">{value}</span>
    </div>
  );
}

export function EmployeeProfile({ employee }: EmployeeProfileProps) {
  const yearsOfService = Math.floor(
    (Date.now() - new Date(employee.joinDate).getTime()) / (365.25 * 24 * 60 * 60 * 1000)
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="border border-border bg-card p-6">
        <div className="flex items-center gap-6">
          <Avatar className="h-20 w-20">
            <AvatarFallback className="bg-primary text-primary-foreground text-2xl font-bold">
              {employee.name.charAt(0)}
            </AvatarFallback>
          </Avatar>
          <div className="flex-1">
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold">{employee.name}</h1>
              <StatusBadge status={employee.status} />
            </div>
            <p className="text-muted-foreground mt-1">{employee.position} — {employee.department}</p>
            <p className="text-xs text-muted-foreground mt-1 font-mono">{employee.employeeId}</p>
          </div>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard title="رصيد الإجازات" value={`${employee.leaveBalance} يوم`} icon={Calendar} />
        <StatCard title="نسبة الحضور" value={`${employee.attendanceRate}%`} icon={Clock} />
        <StatCard title="الراتب الشهري" value={`${employee.salary.toLocaleString()} ر.س`} icon={Banknote} />
        <StatCard title="سنوات الخدمة" value={yearsOfService} icon={Users} />
      </div>

      {/* Tabs */}
      <Tabs defaultValue="personal" className="w-full">
        <TabsList className="w-full justify-start bg-secondary border border-border h-10 p-0">
          <TabsTrigger value="personal" className="text-xs font-bold uppercase tracking-wider data-[state=active]:bg-card h-full px-6">
            البيانات الشخصية
          </TabsTrigger>
          <TabsTrigger value="employment" className="text-xs font-bold uppercase tracking-wider data-[state=active]:bg-card h-full px-6">
            بيانات التوظيف
          </TabsTrigger>
          <TabsTrigger value="documents" className="text-xs font-bold uppercase tracking-wider data-[state=active]:bg-card h-full px-6">
            المستندات
          </TabsTrigger>
          <TabsTrigger value="attendance" className="text-xs font-bold uppercase tracking-wider data-[state=active]:bg-card h-full px-6">
            سجل الحضور
          </TabsTrigger>
        </TabsList>

        <TabsContent value="personal" className="mt-4">
          <div className="border border-border bg-card p-6">
            <InfoRow label="الاسم الكامل" value={employee.name} />
            <InfoRow label="رقم الهوية" value={employee.nationalId} />
            <InfoRow label="البريد الإلكتروني" value={employee.email} />
            <InfoRow label="رقم الجوال" value={employee.phone} />
            <InfoRow label="تاريخ الميلاد" value={employee.dateOfBirth} />
            <InfoRow label="الجنس" value={employee.gender} />
            <InfoRow label="الجنسية" value={employee.nationality} />
          </div>
        </TabsContent>

        <TabsContent value="employment" className="mt-4">
          <div className="border border-border bg-card p-6">
            <InfoRow label="الرقم الوظيفي" value={employee.employeeId} />
            <InfoRow label="القسم" value={employee.department} />
            <InfoRow label="المسمى الوظيفي" value={employee.position} />
            <InfoRow label="تاريخ الانضمام" value={employee.joinDate} />
            <InfoRow label="نوع العقد" value={employee.contractType} />
            <InfoRow label="الراتب" value={`${employee.salary.toLocaleString()} ر.س`} />
            <InfoRow label="الحالة" value={employee.status} />
          </div>
        </TabsContent>

        <TabsContent value="documents" className="mt-4">
          <div className="border border-border bg-card p-12 text-center">
            <p className="text-muted-foreground text-sm">لا توجد مستندات مرفوعة</p>
          </div>
        </TabsContent>

        <TabsContent value="attendance" className="mt-4">
          <div className="border border-border bg-card p-6">
            <div className="space-y-0">
              {[...Array(5)].map((_, i) => {
                const date = new Date();
                date.setDate(date.getDate() - i - 1);
                const isWeekend = date.getDay() === 5 || date.getDay() === 6;
                return (
                  <div key={i} className="flex items-center justify-between py-3 border-b border-border last:border-0">
                    <span className="text-sm">{date.toLocaleDateString("ar-SA")}</span>
                    <span className={`text-xs font-bold uppercase tracking-wider ${isWeekend ? "text-muted-foreground" : "text-green-500"}`}>
                      {isWeekend ? "عطلة" : "حاضر"}
                    </span>
                    {!isWeekend && (
                      <span className="text-xs text-muted-foreground font-mono">08:00 - 17:00</span>
                    )}
                  </div>
                );
              })}
            </div>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
