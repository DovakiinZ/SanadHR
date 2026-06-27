"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { ArrowRight, Search, Loader2, UserPlus, Link2, Unlink, CheckCircle2 } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Combobox } from "@/components/ui/combobox";
import { ApiError } from "@/lib/api-client";
import { AccessGuard } from "@/components/access/access-guard";
import { getEmployees } from "@/lib/api/employees";
import type { Employee } from "@/types";
import {
  listUsers, listRoles, createUserFromEmployee, linkEmployee, unlinkEmployee,
  STATUS_AR, type UserListItem, type RoleDto,
} from "@/lib/api/access";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

function empName(e: Employee) { return e.name; }

export default function EmployeeAccountsPage() {
  return <AccessGuard anyOf={["Settings.ManageUsers"]}><Inner /></AccessGuard>;
}

function Inner() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [createFor, setCreateFor] = useState<Employee | null>(null);
  const [linkFor, setLinkFor] = useState<Employee | null>(null);

  const load = useCallback(async () => {
    try { const [e, u] = await Promise.all([getEmployees(), listUsers()]); setEmployees(e); setUsers(u); }
    catch (err) { notifyError(err, "تعذر تحميل البيانات"); }
  }, []);

  useEffect(() => {
    (async () => {
      setLoading(true);
      try { const [e, u, r] = await Promise.all([getEmployees(), listUsers(), listRoles()]); setEmployees(e); setUsers(u); setRoles(r); }
      catch (err) { notifyError(err, "تعذر تحميل البيانات"); } finally { setLoading(false); }
    })();
  }, []);

  const userByEmp = useMemo(() => {
    const m = new Map<string, UserListItem>();
    for (const u of users) if (u.employeeId) m.set(u.employeeId, u);
    return m;
  }, [users]);

  const unlinkedUsers = useMemo(() => users.filter((u) => !u.employeeId), [users]);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return employees;
    return employees.filter((e) => empName(e).toLowerCase().includes(q) || (e.employeeId ?? "").toLowerCase().includes(q));
  }, [employees, search]);

  async function doUnlink(u: UserListItem) {
    try { await unlinkEmployee(u.id); toast.success("تم فك الربط"); await load(); }
    catch (err) { notifyError(err, "تعذر فك الربط"); }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/access" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> المستخدمون والصلاحيات
        </Link>
        <span className="text-muted-foreground">/</span><span>حسابات الموظفين</span>
      </div>

      <div>
        <h1 className="text-2xl font-bold">حسابات الموظفين</h1>
        <p className="text-sm text-muted-foreground mt-1">الموظف سجل في الموارد البشرية؛ المستخدم هوية دخول. اربط بينهما أو أنشئ حساباً للموظف.</p>
      </div>

      <div className="relative max-w-sm">
        <Search className="absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="بحث باسم الموظف أو رقمه…" className="pr-9 bg-secondary border-border" />
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              {["الموظف", "الرقم الوظيفي", "حالة الحساب", ""].map((h, i) => (
                <TableHead key={i} className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">{h}</TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={4} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل…</TableCell></TableRow>
            ) : filtered.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={4} className="py-12 text-center text-sm text-muted-foreground">لا يوجد موظفون</TableCell></TableRow>
            ) : filtered.map((e) => {
              const u = userByEmp.get(e.id);
              return (
                <TableRow key={e.id} className="border-border hover:bg-card/50">
                  <TableCell className="font-medium">{empName(e)}</TableCell>
                  <TableCell className="text-sm text-muted-foreground">{e.employeeId}</TableCell>
                  <TableCell>
                    {u ? (
                      <span className="inline-flex items-center gap-2 text-sm">
                        <CheckCircle2 className="h-4 w-4 text-green-600" />
                        <span dir="ltr" className="text-muted-foreground">{u.email}</span>
                        <Badge variant="outline" className="text-[10px]">{STATUS_AR[u.status] ?? u.status}</Badge>
                      </span>
                    ) : <span className="text-sm text-muted-foreground">لا يوجد حساب</span>}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center justify-end gap-1">
                      {u ? (
                        <button onClick={() => doUnlink(u)} className="h-8 px-2 inline-flex items-center gap-1 text-xs text-destructive hover:text-destructive/80"><Unlink className="h-3.5 w-3.5" /> فك الربط</button>
                      ) : (
                        <>
                          <button onClick={() => setCreateFor(e)} className="h-8 px-2 inline-flex items-center gap-1 text-xs text-primary hover:underline"><UserPlus className="h-3.5 w-3.5" /> إنشاء حساب</button>
                          <button onClick={() => setLinkFor(e)} className="h-8 px-2 inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground"><Link2 className="h-3.5 w-3.5" /> ربط حساب</button>
                        </>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </div>

      {createFor && <CreateAccountDialog employee={createFor} roles={roles} onClose={() => setCreateFor(null)} onDone={load} />}
      {linkFor && <LinkAccountDialog employee={linkFor} users={unlinkedUsers} onClose={() => setLinkFor(null)} onDone={load} />}
    </div>
  );
}

function CreateAccountDialog({ employee, roles, onClose, onDone }: { employee: Employee; roles: RoleDto[]; onClose: () => void; onDone: () => void; }) {
  const [email, setEmail] = useState(employee.email ?? "");
  const [roleIds, setRoleIds] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);

  async function save() {
    if (!email.trim()) { toast.error("البريد مطلوب"); return; }
    setSaving(true);
    try {
      await createUserFromEmployee({ employeeId: employee.id, email: email.trim(), roleIds: roleIds.length ? roleIds : undefined });
      toast.success("تم إنشاء الحساب وإرسال دعوة التفعيل");
      onClose(); onDone();
    } catch (err) { notifyError(err, "تعذر إنشاء الحساب"); } finally { setSaving(false); }
  }

  return (
    <Dialog open onOpenChange={(o) => { if (!o && !saving) onClose(); }}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader><DialogTitle>إنشاء حساب — {empName(employee)}</DialogTitle></DialogHeader>
        <div className="space-y-4 py-2">
          <div className="space-y-2"><Label className="text-xs font-bold uppercase tracking-wider">البريد الإلكتروني</Label>
            <Input value={email} onChange={(e) => setEmail(e.target.value)} dir="ltr" className="bg-secondary border-border" /></div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الأدوار</Label>
            <div className="flex flex-wrap gap-2">
              {roles.map((r) => {
                const on = roleIds.includes(r.id);
                return <button key={r.id} type="button" onClick={() => setRoleIds((p) => on ? p.filter((x) => x !== r.id) : [...p, r.id])}
                  className={`px-3 h-8 text-sm border ${on ? "bg-primary text-primary-foreground border-primary" : "border-border hover:bg-muted"}`}>{r.nameAr || r.name}</button>;
              })}
            </div>
          </div>
          <p className="text-xs text-muted-foreground">سيتم إنشاء الحساب بحالة «مدعو» وإرسال رابط تفعيل لتعيين كلمة المرور.</p>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={saving}>إلغاء</Button>
          <Button onClick={save} disabled={saving} className="font-bold">{saving ? "جاري الإنشاء…" : "إنشاء وإرسال دعوة"}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function LinkAccountDialog({ employee, users, onClose, onDone }: { employee: Employee; users: UserListItem[]; onClose: () => void; onDone: () => void; }) {
  const [userId, setUserId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const options = useMemo(() => users.map((u) => ({ value: u.id, label: u.fullName, hint: u.email })), [users]);

  async function save() {
    if (!userId) { toast.error("اختر مستخدماً"); return; }
    setSaving(true);
    try { await linkEmployee(userId, employee.id); toast.success("تم الربط"); onClose(); onDone(); }
    catch (err) { notifyError(err, "تعذر الربط"); } finally { setSaving(false); }
  }

  return (
    <Dialog open onOpenChange={(o) => { if (!o && !saving) onClose(); }}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader><DialogTitle>ربط حساب — {empName(employee)}</DialogTitle></DialogHeader>
        <div className="space-y-4 py-2">
          <div className="space-y-2"><Label className="text-xs font-bold uppercase tracking-wider">اختر مستخدماً غير مرتبط</Label>
            <Combobox value={userId} onChange={setUserId} options={options} placeholder="اختر مستخدماً…" allowClear /></div>
          {users.length === 0 && <p className="text-xs text-muted-foreground">لا يوجد مستخدمون غير مرتبطين.</p>}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={saving}>إلغاء</Button>
          <Button onClick={save} disabled={saving} className="font-bold">{saving ? "جاري الربط…" : "ربط"}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
