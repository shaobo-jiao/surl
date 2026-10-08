import { Card, CardContent } from "@/components/ui/card";
import { Link2 } from "lucide-react";

// placeholder shown in place of the list when the user has no short URLs yet
export default function EmptyShortUrlsCard() {
  return (
    <Card className="w-full">
      <CardContent className="flex flex-col items-center gap-2 py-12">
        <Link2 className="size-8 text-muted-foreground" />
        <p className="font-medium">No short URLs yet</p>
        <p className="text-sm text-muted-foreground">
          Create your first short URL and it will show up here.
        </p>
      </CardContent>
    </Card>
  );
}
