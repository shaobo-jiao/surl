import { useState } from "react";
import { Check, Copy } from "lucide-react";

import { Button } from "@/components/ui/button";
import { cn } from "cn";

interface CopyUrlButtonProps {
  value: string; // The URL copied to the clipboard
  className?: string, 
}

export default function CopyUrlButton({ value, className }: CopyUrlButtonProps) {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    await navigator.clipboard.writeText(value);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <Button
      type="button"
      variant="ghost"
      size="icon"
      aria-label="Copy URL"
      onClick={handleCopy}
      className={cn(className)}>
      {copied ? <Check /> : <Copy />}
    </Button>
  );
}
