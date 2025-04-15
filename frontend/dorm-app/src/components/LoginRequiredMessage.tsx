import { Button } from "@/components/ui/button";
import { useNavigate } from "@tanstack/react-router";
import { AlertCircle, LogIn } from "lucide-react";

interface LoginRequiredMessageProps {
  returnTo: string;
}

export function LoginRequiredMessage({ returnTo }: LoginRequiredMessageProps) {
  const navigate = useNavigate();
  return (
    <div className="mx-auto p-4 sm:p-6 max-w-full sm:max-w-2xl">
      <div className="bg-orange-100 border border-orange-400 text-orange-700 px-4 py-5 sm:py-8 rounded mb-4 flex flex-col items-center text-center">
        <AlertCircle className="h-8 w-8 sm:h-10 sm:w-10 mb-3 sm:mb-4" />
        <h3 className="text-base sm:text-lg font-medium mb-1 sm:mb-2">
          Authentication Required
        </h3>
        <p className="text-center text-sm sm:text-base mb-4 sm:mb-6">
          You need to sign in to edit events.
        </p>
        <Button
          onClick={() =>
            navigate({ to: "/login", search: { returnTo } })
          }
          className="w-full sm:w-auto"
        >
          <LogIn className="mr-2 h-4 w-4" /> Sign In
        </Button>
      </div>
    </div>
  );
} 