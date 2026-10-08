import { Spinner } from "@/components/ui/spinner";
import { cn } from "@/lib/utils";

interface PageLoaderProps {
  // extra classes for the centering wrapper, e.g. a height or vertical padding
  className?: string;
}

// a centered spinner for page-level loading states
export default function PageLoader({ className }: PageLoaderProps) {
  return (
    <div className={cn("flex w-full items-center justify-center", className)}>
      <Spinner className="size-8" />
    </div>
  );
}
