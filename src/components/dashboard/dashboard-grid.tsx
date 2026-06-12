"use client";

import { useMemo } from "react";
import ReactGridLayout from "react-grid-layout";
import "react-grid-layout/css/styles.css";
import "react-resizable/css/styles.css";
import { DashboardWidget, WidgetFilterSpec, parseWidgetSpec } from "@/types/dashboard";
import { WidgetCard } from "./widget-card";

const ResponsiveGridLayout = ReactGridLayout.WidthProvider(ReactGridLayout.Responsive);

interface GridItem { i: string; x: number; y: number; w: number; h: number; minW?: number; minH?: number }

interface DashboardGridProps {
  widgets: DashboardWidget[];
  filters?: WidgetFilterSpec[];
  editMode?: boolean;
  refreshKey?: number;
  onEditWidget?: (w: DashboardWidget) => void;
  onDeleteWidget?: (w: DashboardWidget) => void;
  onPersistLayout?: (layouts: { widgetId: string; column: number; row: number; width: number; height: number }[]) => void;
}

export function DashboardGrid({ widgets, filters, editMode, refreshKey, onEditWidget, onDeleteWidget, onPersistLayout }: DashboardGridProps) {
  const layout = useMemo<GridItem[]>(() => {
    let cursorX = 0, cursorY = 0, rowH = 0;
    return widgets.map((w) => {
      const l = w.layout;
      if (l && (l.width || l.height)) {
        return { i: w.id, x: l.column ?? 0, y: l.row ?? 0, w: l.width || 4, h: l.height || 3, minW: 2, minH: 2 };
      }
      // auto-flow for widgets without a saved layout
      const wgt = 4, hgt = 3;
      if (cursorX + wgt > 12) { cursorX = 0; cursorY += rowH; rowH = 0; }
      const pos = { i: w.id, x: cursorX, y: cursorY, w: wgt, h: hgt, minW: 2, minH: 2 };
      cursorX += wgt; rowH = Math.max(rowH, hgt);
      return pos;
    });
  }, [widgets]);

  const persist = (l: GridItem[]) => {
    if (!editMode || !onPersistLayout) return;
    onPersistLayout(l.map((it) => ({ widgetId: it.i, column: it.x, row: it.y, width: it.w, height: it.h })));
  };

  if (widgets.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center gap-2 border border-dashed border-border py-20 text-center text-muted-foreground">
        <p>لا توجد عناصر في هذه اللوحة بعد</p>
      </div>
    );
  }

  return (
    <ResponsiveGridLayout
      className="layout"
      layouts={{ lg: layout, md: layout, sm: layout }}
      breakpoints={{ lg: 1100, md: 768, sm: 0 }}
      cols={{ lg: 12, md: 12, sm: 1 }}
      rowHeight={68}
      margin={[12, 12]}
      isDraggable={!!editMode}
      isResizable={!!editMode}
      draggableHandle=".widget-drag"
      onDragStop={persist}
      onResizeStop={persist}
      compactType="vertical"
    >
      {widgets.map((w) => {
        const spec = parseWidgetSpec(w);
        return (
          <div key={w.id}>
            {spec ? (
              <WidgetCard
                widgetId={w.id}
                spec={spec}
                visualization={spec.visualization || w.widgetType}
                titleAr={w.titleAr}
                dashboardFilters={filters}
                editMode={editMode}
                refreshKey={refreshKey}
                dragHandleClass="widget-drag"
                onEdit={onEditWidget ? () => onEditWidget(w) : undefined}
                onDelete={onDeleteWidget ? () => onDeleteWidget(w) : undefined}
              />
            ) : (
              <div className="flex h-full items-center justify-center border border-border bg-card text-sm text-muted-foreground">
                إعداد العنصر غير صالح
              </div>
            )}
          </div>
        );
      })}
    </ResponsiveGridLayout>
  );
}
