import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useAuth } from "@/lib/hooks/useAuth";
import { EventService } from "@/lib/services/eventService";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { MessageSquare } from "lucide-react";
import { useState } from "react";
import { CommentComposer } from "./CommentComposer";
import { CommentItem } from "./CommentItem";
import { CommentPagination } from "./CommentPagination";
import { CommentSkeleton } from "./CommentSkeleton";

const COMMENTS_PAGE_SIZE = 10;

interface EventDiscussionSectionProps {
  eventId: string;
  eventOwnerId: string;
  canComment: boolean;
  disabledReason: string;
}

export function EventDiscussionSection({
  eventId,
  eventOwnerId,
  canComment,
  disabledReason,
}: EventDiscussionSectionProps) {
  const queryClient = useQueryClient();
  const { user, isAuthenticated } = useAuth();
  const [pageNumber, setPageNumber] = useState(1);
  const [commentText, setCommentText] = useState("");

  const commentsQuery = useQuery({
    queryKey: ["events", eventId, "comments", pageNumber, COMMENTS_PAGE_SIZE],
    queryFn: () =>
      EventService.getEventComments(eventId, pageNumber, COMMENTS_PAGE_SIZE),
    staleTime: 1000 * 30,
  });

  const invalidateComments = () => {
    queryClient.invalidateQueries({
      queryKey: ["events", eventId, "comments"],
    });
  };

  const createMutation = useMutation({
    mutationFn: (content: string) =>
      EventService.createEventComment(eventId, content),
    onSuccess: () => {
      setCommentText("");
      setPageNumber(1);
      invalidateComments();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({
      commentId,
      content,
    }: {
      commentId: string;
      content: string;
    }) => EventService.updateEventComment(eventId, commentId, content),
    onSuccess: invalidateComments,
  });

  const deleteMutation = useMutation({
    mutationFn: (commentId: string) =>
      EventService.deleteEventComment(eventId, commentId),
    onSuccess: invalidateComments,
  });

  const submitComment = () => {
    if (!canComment) {
      return;
    }

    const trimmedComment = commentText.trim();
    if (trimmedComment.length === 0) {
      return;
    }

    createMutation.mutate(trimmedComment);
  };

  const mutationError =
    createMutation.error ?? updateMutation.error ?? deleteMutation.error;
  const commentsPage = commentsQuery.data;

  return (
    <Card className="border-border/80 bg-card/95 shadow-sm">
      <CardHeader className="pb-3">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <CardTitle className="flex items-center gap-2 text-xl">
            <MessageSquare className="text-primary h-5 w-5" />
            Discussion
          </CardTitle>
          {commentsPage && commentsPage.totalCount > 0 && (
            <CommentPagination
              commentsPage={commentsPage}
              onPageChange={setPageNumber}
            />
          )}
        </div>
      </CardHeader>
      <CardContent className="space-y-5">
        {commentsQuery.isLoading ? (
          <CommentSkeleton />
        ) : commentsQuery.isError ? (
          <div className="border-destructive/30 bg-destructive/10 text-destructive rounded-xl border p-4 text-sm">
            Comments could not be loaded. The event details are still available.
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="text-destructive hover:text-destructive ml-2"
              onClick={() => commentsQuery.refetch()}
            >
              Retry
            </Button>
          </div>
        ) : commentsPage && commentsPage.items.length > 0 ? (
          <div className="space-y-4">
            {commentsPage.items.map((comment) => (
              <CommentItem
                key={comment.id}
                comment={comment}
                currentUser={user}
                eventOwnerId={eventOwnerId}
                isDeleting={
                  deleteMutation.isPending &&
                  deleteMutation.variables === comment.id
                }
                isUpdating={
                  updateMutation.isPending &&
                  updateMutation.variables?.commentId === comment.id
                }
                onDelete={(commentId) => deleteMutation.mutate(commentId)}
                onUpdate={(commentId, content) =>
                  updateMutation.mutate({ commentId, content: content.trim() })
                }
              />
            ))}
          </div>
        ) : (
          <div className="border-border rounded-2xl border border-dashed p-8 text-center">
            <p className="font-medium">No comments yet</p>
            <p className="text-muted-foreground mt-1 text-sm">
              Be the first to start the discussion for this event.
            </p>
          </div>
        )}

        <div className="bg-border h-px" />

        {mutationError && (
          <div className="border-destructive/30 bg-destructive/10 text-destructive rounded-xl border p-3 text-sm">
            {mutationError instanceof Error
              ? mutationError.message
              : "Comment action failed."}
          </div>
        )}

        <CommentComposer
          currentUser={user}
          isAuthenticated={isAuthenticated}
          canComment={canComment}
          disabledReason={disabledReason}
          isPosting={createMutation.isPending}
          value={commentText}
          onChange={setCommentText}
          onSubmit={submitComment}
        />
      </CardContent>
    </Card>
  );
}
