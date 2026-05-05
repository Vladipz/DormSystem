import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import type { AuthUser } from "@/lib/types/auth";
import { getAuthorInitials } from "./comment-utils";

interface CommentComposerProps {
  currentUser: AuthUser | null;
  isAuthenticated: boolean;
  canComment: boolean;
  disabledReason: string;
  isPosting: boolean;
  value: string;
  onChange: (value: string) => void;
  onSubmit: () => void;
}

export function CommentComposer({
  currentUser,
  isAuthenticated,
  canComment,
  disabledReason,
  isPosting,
  value,
  onChange,
  onSubmit,
}: CommentComposerProps) {
  const isSubmitDisabled = isPosting || value.trim().length === 0 || !canComment;

  return (
    <div className="flex gap-3 pt-1">
      <Avatar className="h-10 w-10 shrink-0 border border-border bg-muted">
        <AvatarFallback className="text-xs font-semibold">
          {currentUser ? getAuthorInitials(undefined, currentUser, currentUser.id) : "GU"}
        </AvatarFallback>
      </Avatar>

      <div className="min-w-0 flex-1 space-y-3">
        <Textarea
          value={value}
          onChange={(event) => onChange(event.target.value)}
          placeholder={canComment ? "Add a comment..." : disabledReason}
          rows={2}
          maxLength={2000}
          disabled={isPosting || !canComment}
          className="min-h-11 resize-none rounded-xl border-border bg-background/70"
        />
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <p className="text-xs text-muted-foreground">
            {canComment
              ? `${value.length}/2000 characters`
              : isAuthenticated
                ? disabledReason
                : "Only signed-in users can comment."}
          </p>
          <Button
            type="button"
            onClick={onSubmit}
            disabled={isSubmitDisabled}
            className="sm:self-end"
          >
            {isPosting ? "Posting..." : "Post Comment"}
          </Button>
        </div>
      </div>
    </div>
  );
}
