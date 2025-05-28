import { NavigationItems } from "@/components/NavigationItems";
import { Avatar, AvatarFallback, AvatarImage } from "./ui";

export function AppSidebar() {
  return (
    <div className="bg-card hidden w-64 border-r p-4 shadow-lg md:block">
      <div className="flex h-full flex-col">
        <div className="mb-8 ml-8 flex items-center justify-start gap-2.5">
          <Avatar>
            <AvatarImage
              src="/WhiteLogo.svg"
              alt="InDorm Logo"
              className="h-8 w-8"
            />

            <AvatarFallback className="bg-primary/10 text-primary/80">
              inDorm
            </AvatarFallback>
          </Avatar>
          <h1 className="text-xl font-bold">InDorm</h1>
        </div>

        <NavigationItems className="flex-1" />
      </div>
    </div>
  );
}
