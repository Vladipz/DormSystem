import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import type { AuthUser } from "@/lib/types/auth";
import type { EventComment } from "@/lib/types/event";
import { useState } from "react";
import { CommentActionsMenu } from "./CommentActionsMenu";
import {
  canDeleteComment,
  canEditComment,
  getAuthorInitials,
  getAuthorName,
  getRelativeTime,
} from "./comment-utils";

interface CommentItemProps {
  comment: EventComment;
  currentUser: AuthUser | null;
  eventOwnerId: string;
  isDeleting: boolean;
  isUpdating: boolean;
  onDelete: (commentId: string) => void;
  onUpdate: (commentId: string, content: string) => void;
}

export function CommentItem({
  comment,
  currentUser,
  eventOwnerId,
  isDeleting,
  isUpdating,
  onDelete,
  onUpdate,
}: CommentItemProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [draft, setDraft] = useState(comment.content);
  const allowEdit = canEditComment(comment, currentUser);
  const allowDelete = canDeleteComment(comment, currentUser, eventOwnerId);
  const authorName = getAuthorName(comment.author, comment.authorUserId, currentUser?.id);
  const relativeTime = getRelativeTime(comment.createdAt);
  const wasEdited = !!comment.updatedAt;
  const canSave = draft.trim().length > 0 && draft.trim() !== comment.content.trim();

  const cancelEdit = () => {
    setDraft(comment.content);
    setIsEditing(false);
  };

  const saveEdit = () => {
    if (!canSave) {
      return;
    }

    onUpdate(comment.id, draft);
    setIsEditing(false);
  };

  return (
    <div className="flex gap-3">
      <Avatar className="h-10 w-10 shrink-0 border border-border bg-muted">
        <AvatarFallback className="text-xs font-semibold">
          {getAuthorInitials(comment.author, currentUser, comment.authorUserId)}
        </AvatarFallback>
      </Avatar>

      <div className="min-w-0 flex-1 rounded-2xl border border-border bg-card/70 p-4 shadow-sm">
        <div className="mb-2 flex items-start justify-between gap-3">
          <div className="min-w-0">
            <p className="truncate text-sm font-semibold text-foreground">
              {authorName}
              <span className="font-normal text-muted-foreground"> · {relativeTime}</span>
            </p>
            {wasEdited && <p className="text-xs text-muted-foreground">edited</p>}
          </div>
          <CommentActionsMenu
            canEdit={allowEdit}
            canDelete={allowDelete}
            isDeleting={isDeleting}
            onEdit={() => setIsEditing(true)}
            onDelete={() => onDelete(comment.id)}
          />
        </div>

        {isEditing ? (
          <div className="space-y-3">
            <Textarea
              value={draft}
              onChange={(event) => setDraft(event.target.value)}
              rows={3}
              maxLength={2000}
              disabled={isUpdating}
              className="resize-none rounded-xl border-border bg-background/70"
            />
            <div className="flex justify-end gap-2">
              <Button type="button" variant="ghost" onClick={cancelEdit} disabled={isUpdating}>
                Cancel
              </Button>
              <Button type="button" onClick={saveEdit} disabled={!canSave || isUpdating}>
                {isUpdating ? "Saving..." : "Save"}
              </Button>
            </div>
          </div>
        ) : (
          <p className="whitespace-pre-wrap break-words text-sm leading-6 text-foreground/90">
            {comment.content}
          </p>
        )}
      </div>
    </div>
  );
}
