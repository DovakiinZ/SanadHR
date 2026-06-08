"use client";

import { useState, useMemo } from "react";
import Link from "next/link";
import { Search, Filter, MoreHorizontal, Eye, Pencil, Trash2, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { StatusBadge } from "./status-badge";
import { Employee } from "@/types";
import { deleteEmployee } from "@/lib/api/employees";
import { ApiError } from "@/lib/api-client";

interface EmployeeTableProps {
  employees: Employee[];
  limit?: number;
  showFilters?: boolean;
  showPagination?: boolean;
  loading?: boolean;
  onChanged?: () => void;
}

export function EmployeeTable({
  employees: allEmployees,
  limit,
  showFilters = true,
  showPagination = true,
  loading = false,
  onChanged,
}: EmployeeTableProps) {
  const [search, setSearch] = useState("");
  const [departmentFilter, setDepartmentFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [page, setPage] = useState(1);
  const [deleteTarget, setDeleteTarget] = useState<Employee | null>(null);
  const [deleting, setDeleting] = useState(false);
  const perPage = 10;

  // Filter options derived from live data (no mock dependency).
  const departmentOptions = useMemo(
    () => Array.from(new Set(allEmployees.map((e) => e.department).filter((d) => d && d !== "—"))),
    [allEmployees]
  );
  const statusOptions = useMemo(
    () => Array.from(new Set(allEmployees.map((e) => e.status).filter(Boolean))),
    [allEmployees]
  );

  const filtered = useMemo(() => {
    let result = allEmployees;
    if (search) {
      result = result.filter(
        (e) =>
          e.name.includes(search) ||
          e.employeeId.toLowerCase().includes(search.toLowerCase()) ||
          e.position.includes(search)
      );
    }
    if (departmentFilter) {
      result = result.filter((e) => e.department === departmentFilter);
    }
    if (statusFilter) {
      result = result.filter((e) => e.status === statusFilter);
    }
    return result;
  }, [allEmployees, search, departmentFilter, statusFilter]);

  const displayed = limit
    ? filtered.slice(0, limit)
    : filtered.slice((page - 1) * perPage, page * perPage);

  const totalPages = Math.ceil(filtered.length / perPage);

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteEmployee(deleteTarget.id);
      toast.success(`تم حذف الموظف ${deleteTarget.name}`);
      setDeleteTarget(null);
      onChanged?.();
    } catch (err) {
      if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
        toast.error(err instanceof ApiError ? err.message : "تعذر حذف الموظف");
      }
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div>
      {showFilters && (
        <div className="mb-4 flex flex-wrap items-center gap-3">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="بحث بالاسم أو الرقم الوظيفي..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1); }}
              className="pr-10 bg-secondary border-border h-9 text-sm"
            />
          </div>
          <select
            value={departmentFilter}
            onChange={(e) => { setDepartmentFilter(e.target.value); setPage(1); }}
            className="h-9 bg-secondary border border-border px-3 text-sm text-foreground"
          >
            <option value="">كل الأقسام</option>
            {departmentOptions.map((d) => (
              <option key={d} value={d}>{d}</option>
            ))}
          </select>
          <select
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
            className="h-9 bg-secondary border border-border px-3 text-sm text-foreground"
          >
            <option value="">كل الحالات</option>
            {statusOptions.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
          <div className="flex items-center gap-1 text-xs text-muted-foreground">
            <Filter className="h-3.5 w-3.5" />
            <span>{filtered.length} نتيجة</span>
          </div>
        </div>
      )}

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الموظف</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الرقم الوظيفي</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">القسم</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">المسمى الوظيفي</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الحالة</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">تاريخ الانضمام</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground w-12"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="border-border hover:bg-transparent">
                <TableCell colSpan={7} className="py-12 text-center text-sm text-muted-foreground">
                  <span className="inline-flex items-center gap-2">
                    <Loader2 className="h-4 w-4 animate-spin" /> جاري التحميل...
                  </span>
                </TableCell>
              </TableRow>
            ) : displayed.length === 0 ? (
              <TableRow className="border-border hover:bg-transparent">
                <TableCell colSpan={7} className="py-12 text-center text-sm text-muted-foreground">
                  لا يوجد موظفون
                </TableCell>
              </TableRow>
            ) : (
              displayed.map((employee) => (
                <TableRow key={employee.id} className="border-border hover:bg-card/50">
                  <TableCell>
                    <Link href={`/employees/${employee.id}`} className="flex items-center gap-3 hover:text-primary transition-colors">
                      <Avatar className="h-8 w-8">
                        <AvatarFallback className="bg-card text-xs font-bold border border-border">
                          {employee.name.charAt(0)}
                        </AvatarFallback>
                      </Avatar>
                      <span className="font-medium">{employee.name}</span>
                    </Link>
                  </TableCell>
                  <TableCell className="text-muted-foreground text-sm font-mono">{employee.employeeId}</TableCell>
                  <TableCell className="text-sm">{employee.department}</TableCell>
                  <TableCell className="text-sm text-muted-foreground">{employee.position}</TableCell>
                  <TableCell><StatusBadge status={employee.status} /></TableCell>
                  <TableCell className="text-sm text-muted-foreground">{employee.joinDate}</TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger className="inline-flex h-8 w-8 items-center justify-center text-muted-foreground hover:text-foreground transition-colors">
                        <MoreHorizontal className="h-4 w-4" />
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end" className="w-40">
                        <DropdownMenuItem render={<Link href={`/employees/${employee.id}`} />} className="flex items-center gap-2">
                          <Eye className="h-4 w-4" /> عرض الملف
                        </DropdownMenuItem>
                        <DropdownMenuItem render={<Link href={`/employees/${employee.id}/edit`} />} className="flex items-center gap-2">
                          <Pencil className="h-4 w-4" /> تعديل
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          onClick={() => setDeleteTarget(employee)}
                          className="flex items-center gap-2 text-destructive"
                        >
                          <Trash2 className="h-4 w-4" /> حذف
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {showPagination && !limit && totalPages > 1 && (
        <div className="mt-4 flex items-center justify-between">
          <p className="text-xs text-muted-foreground">
            عرض {(page - 1) * perPage + 1} - {Math.min(page * perPage, filtered.length)} من {filtered.length}
          </p>
          <div className="flex items-center gap-1">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage(page - 1)}
              disabled={page === 1}
              className="h-8 text-xs"
            >
              السابق
            </Button>
            {Array.from({ length: totalPages }, (_, i) => (
              <Button
                key={i + 1}
                variant={page === i + 1 ? "default" : "outline"}
                size="sm"
                onClick={() => setPage(i + 1)}
                className="h-8 w-8 text-xs"
              >
                {i + 1}
              </Button>
            ))}
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage(page + 1)}
              disabled={page === totalPages}
              className="h-8 text-xs"
            >
              التالي
            </Button>
          </div>
        </div>
      )}

      {/* Delete confirmation */}
      <Dialog open={!!deleteTarget} onOpenChange={(open) => { if (!open && !deleting) setDeleteTarget(null); }}>
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>حذف موظف</DialogTitle>
            <DialogDescription>
              هل أنت متأكد من حذف الموظف{" "}
              <span className="font-bold text-foreground">{deleteTarget?.name}</span>؟ لا يمكن التراجع عن هذا الإجراء.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)} disabled={deleting}>
              إلغاء
            </Button>
            <Button
              onClick={confirmDelete}
              disabled={deleting}
              className="bg-destructive text-white hover:bg-destructive/90"
            >
              {deleting ? "جاري الحذف..." : "حذف"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
