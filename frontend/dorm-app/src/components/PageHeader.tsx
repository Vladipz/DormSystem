import { Button } from "@/components/ui/button";
import { useNavigate } from "@tanstack/react-router";
import { ArrowLeft } from "lucide-react";

interface PageHeaderProps {
  title: string;
  backButtonLabel?: string;
  backTo?: string;
  children?: React.ReactNode;
  actions?: React.ReactNode;
}

export function PageHeader({
  title,
  backButtonLabel = "Back",
  backTo,
  children,
  actions,
}: PageHeaderProps) {
  const navigate = useNavigate();

  const handleBack = () => {
    if (backTo) {
      navigate({ to: backTo });
    } else {
      navigate({ to: "/" });
    }
  };

  return (
    <div className="flex items-center gap-4 mb-6 flex-wrap">
      <Button variant="ghost" onClick={handleBack}>
        <ArrowLeft className="mr-2 h-4 w-4" />
        {backButtonLabel}
      </Button>
      <h1 className="text-2xl font-bold">{title}</h1>
      {children && <div className="ml-auto">{children}</div>}
      {actions && <div className="ml-auto flex gap-2">{actions}</div>}
    </div>
  );
}
