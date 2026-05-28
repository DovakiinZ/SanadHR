"use client";

import { useState } from "react";
import { Send } from "lucide-react";
import { TaskComment } from "@/types";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Input } from "@/components/ui/input";

interface TaskCommentsProps {
  comments: TaskComment[];
  onAdd?: (comment: TaskComment) => void;
}

export function TaskComments({ comments, onAdd }: TaskCommentsProps) {
  const [text, setText] = useState("");

  const handleSend = () => {
    if (!text.trim() || !onAdd) return;
    const comment: TaskComment = {
      id: `cm-${Date.now()}`,
      author: "المستخدم الحالي",
      content: text.trim(),
      createdAt: new Date().toISOString(),
    };
    onAdd(comment);
    setText("");
  };

  const formatTime = (dateStr: string) => {
    const d = new Date(dateStr);
    return `${d.toLocaleDateString("ar-SA")} ${d.toLocaleTimeString("ar-SA", { hour: "2-digit", minute: "2-digit" })}`;
  };

  return (
    <div className="space-y-3">
      {/* Comments list */}
      <div className="space-y-3 max-h-60 overflow-y-auto">
        {comments.length === 0 && (
          <p className="text-sm text-muted-foreground text-center py-4">لا توجد تعليقات</p>
        )}
        {comments.map((comment) => (
          <div key={comment.id} className="flex gap-3">
            <Avatar className="h-7 w-7 flex-shrink-0">
              <AvatarFallback className="bg-card text-xs font-bold border border-border">
                {comment.author.charAt(0)}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium">{comment.author}</span>
                <span className="text-xs text-muted-foreground">{formatTime(comment.createdAt)}</span>
              </div>
              <p className="text-sm text-muted-foreground mt-0.5">{comment.content}</p>
            </div>
          </div>
        ))}
      </div>

      {/* New comment */}
      {onAdd && (
        <div className="flex items-center gap-2 pt-2 border-t border-border">
          <Input
            value={text}
            onChange={(e) => setText(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleSend()}
            placeholder="أضف تعليق..."
            className="h-9 bg-secondary border-border text-sm flex-1"
          />
          <button
            onClick={handleSend}
            disabled={!text.trim()}
            className="h-9 w-9 flex items-center justify-center text-muted-foreground hover:text-primary transition-colors disabled:opacity-50"
          >
            <Send className="h-4 w-4" />
          </button>
        </div>
      )}
    </div>
  );
}
