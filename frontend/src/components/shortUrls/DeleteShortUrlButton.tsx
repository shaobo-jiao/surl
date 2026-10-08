import { deleteShortUrlApi } from "@/api/shortUrls/deleteShortUrl";
import { type ShortUrl } from "@/api/shortUrls/shortUrl.shared";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogMedia, AlertDialogTitle, AlertDialogTrigger } from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { cn } from "@/lib/utils";
import { Trash2 } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

interface DeleteShortUrlButtonProps {
  shortUrl: ShortUrl;
  onDelete: (shortUrl: ShortUrl) => void;
  // classes for the trigger button that are relevant to the parent, e.g. layout
  className?: string;
}

// delete icon button with its confirmation dialog; 
// confirms before removing the short URL and notifies on success or failure
export default function DeleteShortUrlButton({ shortUrl, onDelete, className }: DeleteShortUrlButtonProps) {
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  const handleDelete = async () => {
    setIsDeleting(true);
    try {
      const deleteResult = await deleteShortUrlApi(shortUrl.id);
      if (!deleteResult.success) {
        toast.error("Failed deleting short URL", { description: deleteResult.error.detail });
        return;
      }
      // on success:
      onDelete(shortUrl);
      setIsOpen(false);
      toast.success("Short URL has been deleted");
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <AlertDialog open={isOpen} onOpenChange={setIsOpen}>
      <AlertDialogTrigger render={<Button type="button"
        variant="ghost"
        size="icon"
        aria-label="Delete short URL"
        className={cn("text-muted-foreground hover:text-destructive", className)}>
        <Trash2 />
      </Button>} />
      <AlertDialogContent size="sm">
        <AlertDialogHeader>
          <AlertDialogMedia className="bg-destructive/10 text-destructive dark:bg-destructive/20 dark:text-destructive">
            <Trash2 />
          </AlertDialogMedia>
          <AlertDialogTitle>Delete short URL?</AlertDialogTitle>
          <AlertDialogDescription>This will permanently delete this short URL.</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel
            variant="outline"
            disabled={isDeleting}>
            Cancel
          </AlertDialogCancel>
          <AlertDialogAction
            variant="destructive"
            disabled={isDeleting}
            onClick={handleDelete}>
            {isDeleting ? <Spinner /> : null} Delete
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
};
