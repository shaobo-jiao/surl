import { useState } from "react";
import { Check, Copy } from "lucide-react";

import { Button } from "@/components/ui/button";

interface CopyUrlButtonProps {
  value: string; // The URL copied to the clipboard
}

export default function CopyUrlButton({ value }: CopyUrlButtonProps) {
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
      onClick={handleCopy}>
      {copied ? <Check /> : <Copy />}
    </Button>
  );
}
