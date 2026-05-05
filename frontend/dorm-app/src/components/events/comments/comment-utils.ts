import type { AuthUser } from "@/lib/types/auth";
import type { EventComment, EventCommentAuthor } from "@/lib/types/event";

export function getAuthorName(
  author: EventCommentAuthor | null | undefined,
  authorUserId: string,
  currentUserId: string | undefined,
) {
  if (currentUserId === authorUserId) {
    return "You";
  }

  if (!author) {
    return "Unknown user";
  }

  return `${author.firstName} ${author.lastName}`.trim();
}

export function getAuthorInitials(
  author: EventCommentAuthor | null | undefined,
  currentUser: AuthUser | null,
  authorUserId: string,
) {
  const firstName =
    currentUser?.id === authorUserId ? currentUser.firstName : author?.firstName;
  const lastName =
    currentUser?.id === authorUserId ? currentUser.lastName : author?.lastName;

  const initials = `${firstName?.[0] ?? ""}${lastName?.[0] ?? ""}`.toUpperCase();

  if (initials.length > 0) {
    return initials;
  }

  return "??";
}

export function getRelativeTime(dateString: string) {
  const date = new Date(dateString);
  const seconds = Math.max(0, Math.floor((Date.now() - date.getTime()) / 1000));

  if (seconds < 60) {
    return "just now";
  }

  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) {
    return `${minutes} ${minutes === 1 ? "minute" : "minutes"} ago`;
  }

  const hours = Math.floor(minutes / 60);
  if (hours < 24) {
    return `${hours} ${hours === 1 ? "hour" : "hours"} ago`;
  }

  const days = Math.floor(hours / 24);
  if (days < 7) {
    return `${days} ${days === 1 ? "day" : "days"} ago`;
  }

  return date.toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    year: date.getFullYear() === new Date().getFullYear() ? undefined : "numeric",
  });
}

export function canEditComment(
  comment: EventComment,
  currentUser: AuthUser | null,
) {
  return !!currentUser && (comment.authorUserId === currentUser.id || currentUser.role === "Admin");
}

export function canDeleteComment(
  comment: EventComment,
  currentUser: AuthUser | null,
  eventOwnerId: string,
) {
  return !!currentUser && (
    comment.authorUserId === currentUser.id ||
    eventOwnerId === currentUser.id ||
    currentUser.role === "Admin"
  );
}
