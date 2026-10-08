import { useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import { Spinner } from "@/components/ui/spinner";
import { Button } from "@/components/ui/button";


interface LogoutButtonProps {
  // extra styling so the button can also be used as a menu item in the small-screen navigation
  className?: string;
  // called right before logging out, e.g. to close the navigation menu
  onClick?: () => void;
}

export default function LogoutButton({ className, onClick }: LogoutButtonProps) {
  const [isLoggingOut, setIsLoggingOut] = useState<boolean>(false);
  const { logout } = useAuth();

  const handleClick = async () => {
    setIsLoggingOut(true);
    onClick?.();
    await logout();
    setIsLoggingOut(false);
  };

  return isLoggingOut
    ? <Button variant="link" className={className} disabled> <Spinner /> Logout</Button>
    : <Button variant="link" className={className} onClick={handleClick}>Logout</Button>;
}
