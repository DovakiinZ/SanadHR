"use client";

import { Users, Clock, Banknote, FileText } from "lucide-react";
import { StatCard } from "@/components/dashboard/stat-card";
import { EmployeeTable } from "@/components/employees/employee-table";
import {
  employees,
  departments,
  getActiveEmployeesCount,
  getAverageAttendanceRate,
  getTotalPayroll,
} from "@/lib/mock-data";

export default function DashboardPage() {
  const activeCount = getActiveEmployeesCount();
  const attendanceRate = getAverageAttendanceRate();
  const totalPayroll = getTotalPayroll();

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h1 className="text-2xl font-bold">لوحة التحكم</h1>
        <p className="text-sm text-muted-foreground mt-1">نظرة عامة على بيانات الموارد البشرية</p>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          title="إجمالي الموظفين"
          value={activeCount}
          icon={Users}
          change="+3 هذا الشهر"
          changeType="positive"
        />
        <StatCard
          title="نسبة الحضور"
          value={`${attendanceRate}%`}
          icon={Clock}
          change="+2% عن الشهر السابق"
          changeType="positive"
        />
        <StatCard
          title="الرواتب الشهرية"
          value={`${(totalPayroll / 1000).toFixed(0)}K ر.س`}
          icon={Banknote}
        />
        <StatCard
          title="الطلبات المعلقة"
          value={7}
          icon={FileText}
          change="3 طلبات جديدة"
          changeType="neutral"
        />
      </div>

      {/* Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Recent Employees */}
        <div className="lg:col-span-2">
          <div className="border border-border bg-card p-5">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-sm font-bold uppercase tracking-wider text-muted-foreground">
                آخر الموظفين المضافين
              </h2>
            </div>
            <EmployeeTable
              employees={employees}
              limit={5}
              showFilters={false}
              showPagination={false}
            />
          </div>
        </div>

        {/* Department Distribution */}
        <div className="border border-border bg-card p-5">
          <h2 className="text-sm font-bold uppercase tracking-wider text-muted-foreground mb-4">
            توزيع الأقسام
          </h2>
          <div className="space-y-3">
            {departments.map((dept) => {
              const percentage = Math.round(
                (dept.employeeCount / employees.length) * 100
              );
              return (
                <div key={dept.id}>
                  <div className="flex items-center justify-between text-sm mb-1">
                    <span>{dept.name}</span>
                    <span className="text-muted-foreground font-mono text-xs">
                      {dept.employeeCount}
                    </span>
                  </div>
                  <div className="h-1.5 bg-secondary">
                    <div
                      className="h-full bg-primary transition-all"
                      style={{ width: `${percentage}%` }}
                    />
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}
