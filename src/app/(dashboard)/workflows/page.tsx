"use client";

import { useState } from "react";
import { GitBranch, Inbox, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { usePermissions } from "@/lib/permissions";
import { DefinitionsPanel } from "@/components/workflows/DefinitionsPanel";
import { RequestsPanel } from "@/components/workflows/RequestsPanel";
import { WorkflowBuilder } from "@/components/workflows/builder/WorkflowBuilder";
import { getWorkflowDefinition, type WorkflowDefinitionDto } from "@/lib/api/workflow-builder";

export default function WorkflowsPage() {
  const { has } = usePermissions();
  const canManage = has("Workflows.Create") || has("Workflows.Edit");
  const canApprove = has("Workflows.Edit");

  const [tab, setTab] = useState<"defs" | "reqs">("defs");
  const [builderDef, setBuilderDef] = useState<WorkflowDefinitionDto | null>(null);
  const [opening, setOpening] = useState(false);
  const [reload, setReload] = useState(0);

  async function openBuilder(id: string) {
    setOpening(true);
    try {
      setBuilderDef(await getWorkflowDefinition(id));
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "تعذر فتح المُصمّم");
    } finally {
      setOpening(false);
    }
  }

  const tabs = [
    { key: "defs" as const, label: "المسارات", icon: GitBranch },
    { key: "reqs" as const, label: "الطلبات", icon: Inbox },
  ];

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-xl font-semibold">مسارات العمل</h1>
        <p className="text-sm text-muted-foreground">صمّم سلاسل الموافقات ونفّذها وتتبّع طلباتها.</p>
      </div>

      <div className="flex gap-1 border-b border-border">
        {tabs.map((t) => {
          const Icon = t.icon;
          return (
            <button key={t.key} onClick={() => setTab(t.key)}
              className={`flex items-center gap-1.5 border-b-2 px-3 py-2 text-sm transition-colors ${
                tab === t.key ? "border-primary text-foreground" : "border-transparent text-muted-foreground hover:text-foreground"
              }`}>
              <Icon className="h-4 w-4" /> {t.label}
            </button>
          );
        })}
        {opening && <Loader2 className="my-auto ms-2 h-4 w-4 animate-spin text-muted-foreground" />}
      </div>

      {tab === "defs" ? (
        <DefinitionsPanel
          canManage={canManage}
          reloadToken={reload}
          onOpen={openBuilder}
          onStarted={() => { setTab("reqs"); setReload((x) => x + 1); }}
        />
      ) : (
        <RequestsPanel canApprove={canApprove} reloadToken={reload} />
      )}

      {builderDef && (
        <WorkflowBuilder
          def={builderDef}
          canEdit={canManage}
          onClose={() => setBuilderDef(null)}
          onSaved={() => { setBuilderDef(null); setReload((x) => x + 1); }}
        />
      )}
    </div>
  );
}
